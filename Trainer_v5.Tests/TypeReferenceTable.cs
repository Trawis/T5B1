using System;
using System.Collections.Generic;
using System.Text;

namespace Trainer_v5.Tests
{
	// Reads the TypeRef table of a managed assembly straight from its metadata (ECMA-335 II.22.38): the list of types the
	// image refers to, which is what the game's DLL mod loader checks. Nothing is loaded or executed.
	internal static class TypeReferenceTable
	{
		private const int ModuleTable = 0;
		private const int TypeRefTable = 1;
		private const int ModuleRefTable = 0x1A;
		private const int AssemblyRefTable = 0x23;

		public static IList<string> Read(byte[] image)
		{
			var metadata = LocateMetadata(image);
			var root = metadata;
			var afterVersion = root + 16 + BitConverter.ToInt32(image, root + 12);
			var streamCount = BitConverter.ToUInt16(image, afterVersion + 2);
			var position = afterVersion + 4;

			var tables = -1;
			var strings = -1;
			for (var i = 0; i < streamCount; i++)
			{
				var offset = BitConverter.ToInt32(image, position);
				var nameStart = position + 8;
				var nameEnd = nameStart;
				while (image[nameEnd] != 0)
				{
					nameEnd++;
				}

				var name = Encoding.ASCII.GetString(image, nameStart, nameEnd - nameStart);
				if (name == "#~" || name == "#-")
				{
					tables = root + offset;
				}
				else if (name == "#Strings")
				{
					strings = root + offset;
				}

				position = nameStart + ((nameEnd - nameStart + 1 + 3) & ~3);
			}

			if (tables < 0 || strings < 0)
			{
				throw new InvalidOperationException("The assembly has no metadata tables or string heap.");
			}

			var heapSizes = image[tables + 6];
			var stringIndex = (heapSizes & 0x01) != 0 ? 4 : 2;
			var guidIndex = (heapSizes & 0x02) != 0 ? 4 : 2;
			var valid = BitConverter.ToUInt64(image, tables + 8);
			var rowCounts = new int[64];
			var cursor = tables + 24;
			for (var table = 0; table < 64; table++)
			{
				if ((valid & (1UL << table)) != 0)
				{
					rowCounts[table] = BitConverter.ToInt32(image, cursor);
					cursor += 4;
				}
			}

			// ResolutionScope is a coded index over Module, ModuleRef, AssemblyRef and TypeRef (2 tag bits).
			var scopeIndex = Math.Max(Math.Max(rowCounts[ModuleTable], rowCounts[ModuleRefTable]), Math.Max(rowCounts[AssemblyRefTable], rowCounts[TypeRefTable])) < (1 << 14) ? 2 : 4;
			var moduleRow = 2 + stringIndex + 3 * guidIndex;
			var typeRefRow = scopeIndex + 2 * stringIndex;
			var typeRefStart = cursor + rowCounts[ModuleTable] * moduleRow;

			var references = new List<string>();
			for (var row = 0; row < rowCounts[TypeRefTable]; row++)
			{
				var entry = typeRefStart + row * typeRefRow + scopeIndex;
				var name = ReadString(image, strings, Index(image, entry, stringIndex));
				var space = ReadString(image, strings, Index(image, entry + stringIndex, stringIndex));
				references.Add(space.Length == 0 ? name : space + "." + name);
			}
			return references;
		}

		private static int Index(byte[] image, int offset, int size)
		{
			return size == 2 ? BitConverter.ToUInt16(image, offset) : BitConverter.ToInt32(image, offset);
		}

		private static string ReadString(byte[] image, int heap, int index)
		{
			var end = heap + index;
			while (image[end] != 0)
			{
				end++;
			}
			return Encoding.UTF8.GetString(image, heap + index, end - heap - index);
		}

		// File offset of the metadata root (the "BSJB" header), found through the CLR header's MetaData directory.
		private static int LocateMetadata(byte[] image)
		{
			var pe = BitConverter.ToInt32(image, 0x3C);
			if (BitConverter.ToUInt32(image, pe) != 0x00004550)
			{
				throw new InvalidOperationException("Not a PE image.");
			}

			var sectionCount = BitConverter.ToUInt16(image, pe + 6);
			var optionalHeaderSize = BitConverter.ToUInt16(image, pe + 20);
			var optionalHeader = pe + 24;
			var directories = optionalHeader + (BitConverter.ToUInt16(image, optionalHeader) == 0x20B ? 112 : 96);
			var clrHeaderRva = BitConverter.ToInt32(image, directories + 14 * 8);
			if (clrHeaderRva == 0)
			{
				throw new InvalidOperationException("Not a managed assembly.");
			}

			var sections = optionalHeader + optionalHeaderSize;
			var clrHeader = ToFileOffset(image, sections, sectionCount, clrHeaderRva);
			var metadata = ToFileOffset(image, sections, sectionCount, BitConverter.ToInt32(image, clrHeader + 8));
			if (BitConverter.ToUInt32(image, metadata) != 0x424A5342)
			{
				throw new InvalidOperationException("The metadata header was not found.");
			}
			return metadata;
		}

		private static int ToFileOffset(byte[] image, int sections, int sectionCount, int rva)
		{
			for (var i = 0; i < sectionCount; i++)
			{
				var section = sections + i * 40;
				var virtualSize = BitConverter.ToInt32(image, section + 8);
				var virtualAddress = BitConverter.ToInt32(image, section + 12);
				var rawSize = BitConverter.ToInt32(image, section + 16);
				var rawPointer = BitConverter.ToInt32(image, section + 20);
				if (rva >= virtualAddress && rva < virtualAddress + Math.Max(virtualSize, rawSize))
				{
					return rva - virtualAddress + rawPointer;
				}
			}

			throw new InvalidOperationException("RVA 0x" + rva.ToString("x") + " is not inside any section.");
		}
	}
}

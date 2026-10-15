using System;

namespace Trainer_v5.Tests
{
	// Just enough of the PE/COFF format to read and clear the CLR "requires 32-bit process" flag.
	// The game's mcs.dll carries it; a 64-bit test host refuses such an assembly with a
	// BadImageFormatException even though the code is plain IL (the game's other assemblies are not marked).
	internal static class PeImage
	{
		private const int ClrDataDirectory = 14;
		private const int FlagsFieldInClrHeader = 16;
		private const byte Requires32BitFlag = 0x02;

		public static bool Requires32BitProcess(byte[] image)
		{
			return (image[ClrFlagsOffset(image)] & Requires32BitFlag) != 0;
		}

		// A copy of the image with that one flag cleared; every other byte is unchanged.
		public static byte[] WithoutRequires32BitProcess(byte[] image)
		{
			var copy = (byte[])image.Clone();
			copy[ClrFlagsOffset(image)] &= unchecked((byte)~Requires32BitFlag);
			return copy;
		}

		private static int ClrFlagsOffset(byte[] image)
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
			var clrHeaderRva = BitConverter.ToInt32(image, directories + ClrDataDirectory * 8);
			if (clrHeaderRva == 0)
			{
				throw new InvalidOperationException("Not a managed assembly.");
			}

			var sections = optionalHeader + optionalHeaderSize;
			for (var i = 0; i < sectionCount; i++)
			{
				var section = sections + i * 40;
				var virtualSize = BitConverter.ToInt32(image, section + 8);
				var virtualAddress = BitConverter.ToInt32(image, section + 12);
				var rawSize = BitConverter.ToInt32(image, section + 16);
				var rawPointer = BitConverter.ToInt32(image, section + 20);
				if (clrHeaderRva >= virtualAddress && clrHeaderRva < virtualAddress + Math.Max(virtualSize, rawSize))
				{
					return clrHeaderRva - virtualAddress + rawPointer + FlagsFieldInClrHeader;
				}
			}

			throw new InvalidOperationException("The CLR header is not inside any section.");
		}
	}
}

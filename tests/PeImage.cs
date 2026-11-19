using System;

namespace Trainer_v5.Tests
{
	// Reads and clears the CLR "requires 32-bit process" flag, which only the game's mcs.dll carries.
	internal static class PeImage
	{
		private const int ClrDataDirectory = 14;
		private const int FlagsFieldInClrHeader = 16;
		private const byte Requires32BitFlag = 0x02;

		public static bool Requires32BitProcess(byte[] image)
		{
			return (image[ClrFlagsOffset(image)] & Requires32BitFlag) != 0;
		}

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

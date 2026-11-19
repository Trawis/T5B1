using System;
using System.IO;

namespace Trainer_v5.Tests
{
	internal static class CompilerImageCache
	{
		public static string Ensure(string directory, string fileName, byte[] image)
		{
			var path = Path.Combine(directory, fileName);
			if (HasExactContent(path, image))
				return path;

			Directory.CreateDirectory(directory);
			var temporary = Path.Combine(directory, fileName + "." + Guid.NewGuid().ToString("n") + ".tmp");
			try
			{
				File.WriteAllBytes(temporary, image);
				Promote(temporary, path, image);
			}
			finally
			{
				DeleteQuietly(temporary);
			}
			return path;
		}

		public static bool HasExactContent(string path, byte[] expected)
		{
			try
			{
				var info = new FileInfo(path);
				if (!info.Exists || info.Length != expected.Length)
					return false;

				var actual = File.ReadAllBytes(path);
				if (actual.Length != expected.Length)
					return false;

				for (var i = 0; i < expected.Length; i++)
				{
					if (actual[i] != expected[i])
						return false;
				}
				return true;
			}
			catch (IOException)
			{
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		private static void Promote(string temporary, string path, byte[] image)
		{
			try
			{
				if (File.Exists(path))
					File.Replace(temporary, path, null);
				else
					File.Move(temporary, path);
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
				if (!HasExactContent(path, image))
					throw;
			}
		}

		private static void DeleteQuietly(string path)
		{
			try
			{
				File.Delete(path);
			}
			catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
			{
			}
		}
	}
}

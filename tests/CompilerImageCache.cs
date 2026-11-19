using System;
using System.IO;

namespace Trainer_v5.Tests
{
	// Publishes the patched game-compiler copy that concurrent test hosts share, and refuses to trust
	// a cached file unless its bytes equal the expected image (equal length is not equal content).
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

				// Compare bytes; a same-length stale or damaged file must not be loaded.
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
				// Being replaced or written by another host right now; treat as not yet valid.
				return false;
			}
			catch (UnauthorizedAccessException)
			{
				return false;
			}
		}

		// A complete temporary file is moved into place in one step, so readers never observe a partial copy.
		// If another host wins the race, its file is accepted only when it matches the expected content.
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
				// A leftover uniquely named temporary file is harmless.
			}
		}
	}
}

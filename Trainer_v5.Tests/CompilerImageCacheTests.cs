using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Trainer_v5.Tests
{
	public class CompilerImageCacheTests : IDisposable
	{
		private readonly string _directory = Path.Combine(Path.GetTempPath(), "t5b1-cache-test-" + Guid.NewGuid().ToString("n"));

		public void Dispose()
		{
			if (Directory.Exists(_directory))
				Directory.Delete(_directory, true);
		}

		[Fact]
		public void Ensure_WritesMissingFile()
		{
			var image = new byte[] { 1, 2, 3, 4 };

			var path = CompilerImageCache.Ensure(_directory, "mcs.dll", image);

			Assert.Equal(image, File.ReadAllBytes(path));
			Assert.Single(Directory.GetFiles(_directory));
		}

		[Fact]
		public void Ensure_ReplacesSameLengthFileWithWrongBytes()
		{
			var image = new byte[] { 1, 2, 3, 4 };
			Directory.CreateDirectory(_directory);
			File.WriteAllBytes(Path.Combine(_directory, "mcs.dll"), new byte[] { 1, 2, 9, 4 });

			var path = CompilerImageCache.Ensure(_directory, "mcs.dll", image);

			Assert.Equal(image, File.ReadAllBytes(path));
			Assert.Single(Directory.GetFiles(_directory));
		}

		[Fact]
		public void Ensure_ReplacesTruncatedFile()
		{
			var image = new byte[] { 1, 2, 3, 4 };
			Directory.CreateDirectory(_directory);
			File.WriteAllBytes(Path.Combine(_directory, "mcs.dll"), new byte[] { 1, 2 });

			var path = CompilerImageCache.Ensure(_directory, "mcs.dll", image);

			Assert.Equal(image, File.ReadAllBytes(path));
		}

		[Fact]
		public void Ensure_LeavesMatchingFileUntouched()
		{
			var image = new byte[] { 1, 2, 3, 4 };
			var path = CompilerImageCache.Ensure(_directory, "mcs.dll", image);
			var stamp = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			File.SetLastWriteTimeUtc(path, stamp);

			CompilerImageCache.Ensure(_directory, "mcs.dll", image);

			Assert.Equal(stamp, File.GetLastWriteTimeUtc(path));
		}

		[Fact]
		public void HasExactContent_RejectsMissingAndMismatchedFiles()
		{
			Directory.CreateDirectory(_directory);
			var path = Path.Combine(_directory, "mcs.dll");
			Assert.False(CompilerImageCache.HasExactContent(path, new byte[] { 1 }));

			File.WriteAllBytes(path, new byte[] { 2 });
			Assert.False(CompilerImageCache.HasExactContent(path, new byte[] { 1 }));
			Assert.True(CompilerImageCache.HasExactContent(path, new byte[] { 2 }));
		}

		[Fact]
		public async Task Ensure_ConcurrentCallersAllEndWithTheExactImage()
		{
			var image = new byte[64 * 1024];
			for (var i = 0; i < image.Length; i++)
				image[i] = (byte)(i * 31);

			var tasks = new Task[8];
			for (var i = 0; i < tasks.Length; i++)
				tasks[i] = Task.Run(() => CompilerImageCache.Ensure(_directory, "mcs.dll", image));
			await Task.WhenAll(tasks);

			Assert.Equal(image, File.ReadAllBytes(Path.Combine(_directory, "mcs.dll")));
			Assert.Single(Directory.GetFiles(_directory));
		}
	}
}

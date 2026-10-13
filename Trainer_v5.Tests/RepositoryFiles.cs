using System;
using System.IO;

namespace Trainer_v5.Tests
{
	internal static class RepositoryFiles
	{
		public static readonly string Root = FindRoot();

		public static string SourceDirectory => Path.Combine(Root, "Trainer_v5", "Trainer.Source");

		private static string FindRoot()
		{
			var directory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while (directory != null)
			{
				if (File.Exists(Path.Combine(directory.FullName, "Trainer v5 - Beta 1.sln")))
				{
					return directory.FullName;
				}
				directory = directory.Parent;
			}
			throw new InvalidOperationException("Could not locate the repository root from " + AppDomain.CurrentDomain.BaseDirectory);
		}
	}
}

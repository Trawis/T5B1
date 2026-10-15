using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The game rejects a DLL mod that references a prohibited type ("Security Check Failed (TypeReferenceRestriction)").
	public class DllModTypeRestrictionTests
	{
		[Theory]
		[InlineData("SINetworking")]
		[InlineData("System.IO.")]
		[InlineData("System.Reflection.")]
		[InlineData("UnityEngine.WWW")]
		[InlineData("using System.IO;")]
		[InlineData("using System.Reflection;")]
		public void TheTrainerSourceNeverNamesATypeTheDllModLoaderProhibits(string forbidden)
		{
			var offenders = new List<string>();
			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				if (File.ReadAllText(file).Contains(forbidden))
				{
					offenders.Add(Path.GetFileName(file));
				}
			}

			Assert.True(offenders.Count == 0, forbidden + " found in: " + string.Join(", ", offenders));
		}
	}
}

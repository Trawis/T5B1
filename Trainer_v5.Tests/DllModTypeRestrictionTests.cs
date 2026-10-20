using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// The game rejects a DLL mod that references a prohibited type ("Security Check Failed (TypeReferenceRestriction)").
	// The loader looks at the compiled assembly, so this reads the TypeRef table of Trainer_v5.dll instead of searching
	// source text; comments, aliases and `var` can neither hide nor fake a hit.
	public class DllModTypeRestrictionTests
	{
		private static readonly string[] ProhibitedNamespaces = { "SINetworking", "System.IO", "System.Reflection" };
		private static readonly string[] ProhibitedTypes = { "UnityEngine.WWW" };

		// The SDK stamps these assembly-level attributes into every build (GenerateAssemblyInfo), so they come with the
		// project and not with any source line; the old source scan never saw them. Anything else in System.Reflection
		// (a call, a field, a signature) still fails. Whether the loader itself tolerates them is not verified here.
		private static readonly string[] BuildStampedAttributes =
		{
			"System.Reflection.AssemblyCompanyAttribute",
			"System.Reflection.AssemblyConfigurationAttribute",
			"System.Reflection.AssemblyFileVersionAttribute",
			"System.Reflection.AssemblyInformationalVersionAttribute",
			"System.Reflection.AssemblyProductAttribute",
			"System.Reflection.AssemblyTitleAttribute"
		};

		[Fact]
		public void TheCompiledTrainerReferencesNoTypeTheDllModLoaderProhibits()
		{
			var references = TypeReferenceTable.Read(File.ReadAllBytes(typeof(FeatureRegistry).Assembly.Location));
			var offenders = Prohibited(references).FindAll(reference => Array.IndexOf(BuildStampedAttributes, reference) < 0);

			Assert.True(references.Count > 100, "Read only " + references.Count + " type references from Trainer_v5.dll; the metadata reader is broken.");
			Assert.True(offenders.Count == 0, "Trainer_v5.dll refers to prohibited types: " + string.Join(", ", offenders));
		}

		[Fact]
		public void TheReaderFindsTheTypeReferencesOfAnAssemblyThatDoesUseThem()
		{
			// Guards the reader: this test assembly uses System.IO.File and the game's own UnityEngine types.
			var references = TypeReferenceTable.Read(File.ReadAllBytes(typeof(DllModTypeRestrictionTests).Assembly.Location));
			var offenders = Prohibited(references);

			Assert.Contains("System.IO.File", offenders);
			Assert.Contains("System.IO.Path", offenders);
			Assert.Contains("System.Reflection.MethodBase", offenders);
			Assert.Contains("System.String", references);
		}

		[Theory]
		[InlineData("System.IO.File", true)]
		[InlineData("System.IO.Compression.GZipStream", true)]
		[InlineData("System.Reflection.Assembly", true)]
		[InlineData("SINetworking.NetworkManager", true)]
		[InlineData("UnityEngine.WWW", true)]
		[InlineData("System.IOException", false)]
		[InlineData("System.Collections.Generic.List`1", false)]
		[InlineData("UnityEngine.WWWForm", false)]
		public void OnlyTheProhibitedNamespacesAndTypesAreRejected(string reference, bool prohibited)
		{
			Assert.Equal(prohibited, Prohibited(new[] { reference }).Count == 1);
		}

		private static List<string> Prohibited(IEnumerable<string> references)
		{
			var offenders = new List<string>();
			foreach (var reference in references)
			{
				var prohibited = Array.IndexOf(ProhibitedTypes, reference) >= 0;
				foreach (var prohibitedNamespace in ProhibitedNamespaces)
				{
					prohibited |= reference.StartsWith(prohibitedNamespace + ".", StringComparison.Ordinal);
				}

				if (prohibited && !offenders.Contains(reference))
				{
					offenders.Add(reference);
				}
			}

			offenders.Sort(StringComparer.Ordinal);
			return offenders;
		}
	}
}

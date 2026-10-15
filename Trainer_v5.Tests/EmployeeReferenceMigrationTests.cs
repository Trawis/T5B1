using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	// The collection helpers behind the Creativity migration (#183), exercised with plain keys.
	public class EmployeeReferenceMigrationTests
	{
		[Fact]
		public void ADictionaryKeyMovesToTheNewKeyWithItsValue()
		{
			var dictionary = new Dictionary<string, float> { { "old", 0.5f }, { "other", 1f } };

			Assert.True(EmployeeReferenceMigration.ReplaceKeyOf(dictionary, "old", "new"));

			Assert.False(dictionary.ContainsKey("old"));
			Assert.Equal(0.5f, dictionary["new"]);
			Assert.Equal(1f, dictionary["other"]);
		}

		[Fact]
		public void AMissingKeyOrDictionaryChangesNothing()
		{
			var dictionary = new Dictionary<string, float> { { "other", 1f } };

			Assert.False(EmployeeReferenceMigration.ReplaceKeyOf(dictionary, "old", "new"));
			Assert.False(EmployeeReferenceMigration.ReplaceKeyOf<string, float>(null, "old", "new"));
			Assert.Single(dictionary);
		}

		[Fact]
		public void EveryOccurrenceInAListIsReplacedInPlace()
		{
			var list = new List<string> { "a", "old", "b", "old" };

			Assert.True(EmployeeReferenceMigration.ReplaceInList(list, "old", "new"));

			Assert.Equal(new[] { "a", "new", "b", "new" }, list.ToArray());
			Assert.False(EmployeeReferenceMigration.ReplaceInList<string>(null, "old", "new"));
		}

		[Fact]
		public void AMemberOfASetIsSwappedAndAbsenceIsALeftAlone()
		{
			var set = new HashSet<string> { "a", "old" };

			Assert.True(EmployeeReferenceMigration.ReplaceInSet(set, "old", "new"));
			Assert.False(EmployeeReferenceMigration.ReplaceInSet(set, "gone", "new"));
			Assert.False(EmployeeReferenceMigration.ReplaceInSet<string>(null, "old", "new"));

			Assert.True(set.SetEquals(new[] { "a", "new" }));
		}
	}
}

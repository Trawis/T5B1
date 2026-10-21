using Xunit;

namespace Trainer_v5.Tests
{
	// Max Followers fills the game's own cap once; it adds no arbitrary amounts (#232).
	public class MaxFollowersTests
	{
		[Fact]
		public void TheActionRecomputesTheCapAndFillsItWithoutAddingBillions()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void MaxFollowersAction");

			Assert.Contains("ReEvaluateMaxFollowers()", body);
			Assert.Contains("Followers = WorkItem.MaxFollowers", body);
			Assert.DoesNotContain("1000000000", body);
			Assert.DoesNotContain("FollowerChange", body);
			Assert.DoesNotContain("+=", body);
		}

		[Fact]
		public void TheOldActionRunsTheFourOriginalStatementsInOrder()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void MaxFollowersOldAction");

			Assert.Contains("item.Name == input && !item.Paused", body);
			Assert.Contains("FirstOrDefault(", body);
			SourceCode.AssertInOrder(body, "WorkItem.MaxFollowers += 1000000000;", "WorkItem.ReEvaluateMaxFollowers();", "order");
			SourceCode.AssertInOrder(body, "WorkItem.ReEvaluateMaxFollowers();", "WorkItem.FollowerChange += 1000000000f;", "order");
			SourceCode.AssertInOrder(body, "WorkItem.FollowerChange += 1000000000f;", "WorkItem.Followers += 1000000000f;", "order");
		}

		[Fact]
		public void TheOldActionReturnsSilentlyWhenNothingMatchesAndAddsNoSafeguards()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void MaxFollowersOldAction");

			SourceCode.AssertInOrder(body, "if (WorkItem == null)", "return;", "silent");
			SourceCode.AssertInOrder(body, "return;", "WorkItem.MaxFollowers +=", "lookup first");
			Assert.DoesNotContain("Notification", body);
			Assert.DoesNotContain("Confirmation", body);
			Assert.DoesNotContain("ExperimentalOptions", body);
			Assert.DoesNotContain("Math.", body);
		}

		[Fact]
		public void TheOldButtonOpensTheProductNameDialogAndSitsRightAfterMaxFollowers()
		{
			var open = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void MaxFollowersOld()");
			Assert.Contains("SharedMessages.ProductNamePrompt()", open);
			Assert.Contains("MaxFollowersOldAction", open);

			var keys = new System.Collections.Generic.List<string>();
			foreach (var row in SettingActionRows.Create())
			{
				keys.Add(row.LocKey);
			}

			Assert.Equal(keys.IndexOf("MaxFollowers") + 1, keys.IndexOf("MaxFollowersOld"));
		}

		[Fact]
		public void TheCurrentActionIsStillTheSafeOneAndTheButtonsAreSeparate()
		{
			var source = SourceCode.Read("Features", "WorkItemFeatures.cs");

			Assert.Contains("WindowManager.SpawnInputDialog(SharedMessages.ProductNamePrompt(), \"MaxFollowersTitle\".LocDef(\"Max Followers\"), \"\", MaxFollowersAction);", source);
			Assert.NotEqual(SourceCode.Body(source, "internal static void MaxFollowersAction"), SourceCode.Body(source, "internal static void MaxFollowersOldAction"));
		}
	}
}

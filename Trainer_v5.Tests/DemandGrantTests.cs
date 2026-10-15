using System.IO;
using Xunit;

namespace Trainer_v5.Tests
{
	// Granting a demand uses the game's AcceptDemand so its effect runs, without the follow-up demand request (#185).
	public class DemandGrantTests
	{
		[Fact]
		public void GrantingCallsAcceptDemandWithoutTheRefreshFlagAndRefundsTheCost()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeDemandChangeWindow.cs"));
			var start = source.IndexOf("private static void GrantDemand", System.StringComparison.Ordinal);
			var body = source.Substring(start);

			Assert.Contains("employee.AcceptDemand(choice, index, false)", body);
			Assert.Contains("MyEmployer.MakeTransaction(cost, Company.TransactionCategory.Deals)", body);
			Assert.True(body.IndexOf("AcceptDemand", System.StringComparison.Ordinal) < body.IndexOf("MakeTransaction", System.StringComparison.Ordinal));
		}

		[Fact]
		public void AnAlreadyGrantedDemandIsNeverAcceptedTwice()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Window", "EmployeeDemandChangeWindow.cs"));

			Assert.Contains("if (on && !has)", source);
			Assert.Single(System.Text.RegularExpressions.Regex.Matches(source, @"GrantDemand\(employee, choice"));
		}
	}
}

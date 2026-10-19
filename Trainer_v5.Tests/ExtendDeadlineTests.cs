using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
	// Extend Deadline moves a contract deadline forward by exactly one year (#353).
	// SDateTime cannot be constructed here (its constructor reads GameSettings), so the argument order is checked against the game assembly's metadata.
	public class ExtendDeadlineTests
	{
		private static readonly string[] FiveParameterOrder = { "minute", "hour", "day", "month", "year" };

		[Fact]
		public void TheGameConstructorTakesMinuteHourDayMonthYear()
		{
			var constructor = typeof(SDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) });

			Assert.NotNull(constructor);
			Assert.Equal(FiveParameterOrder, Enumerable.ToArray(Enumerable.Select(constructor.GetParameters(), parameter => parameter.Name)));
		}

		[Fact]
		public void TheThreeIntegerConstructorTakesDayMonthYearNotYearFirst()
		{
			var constructor = typeof(SDateTime).GetConstructor(new[] { typeof(int), typeof(int), typeof(int) });

			Assert.NotNull(constructor);
			Assert.Equal(new[] { "day", "month", "year" }, Enumerable.ToArray(Enumerable.Select(constructor.GetParameters(), parameter => parameter.Name)));
		}

		[Fact]
		public void TheHelperKeepsEveryPartInTheConstructorOrderAndAddsOnlyOneYear()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "WorkItemFeatures.cs"));

			Assert.Contains("new SDateTime(deadline.Minute, deadline.Hour, deadline.Day, deadline.Month, deadline.Year + 1)", source);
			Assert.DoesNotContain("new SDateTime(deadline.Year", source);
		}

		[Fact]
		public void TheActionSkipsWorkItemsWithoutAContractAndUsesTheHelper()
		{
			var source = File.ReadAllText(Path.Combine(RepositoryFiles.SourceDirectory, "Features", "WorkItemFeatures.cs"));
			var start = source.IndexOf("internal static void ExtendDeadline()", System.StringComparison.Ordinal);
			var body = source.Substring(start, source.IndexOf("internal static SDateTime ExtendedByYear", System.StringComparison.Ordinal) - start);

			Assert.Contains("if (contract == null)", body);
			Assert.Contains("continue;", body);
			Assert.Contains("contract.Deadline = ExtendedByYear(contract.Deadline);", body);
		}
	}
}

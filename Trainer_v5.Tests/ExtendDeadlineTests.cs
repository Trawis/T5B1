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
			var source = SourceCode.Read("Features", "WorkItemFeatures.cs");

			Assert.Contains("new SDateTime(deadline.Minute, deadline.Hour, deadline.Day, deadline.Month, deadline.Year + 1)", source);
			Assert.DoesNotContain("new SDateTime(deadline.Year", source);
		}

		[Fact]
		public void TheActionSkipsWorkItemsWithoutAContractAndUsesTheHelper()
		{
			var body = SourceCode.Body(SourceCode.Read("Features", "WorkItemFeatures.cs"), "internal static void ExtendDeadline()");

			Assert.Contains("if (contract == null)", body);
			Assert.Contains("continue;", body);
			Assert.Contains("contract.Deadline = ExtendedByYear(contract.Deadline);", body);
		}
	}
}

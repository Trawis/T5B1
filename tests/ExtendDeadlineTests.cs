using System.Linq;
using System.Reflection;
using Xunit;

namespace Trainer_v5.Tests
{
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
			CompiledCalls.Adjacent(typeof(WorkItemFeatures), "ExtendedByYear", "ldfld SDateTime.Minute", "ldfld SDateTime.Hour", "ldfld SDateTime.Day", "ldfld SDateTime.Month", "ldfld SDateTime.Year", "ldc 1", "call SDateTime..ctor");
		}
	}
}

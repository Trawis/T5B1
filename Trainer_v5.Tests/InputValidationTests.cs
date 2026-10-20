using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
	// Every test sets its own culture and restores it, so the parsing policy is proven under comma-decimal locales (#337).
	public class InputValidationTests : System.IDisposable
	{
		private readonly CultureInfo _originalCulture = Thread.CurrentThread.CurrentCulture;

		public void Dispose()
		{
			Thread.CurrentThread.CurrentCulture = _originalCulture;
		}

		[Theory]
		[InlineData("0", 0f)]
		[InlineData("1.5", 1.5f)]
		[InlineData("-3", -3f)]
		public void FloatWithoutRange_AcceptsFiniteNumbers(string input, float expected)
		{
			float value;

			Assert.Null(InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("")]
		[InlineData("abc")]
		[InlineData("1.2.3")]
		[InlineData("NaN")]
		[InlineData("Infinity")]
		[InlineData("-Infinity")]
		public void FloatWithoutRange_RejectsNonNumbersAndNonFiniteValues(string input)
		{
			float value;

			Assert.Equal("Invalid input!", InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
		}

		[Theory]
		[InlineData("0")]
		[InlineData("0.5")]
		[InlineData("1")]
		public void FloatInRange_AcceptsValuesIncludingTheBounds(string input)
		{
			float value;

			Assert.Null(InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMinimumWhenBelowIt()
		{
			float value;

			Assert.Equal("Invalid input!\nmin value is 0", InputHelper.Validate<float>("-0.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMaximumWhenAboveIt()
		{
			float value;

			Assert.Equal("Invalid input!\nmax value is 1", InputHelper.Validate<float>("1.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_RejectsNaNInsteadOfTreatingItAsInRange()
		{
			float value;

			Assert.Equal("Invalid input!", InputHelper.Validate<float>("NaN", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Theory]
		[InlineData("1", 1)]
		[InlineData("30", 30)]
		[InlineData("31", 31)]
		public void IntInRange_AcceptsValuesIncludingTheBounds(string input, int expected)
		{
			int value;

			Assert.Null(InputHelper.Validate<int>(input, InputHelper.ParseInt, 1, 31, out value));
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("0", "Invalid input!\nmin value is 1")]
		[InlineData("32", "Invalid input!\nmax value is 31")]
		[InlineData("1.5", "Invalid input!")]
		[InlineData("x", "Invalid input!")]
		[InlineData("", "Invalid input!")]
		[InlineData("99999999999", "Invalid input!")]
		public void IntInRange_RejectsOutOfRangeAndMalformedInput(string input, string expectedError)
		{
			int value;

			Assert.Equal(expectedError, InputHelper.Validate<int>(input, InputHelper.ParseInt, 1, 31, out value));
		}

		[Theory]
		[InlineData("-1")]
		[InlineData("")]
		[InlineData("abc")]
		public void UIntWithoutRange_RejectsNegativeAndMalformedInput(string input)
		{
			uint value;

			Assert.Equal("Invalid input!", InputHelper.Validate<uint>(input, InputHelper.ParseUInt, out value));
		}

		[Fact]
		public void UIntInRange_ReportsBoundsLikeOtherTypes()
		{
			uint value;

			Assert.Equal("Invalid input!\nmax value is 100", InputHelper.Validate<uint>("101", InputHelper.ParseUInt, 0u, 100u, out value));
			Assert.Null(InputHelper.Validate<uint>("100", InputHelper.ParseUInt, 0u, 100u, out value));
			Assert.Equal(100u, value);
		}

		[Theory]
		[InlineData("")]
		[InlineData("de-DE")]
		[InlineData("hr-HR")]
		[InlineData("en-US")]
		public void FloatParsing_IsTheSameInEveryCulture(string cultureName)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			float value;

			Assert.Null(InputHelper.Validate<float>("0.5", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal(0.5f, value);
			Assert.Null(InputHelper.Validate<float>("1.5", InputHelper.ParseFloat, out value));
			Assert.Equal(1.5f, value);
			Assert.Null(InputHelper.Validate<float>("-0.25", InputHelper.ParseFloat, out value));
			Assert.Equal(-0.25f, value);
		}

		[Theory]
		[InlineData("de-DE", "0,5")]
		[InlineData("hr-HR", "0,5")]
		[InlineData("en-US", "0,5")]
		[InlineData("de-DE", "1,234")]
		[InlineData("hr-HR", "1,234")]
		[InlineData("en-US", "1,234")]
		[InlineData("de-DE", "1.234,5")]
		[InlineData("en-US", "1,234.5")]
		[InlineData("de-DE", " ")]
		public void FloatParsing_RejectsCommaAndGroupedFormsInsteadOfGuessing(string cultureName, string input)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			float value;

			Assert.Equal("Invalid input!", InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
			Assert.Equal("Invalid input!", InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 5000f, out value));
		}

		[Theory]
		[InlineData("de-DE")]
		[InlineData("hr-HR")]
		public void FloatParsing_KeepsRejectingNonFiniteValuesAndBounds(string cultureName)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			float value;

			Assert.Equal("Invalid input!", InputHelper.Validate<float>("NaN", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal("Invalid input!", InputHelper.Validate<float>("Infinity", InputHelper.ParseFloat, out value));
			Assert.Equal("Invalid input!\nmax value is 1", InputHelper.Validate<float>("1.1", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal("Invalid input!\nmin value is 0", InputHelper.Validate<float>("-0.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Theory]
		[InlineData("de-DE")]
		[InlineData("hr-HR")]
		public void IntegerParsing_AcceptsPlainDigitsAndRejectsGroupingAndDecimals(string cultureName)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			int intValue;
			uint uintValue;

			Assert.Null(InputHelper.Validate<int>("100000", InputHelper.ParseInt, out intValue));
			Assert.Equal(100000, intValue);
			Assert.Null(InputHelper.Validate<int>("-3", InputHelper.ParseInt, out intValue));
			Assert.Equal(-3, intValue);
			Assert.Null(InputHelper.Validate<uint>("100000", InputHelper.ParseUInt, out uintValue));
			Assert.Equal(100000u, uintValue);

			foreach (var input in new[] { "1,234", "1.234", "1,5", "1.5" })
			{
				Assert.Equal("Invalid input!", InputHelper.Validate<int>(input, InputHelper.ParseInt, out intValue));
				Assert.Equal("Invalid input!", InputHelper.Validate<uint>(input, InputHelper.ParseUInt, out uintValue));
			}
		}
	}
}

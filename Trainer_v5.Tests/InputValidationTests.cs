using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
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

			Assert.Equal(InputError.None, InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
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

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
		}

		[Theory]
		[InlineData("0")]
		[InlineData("0.5")]
		[InlineData("1")]
		public void FloatInRange_AcceptsValuesIncludingTheBounds(string input)
		{
			float value;

			Assert.Equal(InputError.None, InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMinimumWhenBelowIt()
		{
			float value;

			Assert.Equal(InputError.BelowMin, InputHelper.Validate<float>("-0.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMaximumWhenAboveIt()
		{
			float value;

			Assert.Equal(InputError.AboveMax, InputHelper.Validate<float>("1.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_RejectsNaNInsteadOfTreatingItAsInRange()
		{
			float value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>("NaN", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Theory]
		[InlineData("1", 1)]
		[InlineData("30", 30)]
		[InlineData("31", 31)]
		public void IntInRange_AcceptsValuesIncludingTheBounds(string input, int expected)
		{
			int value;

			Assert.Equal(InputError.None, InputHelper.Validate<int>(input, InputHelper.ParseInt, 1, 31, out value));
			Assert.Equal(expected, value);
		}

		[Theory]
		[InlineData("1.5")]
		[InlineData("x")]
		[InlineData("")]
		[InlineData("99999999999")]
		public void IntInRange_RejectsMalformedInput(string input)
		{
			int value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<int>(input, InputHelper.ParseInt, 1, 31, out value));
		}

		[Fact]
		public void IntInRange_ReportsWhichBoundWasCrossed()
		{
			int value;

			Assert.Equal(InputError.BelowMin, InputHelper.Validate<int>("0", InputHelper.ParseInt, 1, 31, out value));
			Assert.Equal(InputError.AboveMax, InputHelper.Validate<int>("32", InputHelper.ParseInt, 1, 31, out value));
		}

		[Theory]
		[InlineData("-1")]
		[InlineData("")]
		[InlineData("abc")]
		public void UIntWithoutRange_RejectsNegativeAndMalformedInput(string input)
		{
			uint value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<uint>(input, InputHelper.ParseUInt, out value));
		}

		[Fact]
		public void UIntInRange_ReportsBoundsLikeOtherTypes()
		{
			uint value;

			Assert.Equal(InputError.AboveMax, InputHelper.Validate<uint>("101", InputHelper.ParseUInt, 0u, 100u, out value));
			Assert.Equal(InputError.None, InputHelper.Validate<uint>("100", InputHelper.ParseUInt, 0u, 100u, out value));
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

			Assert.Equal(InputError.None, InputHelper.Validate<float>("0.5", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal(0.5f, value);
			Assert.Equal(InputError.None, InputHelper.Validate<float>("1.5", InputHelper.ParseFloat, out value));
			Assert.Equal(1.5f, value);
			Assert.Equal(InputError.None, InputHelper.Validate<float>("-0.25", InputHelper.ParseFloat, out value));
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

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 5000f, out value));
		}

		[Theory]
		[InlineData("de-DE")]
		[InlineData("hr-HR")]
		public void FloatParsing_KeepsRejectingNonFiniteValuesAndBounds(string cultureName)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			float value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>("NaN", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>("Infinity", InputHelper.ParseFloat, out value));
			Assert.Equal(InputError.AboveMax, InputHelper.Validate<float>("1.1", InputHelper.ParseFloat, 0f, 1f, out value));
			Assert.Equal(InputError.BelowMin, InputHelper.Validate<float>("-0.1", InputHelper.ParseFloat, 0f, 1f, out value));
		}

		[Theory]
		[InlineData("de-DE")]
		[InlineData("hr-HR")]
		public void IntegerParsing_AcceptsPlainDigitsAndRejectsGroupingAndDecimals(string cultureName)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			int intValue;
			uint uintValue;

			Assert.Equal(InputError.None, InputHelper.Validate<int>("100000", InputHelper.ParseInt, out intValue));
			Assert.Equal(100000, intValue);
			Assert.Equal(InputError.None, InputHelper.Validate<int>("-3", InputHelper.ParseInt, out intValue));
			Assert.Equal(-3, intValue);
			Assert.Equal(InputError.None, InputHelper.Validate<uint>("100000", InputHelper.ParseUInt, out uintValue));
			Assert.Equal(100000u, uintValue);

			foreach (var input in new[] { "1,234", "1.234", "1,5", "1.5" })
			{
				Assert.Equal(InputError.Invalid, InputHelper.Validate<int>(input, InputHelper.ParseInt, out intValue));
				Assert.Equal(InputError.Invalid, InputHelper.Validate<uint>(input, InputHelper.ParseUInt, out uintValue));
			}
		}
	}
}

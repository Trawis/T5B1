using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
	// Every test sets its own culture and restores it, so the parsing policy is proven under comma-decimal locales.
	public class InputValidationTests : System.IDisposable
	{
		private readonly CultureInfo _originalCulture = Thread.CurrentThread.CurrentCulture;

		public void Dispose()
		{
			Thread.CurrentThread.CurrentCulture = _originalCulture;
		}

		[Theory]
		[InlineData("abc")]
		[InlineData("NaN")]
		public void FloatWithoutRange_RejectsNonNumbersAndNonFiniteValues(string input)
		{
			float value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
		}

		[Theory]
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
		[InlineData("1.5")]
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
		public void UIntWithoutRange_RejectsNegativeAndMalformedInput(string input)
		{
			uint value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<uint>(input, InputHelper.ParseUInt, out value));
		}

		[Theory]
		[InlineData("de-DE")]
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
		[InlineData("en-US", "1,234.5")]
		public void FloatParsing_RejectsCommaAndGroupedFormsInsteadOfGuessing(string cultureName, string input)
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			float value;

			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, out value));
			Assert.Equal(InputError.Invalid, InputHelper.Validate<float>(input, InputHelper.ParseFloat, 0f, 5000f, out value));
		}

		[Theory]
		[InlineData("de-DE")]
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

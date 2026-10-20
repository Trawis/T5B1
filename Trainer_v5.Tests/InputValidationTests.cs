using System.Globalization;
using System.Threading;
using Xunit;

namespace Trainer_v5.Tests
{
	// Pins the invariant culture on purpose; the culture-dependent parsing it hides is tracked in #337 and this fixture changes with it.
	public class InputValidationTests : System.IDisposable
	{
		private readonly CultureInfo _originalCulture = Thread.CurrentThread.CurrentCulture;

		public InputValidationTests()
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
		}

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

			Assert.Null(InputHelper.Validate<float>(input, float.TryParse, out value));
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

			Assert.Equal("Invalid input!", InputHelper.Validate<float>(input, float.TryParse, out value));
		}

		[Theory]
		[InlineData("0")]
		[InlineData("0.5")]
		[InlineData("1")]
		public void FloatInRange_AcceptsValuesIncludingTheBounds(string input)
		{
			float value;

			Assert.Null(InputHelper.Validate<float>(input, float.TryParse, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMinimumWhenBelowIt()
		{
			float value;

			Assert.Equal("Invalid input!\nmin value is 0", InputHelper.Validate<float>("-0.1", float.TryParse, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_ReportsTheMaximumWhenAboveIt()
		{
			float value;

			Assert.Equal("Invalid input!\nmax value is 1", InputHelper.Validate<float>("1.1", float.TryParse, 0f, 1f, out value));
		}

		[Fact]
		public void FloatInRange_RejectsNaNInsteadOfTreatingItAsInRange()
		{
			float value;

			Assert.Equal("Invalid input!", InputHelper.Validate<float>("NaN", float.TryParse, 0f, 1f, out value));
		}

		[Theory]
		[InlineData("1", 1)]
		[InlineData("30", 30)]
		[InlineData("31", 31)]
		public void IntInRange_AcceptsValuesIncludingTheBounds(string input, int expected)
		{
			int value;

			Assert.Null(InputHelper.Validate<int>(input, int.TryParse, 1, 31, out value));
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

			Assert.Equal(expectedError, InputHelper.Validate<int>(input, int.TryParse, 1, 31, out value));
		}

		[Theory]
		[InlineData("-1")]
		[InlineData("")]
		[InlineData("abc")]
		public void UIntWithoutRange_RejectsNegativeAndMalformedInput(string input)
		{
			uint value;

			Assert.Equal("Invalid input!", InputHelper.Validate<uint>(input, uint.TryParse, out value));
		}

		[Fact]
		public void UIntInRange_ReportsBoundsLikeOtherTypes()
		{
			uint value;

			Assert.Equal("Invalid input!\nmax value is 100", InputHelper.Validate<uint>("101", uint.TryParse, 0u, 100u, out value));
			Assert.Null(InputHelper.Validate<uint>("100", uint.TryParse, 0u, 100u, out value));
			Assert.Equal(100u, value);
		}
	}
}

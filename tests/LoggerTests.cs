using System;
using Xunit;

namespace Trainer_v5.Tests
{
	public class LoggerTests
	{
		private static Exception Thrown(Exception exception)
		{
			try
			{
				throw exception;
			}
			catch (Exception caught)
			{
				return caught;
			}
		}

		[Fact]
		public void ExceptionTextContainsTypeMessageAndStackTrace()
		{
			var text = Logger.FormatException(Thrown(new InvalidOperationException("boom")), null);

			Assert.StartsWith("Trainer Exception: ", text);
			Assert.Contains("System.InvalidOperationException", text);
			Assert.Contains("boom", text);
			Assert.Contains("LoggerTests.Thrown", text);
		}

		[Fact]
		public void ExceptionTextContainsTheInnerExceptionChain()
		{
			var outer = new InvalidOperationException("outer", new ArgumentException("inner problem"));

			var text = Logger.FormatException(Thrown(outer), null);

			Assert.Contains("outer", text);
			Assert.Contains("System.ArgumentException", text);
			Assert.Contains("inner problem", text);
		}

		[Fact]
		public void FeatureExceptionTextNamesTheFeature()
		{
			var text = Logger.FormatException(new InvalidOperationException("boom"), "NoSickness");

			Assert.StartsWith("Trainer feature 'NoSickness' failed: ", text);
			Assert.Contains("System.InvalidOperationException: boom", text);
		}
	}
}

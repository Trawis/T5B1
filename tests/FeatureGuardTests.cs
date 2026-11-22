using System;
using System.Collections.Generic;
using Xunit;

namespace Trainer_v5.Tests
{
	public class FeatureGuardTests
	{
		private float _now;
		private readonly List<string> _reports = new List<string>();

		private FeatureGuard CreateGuard()
		{
			return new FeatureGuard(() => _now, (featureId, ex) => _reports.Add(featureId + "|" + ex.GetType().Name));
		}

		private static void Throw<T>() where T : Exception, new()
		{
			throw new T();
		}

		[Fact]
		public void AFailingActionReturnsFalseInsteadOfThrowing()
		{
			Assert.False(CreateGuard().TryExecute("A", Throw<InvalidOperationException>));
		}

		[Fact]
		public void TheSameFailureWithinTheIntervalIsReportedOnce()
		{
			var guard = CreateGuard();

			guard.TryExecute("A", Throw<InvalidOperationException>);
			_now = 59.9f;
			Assert.False(guard.TryExecute("A", Throw<InvalidOperationException>));

			Assert.Single(_reports);
		}

		[Fact]
		public void TheSameFailureIsReportedAgainOnceTheIntervalHasPassed()
		{
			var guard = CreateGuard();

			guard.TryExecute("A", Throw<InvalidOperationException>);
			_now = 60f;
			guard.TryExecute("A", Throw<InvalidOperationException>);

			Assert.Equal(2, _reports.Count);
		}

		[Fact]
		public void ADifferentFeatureIsReportedImmediately()
		{
			var guard = CreateGuard();

			guard.TryExecute("A", Throw<InvalidOperationException>);
			guard.TryExecute("B", Throw<InvalidOperationException>);

			Assert.Equal(new[] { "A|InvalidOperationException", "B|InvalidOperationException" }, _reports);
		}
	}
}

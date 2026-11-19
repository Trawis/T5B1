using System;
using Xunit;

namespace Trainer_v5.Tests
{
	public class BusinessReputationTests
	{
		private static float NativeChange(float reputation, float percentStars, float max)
		{
			if (percentStars == 0f)
				return reputation;

			percentStars = percentStars > 0f ? Math.Min(max, percentStars) : Math.Max(-max, percentStars);
			float sum = (float)(reputation + (float)(percentStars / 6f));
			return Math.Max(0f, Math.Min(1f, sum));
		}

		private static int NativeStars(float reputation)
		{
			float tolerance = Math.Max(1e-6f * Math.Max(Math.Abs(reputation), 1f), float.Epsilon * 8f);
			if (Math.Abs(1f - reputation) < tolerance)
				return 6;

			return (int)Math.Floor((float)(reputation * 6f));
		}

		private static float SetTo(float reputation, int stars)
		{
			float delta = (float)((float)(CompanyFeatures.BusinessReputationFor(stars) - reputation) * Company.BusinessRepStars);
			return NativeChange(reputation, delta, Company.BusinessRepStars);
		}

		[Fact]
		public void EveryWholeStarIsShownExactlyFromAnyStartingReputation()
		{
			for (int stars = 0; stars <= Company.BusinessRepStars; stars++)
			{
				for (int i = 0; i <= 20000; i++)
				{
					float start = i / 20000f;
					float result = SetTo(start, stars);
					Assert.True(NativeStars(result) == stars, "Start " + start.ToString("R") + " set to " + stars + " shows " + NativeStars(result));
					Assert.True(CompanyFeatures.IsBusinessReputationAt(result, stars), "Start " + start.ToString("R") + " set to " + stars + " gave " + result.ToString("R"));
				}
			}
		}

		[Fact]
		public void SettingTheSameStarsAgainIsRecognisedAsNoChange()
		{
			Assert.True(CompanyFeatures.IsBusinessReputationAt(SetTo(0.3f, 4), 4));
			Assert.True(CompanyFeatures.IsBusinessReputationAt(1f, 6));
			Assert.True(CompanyFeatures.IsBusinessReputationAt(0f, 0));
		}

		[Fact]
		public void AReputationBetweenStarsIsNotTakenForTheLowerStar()
		{
			Assert.False(CompanyFeatures.IsBusinessReputationAt(3.5f / 6f, 3));
			Assert.False(CompanyFeatures.IsBusinessReputationAt(2.98f / 6f, 3));
			Assert.False(CompanyFeatures.IsBusinessReputationAt(5.5f / 6f, 6));
		}
	}
}

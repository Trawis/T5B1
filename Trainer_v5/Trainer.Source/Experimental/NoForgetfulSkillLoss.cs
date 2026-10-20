namespace Trainer_v5.Experimental
{
	internal static class NoForgetfulSkillLoss
	{
		// The game loses a Forgetful employee's specialization only when the actor leaves the office after ForgetfulETA has
		// passed (AI.Despawn); keeping the timer ahead of the clock skips the loss, the popup and the re-arm. Unarmed (0) stays.
		internal static void Apply()
		{
			GameSettings settings = GameSettings.Instance;

			if (!ExperimentalOptions.Enabled || settings == null || TimeOfDay.Instance == null)
				return;

			SDateTime now = SDateTime.Now();
			SDateTime unarmed = new SDateTime(0);
			SDateTime threshold = now + 3;

			for (int i = 0; i < settings.sActorManager.Actors.Count; i++)
			{
				Actor actor = settings.sActorManager.Actors[i];

				if (actor.employee == null || !actor.employee.HasTrait(Employee.Trait.Forgetful))
					continue;

				if (actor.ForgetfulETA != unarmed && actor.ForgetfulETA <= threshold)
					actor.ForgetfulETA = now + 12;
			}
		}
	}
}

using System;
using System.Collections.Generic;

namespace Trainer_v5.Experimental
{
	// The same steps as the game's own hire (HireWindow.DoActualHiring): Employ pays the upfront demands, then the actor is spawned.
	internal static class InstantHire
	{
		private const string ActionName = "Hire Employees";
		internal const int MaxCount = 10;
		private const Employee.WageBracket Bracket = Employee.WageBracket.Medium;

		internal static void Run()
		{
			if (!CanHire())
				return;

			WindowManager.SpawnInputDialog("HireRolePrompt".LocDef("Which role? Lead, Programmer, Designer, Artist or Service"), Title(), "Programmer", text =>
			{
				Employee.EmployeeRole role;
				if (!TryParseRole(text, out role))
				{
					Notification.ShowError("HireRoleInvalid".LocDef("Unknown role. Enter Lead, Programmer, Designer, Artist or Service."));
					return;
				}

				InputHelper.RequestInt(string.Format("HireCountPrompt".LocDef("How many employees? (1-{0})"), MaxCount), Title(), "1", count => Prepare(role, count), 1, MaxCount);
			});
		}

		internal static bool TryParseRole(string text, out Employee.EmployeeRole role)
		{
			role = Employee.EmployeeRole.Programmer;
			if (string.IsNullOrEmpty(text))
				return false;

			foreach (Employee.EmployeeRole value in Enum.GetValues(typeof(Employee.EmployeeRole)))
			{
				if (string.Equals(value.ToString(), text.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					role = value;
					return true;
				}
			}

			return false;
		}

		private static void Prepare(Employee.EmployeeRole role, int count)
		{
			if (!CanHire())
				return;

			GameSettings game = GameSettings.Instance;
			HireWindow hire = HireWindowOrNull();
			if (hire == null || hire.HirePool == null || !hire.HirePool.ContainsKey(new KeyValuePair<Employee.EmployeeRole, Employee.WageBracket>(role, Bracket)))
			{
				Notification.ShowError(Unavailable());
				return;
			}

			Employee[] candidates = hire.GenerateEmployees(count, Bracket, role, false, null, null, null, Employee.Trait.None, Employee.Trait.None, null, null);
			if (candidates == null || candidates.Length == 0)
			{
				Notification.ShowError(Unavailable());
				return;
			}

			string team = TeamOrNull(game, hire.SelectedTeam);
			double upfront = 0;
			double salaries = 0;
			foreach (Employee emp in candidates)
			{
				upfront += emp.GetUpfrontCost(false);
				salaries += emp.GetMonthlySalary(team != null ? game.sActorManager.Teams[team] : null);
			}

			string message = string.Format("HireConfirm".LocDef("Hire {0} new {1} employee(s) at medium wage?\nUpfront demands: {2}. Monthly salaries: {3}.\nTeam: {4}.\nEach one is paid, employed and added to the office the game's own way. Anyone your business reputation cannot attract, or you cannot pay, is skipped.\nThis cannot be undone."), candidates.Length, role, upfront.Currency(), salaries.Currency(), team ?? "TeamNone".LocDef("none"));
			Confirmation.Ask(ActionName, message, () => Apply(game, hire, candidates, team));
		}

		// The dialog stays open while the game runs, so the game and the team are re-checked first.
		private static void Apply(GameSettings game, HireWindow hire, Employee[] candidates, string team)
		{
			if (!ReferenceEquals(game, GameSettings.Instance) || !ReferenceEquals(hire, HireWindowOrNull()) || GameSettings.ForcePause)
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			team = TeamOrNull(game, team);
			Company company = game.MyCompany;
			int hired = 0, lowReputation = 0, noMoney = 0;
			foreach (Employee emp in candidates)
			{
				if (emp.MyEmployer != null)
					continue;

				if (!GameSettings.IgnoreBusinessRep && emp.LastDemandScore > company.BusinessReputation)
				{
					lowReputation++;
					continue;
				}

				float cost = emp.GetUpfrontCost(false);
				if (cost > 0f && !company.CanMakeTransaction(-cost))
				{
					noMoney++;
					continue;
				}

				emp.Filter = true;
				emp.Employ(company, SDateTime.Now(), false, false);
				game.RegisterStat("Hired", 1f);
				Actor actor = game.SpawnActor(emp, true);
				if (team != null)
					actor.Team = team;

				hired++;
			}

			string result = string.Format("HireResult".LocDef("Trainer: hired {0} of {1} employee(s). Skipped: {2} for business reputation, {3} for money."), hired, candidates.Length, lowReputation, noMoney);
			if (hired > 0)
				Notification.Popup(result);
			else
				Notification.ShowError(result);
		}

		private static bool CanHire()
		{
			if (!ExperimentalOptions.Enabled || !Helpers.RequireGameReady(ActionName))
				return false;

			return true;
		}

		private static string Title()
		{
			return "HireEmployeesTitle".LocDef("Hire Employees");
		}

		private static string Unavailable()
		{
			return "HireUnavailable".LocDef("The game's hiring is not available right now. Nothing was changed.");
		}

		private static HireWindow HireWindowOrNull()
		{
			return HUD.Instance != null && HUD.Instance.hireWindow != null ? HUD.Instance.hireWindow.HireWin : null;
		}

		private static string TeamOrNull(GameSettings game, string team)
		{
			return !string.IsNullOrEmpty(team) && game.sActorManager.Teams.ContainsKey(team) ? team : null;
		}
	}
}

using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class WorkItemFeatures
	{
		private static GameSettings Settings => GameSettings.Instance;

		internal static void OptimizeTeamAssignments()
		{
			int reassigned = 0;

			foreach (WorkItem item in Settings.MyCompany.WorkItems.ToList())
			{
				if (item.AutoDev || item.DevTeams.Count != 1)
				{
					continue;
				}

				Employee.RoleBit? roleBit = GetOptimizerRoleBit(item);
				if (roleBit == null)
				{
					continue;
				}

				DesignDocument designDocument = item as DesignDocument;
				if (designDocument != null && designDocument.NeedsLead() && designDocument.LeadWork != null)
				{
					continue;
				}

				Team currentTeam = GameSettings.GetTeam(item.DevTeams.First());
				if (currentTeam == null)
				{
					continue;
				}

				Team bestTeam = currentTeam;
				int bestScore = ScoreTeamForRole(currentTeam, roleBit.Value);

				foreach (Team team in Settings.sActorManager.Teams.Values)
				{
					int score = ScoreTeamForRole(team, roleBit.Value);
					if (score > bestScore)
					{
						bestScore = score;
						bestTeam = team;
					}
				}

				if (bestTeam != currentTeam)
				{
					item.SetDevTeams(new List<string> { bestTeam.Name });
					reassigned++;
				}
			}

			if (reassigned == 0)
			{
				WindowManager.SpawnDialog("TeamsAlreadyOptimal".LocDef("Trainer: All eligible work is already assigned to its best available team!"), false, DialogWindow.DialogType.Information);
				return;
			}

			HUD.Instance.AddPopupMessage("TeamAssignmentsOptimized".LocDef("Trainer: Team assignments optimized!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void InstantPorting()
		{
			var ports = Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList();

			if (ports.Count == 0)
			{
				WindowManager.SpawnDialog("NoPortingWork".LocDef("Trainer: No active porting work to complete!"), false, DialogWindow.DialogType.Information);
				return;
			}

			foreach (SoftwarePort port in ports)
			{
				// Bounded by OSs.Count so a stuck port is not looped on.
				for (int i = 0; i < port.OSs.Count; i++)
				{
					if (!AdvanceSoftwarePort(port))
					{
						break;
					}
				}
			}

			HUD.Instance.AddPopupMessage("PortingCompleted".LocDef("Trainer: Porting work has been completed!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void ExtendDeadline()
		{
			foreach (var work in Settings.MyCompany.WorkItems)
			{
				var contract = work.contract;
				if (contract == null)
					continue;
				contract.Deadline = ExtendedByYear(contract.Deadline);
			}
		}

		// The game calendar has a fixed number of days per month and no leap days, so the same day and month is always valid.
		internal static SDateTime ExtendedByYear(SDateTime deadline)
		{
			return new SDateTime(deadline.Minute, deadline.Hour, deadline.Day, deadline.Month, deadline.Year + 1);
		}

		internal static void FixBugsAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && item.InBeta);

			if (WorkItem == null)
			{
				WindowManager.SpawnDialog("Trainer: No product named " + input + " is in beta!", false, DialogWindow.DialogType.Information);
				return;
			}

			WorkItem.FixedBugs = WorkItem.MaxBugs;
		}

		internal static void FixBugs()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Fix Bugs", "", FixBugsAction);
		}

		internal static void MaxFollowersAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.Paused);

			if (WorkItem == null)
			{
				Notification.ShowError("No active (not paused) alpha named \"" + input + "\".");
				return;
			}

			// ReEvaluateMaxFollowers recomputes the cap and the setter clamps to it: fill it once, leave the daily change alone.
			WorkItem.ReEvaluateMaxFollowers();
			WorkItem.Followers = WorkItem.MaxFollowers;
		}

		internal static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Max Followers", "", MaxFollowersAction);
		}

		internal static void InstantResearch()
		{
			var eligibleResearch = Settings.MyCompany.WorkItems
								.OfType<ResearchWork>()
								.Where(rw => !rw.Finished)
								.ToList();

			if (eligibleResearch.Count == 0)
			{
				WindowManager.SpawnDialog("NoActiveResearch".LocDef("Trainer: No active research to complete!"), false, DialogWindow.DialogType.Information);
				return;
			}

			eligibleResearch.ForEach(researchWork =>
			{
				researchWork.Progress = researchWork.Max;
				researchWork.Finished = true;
				CompleteResearchWork(researchWork);
			});

			HUD.Instance.AddPopupMessage("ResearchCompleted".LocDef("Trainer: Research completed instantly!"), "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		internal static void ApplyAutoEndDesign()
		{
			// PromoteAction() blocks without this precondition, so it is checked here first.
			var designDocuments = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			designDocuments.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

		// Same precondition as ApplyAutoEndDesign, for contract work.
		internal static void ApplyAutoContractProgression()
		{
			var contractDesigns = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.contract != null && d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			contractDesigns.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

		internal static void ApplyAutoEndResearch()
		{
			var researchWorks = Settings.MyCompany.WorkItems
								.OfType<ResearchWork>()
								.Where(rw => rw.Finished)
								.ToList();

			researchWorks.ForEach(CompleteResearchWork);
		}

		internal static void ApplyAutoEndPatent()
		{
			var legalWorks = Settings.MyCompany.WorkItems
							   .OfType<LegalWork>()
							   .Where(lw => lw.Type == LegalWork.WorkType.Patent &&
									lw.Progress >= 1f)
							   .ToList();

			legalWorks.ForEach(legalWork =>
			{
				legalWork.PatentNow();
			});
		}

		// Refreshing the timestamp keeps Simulate() from ever counting a ticket as missed.
		internal static void ApplyNoMissedSupportTickets()
		{
			foreach (SupportWork supportWork in Settings.MyCompany.WorkItems.OfType<SupportWork>())
			{
				for (int i = 0; i < supportWork.Tickets.Count; i++)
				{
					supportWork.Tickets[i] = SDateTime.Now();
				}
			}
		}

		internal static void ApplyAutoResearchStart()
		{
			var activeTechLevels = MarketSimulation.Active.TechLevels;
			var defaultResearchTeams = GameSettings.Instance.GetDefaultTeams("Research");
			var currentYear = TimeOfDay.Instance.Year;

			if (activeTechLevels.Count > 0 && defaultResearchTeams.Count > 0)
			{
				foreach (var activeTechLevel in activeTechLevels)
				{
					if (!Settings.IsResearching(activeTechLevel.Key))
					{
						int latestResearchYear = Settings.MyCompany.GetLatestResearch(activeTechLevel.Key, -1);
						if (latestResearchYear < currentYear)
						{
							var researchWork = new ResearchWork(activeTechLevel.Key, currentYear);
							researchWork.AddDevTeams(defaultResearchTeams);
							Settings.MyCompany.AddWorkItem(researchWork);
						}
					}
				}
			}
		}

		internal static void ApplyAutoPorting()
		{
			foreach (SoftwarePort port in Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList())
			{
				AdvanceSoftwarePort(port);
			}
		}

		// The RoleBit each type's own GetBoostRole checks; other types are left alone.
		private static Employee.RoleBit? GetOptimizerRoleBit(WorkItem item)
		{
			if (item is DesignDocument)
			{
				return Employee.RoleBit.Designer;
			}

			if (item is MarketingPlan)
			{
				return Employee.RoleBit.Service;
			}

			if (item is SoftwarePort)
			{
				return Employee.RoleBit.Programmer;
			}

			return null;
		}

		private static int ScoreTeamForRole(Team team, Employee.RoleBit roleBit)
		{
			return team.GetEmployeesDirect().Count(actor => actor.employee.IsRole(roleBit, false));
		}

		// Same sequence as ResearchWork.FinishNow, minus the patent confirmation dialog and its achievement counter.
		private static void CompleteResearchWork(ResearchWork researchWork)
		{
			Settings.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
			TechLevel tech = Settings.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
			if (tech != null)
			{
				LegalWork legalWork = new LegalWork(tech);
				Settings.MyCompany.AddWorkItem(legalWork);
				// The game's key is the enum name ("PatentTeam"), not its number.
				Settings.ApplyDefaultTeams(legalWork, legalWork.Type + "Team");
			}
			HUD.Instance.researchWindow.UpdateLists();
			researchWork.Kill(false);
		}

		// Mirrors SoftwarePort.DoWork at Progress == Goal.
		private static bool AdvanceSoftwarePort(SoftwarePort port)
		{
			if (port.Current == null || port.Product == null || port.Product.DevCompany != Settings.MyCompany)
			{
				return false;
			}

			port.Current.Progress = port.Current.Goal;
			port.Current.Finished = true;
			port.RefreshCurrent();
			return true;
		}
	}
}

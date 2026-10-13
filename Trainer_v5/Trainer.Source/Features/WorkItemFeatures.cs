using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	internal static class WorkItemFeatures
	{
		private static GameSettings Settings => GameSettings.Instance;

		internal static void ApplyAutoEndDesign()
		{
			// PromoteAction() itself blocks on this precondition (staying HasFinished forever
			// without it), so it must be mirrored here to avoid calling it on ineligible designs.
			var designDocuments = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			designDocuments.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

		// WorkItem.contract is set on the DesignDocument the game creates for accepted contract work
		// (ContractWork.GenerateWorkItem -> DesignDocument.CreateWork), so it identifies contract work
		// without guessing from names. HasFinished already means "required code/art units satisfied for
		// every design iteration" (verified in DesignDocument.DoWork: it only flips true once AllDone()
		// passes for the current iteration and either Parent is set or the iteration cap is reached), so
		// PromoteAction() is called with the exact same precondition as ApplyAutoEndDesign(). PromoteAction
		// itself only replaces the DesignDocument with the next-phase WorkItem (QA/bug-fixing/release stay
		// untouched, and normal/manual control over that new item is unaffected). Running after
		// ApplyAutoEndDesign() in OnMinutePassed means anything it already promoted (and Kill()ed out of
		// WorkItems) is gone from this query, so the two toggles can never double-promote the same item.
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

		// The RoleBit each type's own GetBoostRole checks; other WorkItem types have no single verified preference and are left alone.
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

		public static void OptimizeTeamAssignments()
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
				WindowManager.SpawnDialog("Trainer: All eligible work is already assigned to its best available team!", false, DialogWindow.DialogType.Information);
				return;
			}

			HUD.Instance.AddPopupMessage("Trainer: Team assignments optimized!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		// Verified against ResearchWork.FinishNow() in the vendored assembly: this is the same
		// AddResearch + AddTechLevel + (conditional) LegalWork + Kill(false) sequence the game
		// itself runs to finalize a completed research work item, minus the player-facing patent
		// confirmation dialog. Kill(false) removes the item from Company.WorkItems immediately,
		// so a work item can never reach this method twice.
		private static void CompleteResearchWork(ResearchWork researchWork)
		{
			Settings.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
			TechLevel tech = Settings.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
			if (tech != null)
			{
				LegalWork legalWork = new LegalWork(tech);
				Settings.MyCompany.WorkItems.Add(legalWork);
				Settings.ApplyDefaultTeams(legalWork, ((int)legalWork.Type).ToString() + "Team");
			}
			researchWork.Kill(false);
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
							   .Where(lw => lw.CurrentStage() == "Finished" &&
									lw.Type == LegalWork.WorkType.Patent)
							   .ToList();

			legalWorks.ForEach(legalWork =>
			{
				legalWork.PatentNow();
			});
		}

		// Simulate() misses a ticket once its stored timestamp ages past ~2 months; refreshing it here keeps that check from ever tripping.
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

		// Mirrors what SoftwarePort.DoWork itself does once Progress reaches Goal; RefreshCurrent() advances to the next OS or kills the work item once all are ported.
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

		internal static void ApplyAutoPorting()
		{
			foreach (SoftwarePort port in Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList())
			{
				AdvanceSoftwarePort(port);
			}
		}

		public static void InstantPorting()
		{
			var ports = Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList();

			if (ports.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active porting work to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			foreach (SoftwarePort port in ports)
			{
				// Bounded by OSs.Count so a port stuck waiting on something else is left alone rather than looped on.
				for (int i = 0; i < port.OSs.Count; i++)
				{
					if (!AdvanceSoftwarePort(port))
					{
						break;
					}
				}
			}

			HUD.Instance.AddPopupMessage("Trainer: Porting work has been completed!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void Test()
		{
			var designDocuments = Settings.MyCompany.WorkItems.OfType<DesignDocument>().Where(d => !d.HasFinished).ToList();

			foreach (var designDocument in designDocuments)
			{
				for (int i = 0; i < DesignDocument.MaxIteration; i++)
				{
					if (designDocument.Parent == null && designDocument.Iteration < 3)
					{
						for (int index = 0; index < designDocument.Features.Length; index++)
						{
							designDocument.Features[index].ArtDone = designDocument.Features[index].CodeDone = false;
							designDocument.Features[index].Progress = 1f;
							designDocument.Features[index].DevTime = 1f;
							designDocument.Features[index].Qual = 1f;
							designDocument.Features[index].LastIterationProg = 1f;
						}
						DevConsole.Console.Log(designDocument.GetProgress());
						designDocument.Iteration++;
					}
				}
			}
		}

		public static void ExtendDeadline()
		{
			foreach (var work in Settings.MyCompany.WorkItems)
			{
				var contract = work.contract;
				if (contract == null) continue;
				var deadline = contract.Deadline;
				contract.Deadline = new SDateTime(deadline.Year + 1, deadline.Month, deadline.Day);
			}
		}

		public static void FixBugsAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && item.InBeta);

			if (WorkItem == null)
			{
				return;
			}

			WorkItem.FixedBugs = WorkItem.MaxBugs;
		}

		public static void FixBugs()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Fix Bugs", "", FixBugsAction);
		}

		public static void MaxFollowersAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.Paused);

			if (WorkItem == null)
			{
				return;
			}

			WorkItem.MaxFollowers += 1000000000;
			WorkItem.ReEvaluateMaxFollowers();

			WorkItem.FollowerChange += 1000000000f;
			WorkItem.Followers += 1000000000f;
		}

		public static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog("Type product name:", "Max Followers", "", MaxFollowersAction);
		}

		public static void InstantResearch()
		{
			var eligibleResearch = Settings.MyCompany.WorkItems
								.OfType<ResearchWork>()
								.Where(rw => !rw.Finished)
								.ToList();

			if (eligibleResearch.Count == 0)
			{
				WindowManager.SpawnDialog("Trainer: No active research to complete!", false, DialogWindow.DialogType.Information);
				return;
			}

			eligibleResearch.ForEach(researchWork =>
			{
				researchWork.Progress = researchWork.Max;
				researchWork.Finished = true;
				CompleteResearchWork(researchWork);
			});

			HUD.Instance.AddPopupMessage("Trainer: Research completed instantly!", "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
		}

		public static void TestAction(string input)
		{
			WorkItem WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.InBeta);

			if (WorkItem == null)
				return;

			var softwareAlpha = ((SoftwareAlpha)WorkItem);
			softwareAlpha.AddQuality(10f, 10f, false);
		}

		public static void TestButton()
		{
			WindowManager.SpawnInputDialog("Type product name in alpha (not beta):", "Add Quality", "", TestAction);
		}
	}
}

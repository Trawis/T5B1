using System.Collections.Generic;
using System.Linq;
using Trainer_v5.Experimental;

namespace Trainer_v5
{
	internal static class WorkItemFeatures
	{
		private static GameSettings Settings => GameSettings.Instance;

		internal static void OptimizeTeamAssignments()
		{
			var changes = new List<TeamRecommendation>();
			var skippedLines = new List<string>();
			int skipped = 0;

			foreach (WorkItem item in Settings.MyCompany.WorkItems.ToList())
			{
				string reason;
				TeamRecommendation recommendation = Recommend(item, out reason);
				if (recommendation != null)
				{
					changes.Add(recommendation);
					continue;
				}

				skipped++;
				if (reason != null)
				{
					skippedLines.Add(string.Format("{0}: {1}", item.Name, reason));
				}
			}

			if (changes.Count == 0)
			{
				Notification.ShowInfo(SharedMessages.TeamAssignmentsNoChange(skipped));
				return;
			}

			var lines = new List<string>();
			foreach (TeamRecommendation change in changes)
			{
				lines.Add(string.Format("{0}: {1} ({2}) -> {3} ({4})", change.Item.Name, change.Current.Name, change.CurrentScore, change.Proposed.Name, change.ProposedScore));
			}

			if (skippedLines.Count > 0)
			{
				lines.Add("TeamAssignmentsSkippedHeader".LocDef("Not changed:"));
				lines.AddRange(skippedLines);
			}

			int omitted;
			string list = TeamAssignmentResult.PreviewText(lines, MaxPreviewLines, out omitted);
			if (omitted > 0)
			{
				list += "\n" + string.Format("TeamAssignmentsMore".LocDef("... and {0} more"), omitted);
			}

			string preview = string.Format("TeamAssignmentsPreview".LocDef("Apply {0} team change(s)? Scores are the team members with the work's role.\n{1}"), changes.Count, list);
			GameSettings game = Settings;
			Confirmation.Ask("Optimize Team Assignments", preview, () => ApplyTeamRecommendations(game, changes, skipped));
		}

		private static void ApplyTeamRecommendations(GameSettings game, List<TeamRecommendation> changes, int skipped)
		{
			if (!ReferenceEquals(game, Settings))
			{
				Confirmation.ShowScopeChanged();
				return;
			}

			int changed = 0;
			int failed = 0;
			foreach (TeamRecommendation change in changes)
			{
				WorkItem item = change.Item;
				string reason;
				TeamRecommendation current = Settings.MyCompany.WorkItems.Contains(item) && !item.Done ? Recommend(item, out reason) : null;
				if (current == null || !current.SameAs(change))
				{
					skipped++;
					continue;
				}

				Team bestTeam = change.Proposed;
				item.SetDevTeams(new List<string> { bestTeam.Name });
				if (TeamAssignmentResult.Applied(item.DevTeams, bestTeam.Name))
				{
					changed++;
				}
				else
				{
					failed++;
				}
			}

			if (TeamAssignmentResult.ChangedNothing(changed, failed))
			{
				Notification.ShowInfo(SharedMessages.TeamAssignmentsNoChange(skipped));
				return;
			}

			string summary = string.Format("TeamAssignmentsSummary".LocDef("Trainer: Team assignments: {0} changed, {1} skipped, {2} failed."), changed, skipped, failed);
			if (failed == 0)
			{
				Notification.Popup(summary);
			}
			else if (changed > 0)
			{
				Notification.ShowWarning(summary);
			}
			else
			{
				Notification.ShowError(summary);
			}
		}

		private static TeamRecommendation Recommend(WorkItem item, out string reason)
		{
			reason = null;
			Employee.RoleBit? roleBit = GetOptimizerRoleBit(item);
			if (roleBit == null)
			{
				return null;
			}

			if (item.AutoDev)
			{
				reason = "TeamSkipAutoDev".LocDef("automated development");
				return null;
			}

			if (item.DevTeams.Count != 1)
			{
				reason = "TeamSkipTeamCount".LocDef("not exactly one team assigned");
				return null;
			}

			DesignDocument designDocument = item as DesignDocument;
			if (designDocument != null && designDocument.NeedsLead() && designDocument.LeadWork != null)
			{
				reason = "TeamSkipLeadDesign".LocDef("lead design work in progress");
				return null;
			}

			Team currentTeam = GameSettings.GetTeam(item.DevTeams.First());
			if (currentTeam == null)
			{
				reason = "TeamSkipMissingTeam".LocDef("its team no longer exists");
				return null;
			}

			Team bestTeam = currentTeam;
			int currentScore = ScoreTeamForRole(currentTeam, roleBit.Value);
			int bestScore = currentScore;

			foreach (Team team in Settings.sActorManager.Teams.Values)
			{
				int score = ScoreTeamForRole(team, roleBit.Value);
				if (score > bestScore)
				{
					bestScore = score;
					bestTeam = team;
				}
			}

			if (bestTeam == currentTeam)
			{
				reason = string.Format("TeamSkipAlreadyBest".LocDef("{0} ({1}) is already the best team"), currentTeam.Name, currentScore);
				return null;
			}

			return new TeamRecommendation(item, currentTeam, currentScore, bestTeam, bestScore);
		}

		internal static void InstantPorting()
		{
			var ports = Settings.MyCompany.WorkItems.OfType<SoftwarePort>().ToList();

			if (ports.Count == 0)
			{
				Notification.ShowInfo("NoPortingWork".LocDef("Trainer: No active porting work to complete!"));
				return;
			}

			foreach (SoftwarePort port in ports)
			{
				for (int i = 0; i < port.OSs.Count; i++)
				{
					if (!AdvanceSoftwarePort(port))
					{
						break;
					}
				}
			}

			Notification.Popup("PortingCompleted".LocDef("Trainer: Porting work has been completed!"));
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
				Notification.ShowError(string.Format("ProductNotInBeta".LocDef("Trainer: No product named {0} is in beta!"), input));
				return;
			}

			WorkItem.FixedBugs = WorkItem.MaxBugs;
		}

		internal static void FixBugs()
		{
			WindowManager.SpawnInputDialog(SharedMessages.ProductNamePrompt(), "FixBugsTitle".LocDef("Fix Bugs"), "", FixBugsAction);
		}

		internal static void MaxFollowersAction(string input)
		{
			SoftwareAlpha WorkItem = Settings.MyCompany.WorkItems
				.OfType<SoftwareAlpha>().FirstOrDefault(item =>
					item.Name == input && !item.Paused);

			if (WorkItem == null)
			{
				Notification.ShowError(string.Format("NoActiveAlphaNamed".LocDef("No active (not paused) alpha named {0}."), input));
				return;
			}

			WorkItem.ReEvaluateMaxFollowers();
			WorkItem.Followers = WorkItem.MaxFollowers;
		}

		internal static void MaxFollowers()
		{
			WindowManager.SpawnInputDialog(SharedMessages.ProductNamePrompt(), "MaxFollowersTitle".LocDef("Max Followers"), "", MaxFollowersAction);
		}

		internal static void MaxFollowersOldAction(string input)
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

		internal static void MaxFollowersOld()
		{
			WindowManager.SpawnInputDialog(SharedMessages.ProductNamePrompt(), "MaxFollowersOldTitle".LocDef("Max Followers (Old)"), "", MaxFollowersOldAction);
		}

		internal static void ApplyAutoMaxFollowers()
		{
			if (!ExperimentalOptions.Enabled)
			{
				return;
			}

			var betaWorks = Settings.MyCompany.WorkItems
								.OfType<SoftwareAlpha>()
								.Where(item => item.InBeta && !item.Paused && item.contract == null && item.GetWorkOwner() == Settings.MyCompany)
								.ToList();

			foreach (SoftwareAlpha workItem in betaWorks)
			{
				workItem.ReEvaluateMaxFollowers();
				if (IsBelowFollowerCap(workItem.Followers, workItem.MaxFollowers))
				{
					workItem.Followers = workItem.MaxFollowers;
				}
			}
		}

		internal static bool IsBelowFollowerCap(float followers, uint maxFollowers)
		{
			return followers < maxFollowers;
		}

		internal static void InstantResearch()
		{
			var eligibleResearch = UnfinishedResearchWorks();

			if (eligibleResearch.Count == 0)
			{
				Notification.ShowInfo("NoActiveResearch".LocDef("Trainer: No active research to complete!"));
				return;
			}

			eligibleResearch.ForEach(FinishResearchNow);

			Notification.Popup("ResearchCompleted".LocDef("Trainer: Research completed instantly!"));
		}

		internal static void ApplyAutoInstantResearch()
		{
			if (!ExperimentalOptions.Enabled)
			{
				return;
			}

			foreach (ResearchWork researchWork in UnfinishedResearchWorks())
			{
				if (!IsResearchObsolete(researchWork))
				{
					FinishResearchNow(researchWork);
				}
			}
		}

		internal static void InstantPatent()
		{
			var patentWorks = PendingPatentWorks();

			if (patentWorks.Count == 0)
			{
				Notification.ShowInfo("NoPatentWork".LocDef("Trainer: No patent work to complete!"));
				return;
			}

			int patented = 0;
			int notPatentable = 0;
			int unaffordable = 0;

			foreach (LegalWork legalWork in patentWorks)
			{
				switch (TryPatentNow(legalWork))
				{
					case PatentOutcome.Patented:
						patented++;
						break;
					case PatentOutcome.Unaffordable:
						unaffordable++;
						break;
					default:
						notPatentable++;
						break;
				}
			}

			string summary = string.Format("PatentsSummary".LocDef("Trainer: Patents: {0} filed, {1} skipped for lack of money, {2} skipped as not patentable."), patented, unaffordable, notPatentable);
			if (unaffordable == 0 && notPatentable == 0)
			{
				Notification.Popup(summary);
			}
			else
			{
				Notification.ShowWarning(summary);
			}
		}

		internal static void ApplyAutoInstantPatent()
		{
			if (!ExperimentalOptions.Enabled)
			{
				return;
			}

			foreach (LegalWork legalWork in PendingPatentWorks())
			{
				TryPatentNow(legalWork);
			}
		}

		internal static void ApplyAutoEndDesign()
		{
			var designDocuments = Settings.MyCompany.WorkItems
								.OfType<DesignDocument>()
								.Where(d => d.HasFinished && (!d.NeedsLead() || d.LeadWork != null))
								.ToList();

			designDocuments.ForEach(designDocument =>
			{
				designDocument.PromoteAction();
			});
		}

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

		private const int MaxPreviewLines = 20;

		private sealed class TeamRecommendation
		{
			public readonly WorkItem Item;
			public readonly Team Current;
			public readonly int CurrentScore;
			public readonly Team Proposed;
			public readonly int ProposedScore;

			public TeamRecommendation(WorkItem item, Team current, int currentScore, Team proposed, int proposedScore)
			{
				Item = item;
				Current = current;
				CurrentScore = currentScore;
				Proposed = proposed;
				ProposedScore = proposedScore;
			}

			public bool SameAs(TeamRecommendation other)
			{
				return Item == other.Item && Current == other.Current && CurrentScore == other.CurrentScore && Proposed == other.Proposed && ProposedScore == other.ProposedScore;
			}
		}

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

		private static void CompleteResearchWork(ResearchWork researchWork)
		{
			Settings.MyCompany.AddResearch(researchWork.Spec, researchWork.Year);
			TechLevel tech = Settings.simulation.AddTechLevel(researchWork.Spec, researchWork.Year, SDateTime.Now(), true);
			if (tech != null)
			{
				LegalWork legalWork = new LegalWork(tech);
				Settings.MyCompany.AddWorkItem(legalWork);
				Settings.ApplyDefaultTeams(legalWork, legalWork.Type + "Team");
			}
			HUD.Instance.researchWindow.UpdateLists();
			researchWork.Kill(false);
		}

		private static List<ResearchWork> UnfinishedResearchWorks()
		{
			return Settings.MyCompany.WorkItems
					.OfType<ResearchWork>()
					.Where(rw => !rw.Finished)
					.ToList();
		}

		private static void FinishResearchNow(ResearchWork researchWork)
		{
			researchWork.Progress = researchWork.Max;
			researchWork.Finished = true;
			CompleteResearchWork(researchWork);
		}

		private static bool IsResearchObsolete(ResearchWork researchWork)
		{
			List<TechLevel> techLevels;
			if (!MarketSimulation.Active.TechLevels.TryGetValue(researchWork.Spec, out techLevels) || techLevels.Count == 0)
			{
				return true;
			}

			return techLevels[techLevels.Count - 1].Year > researchWork.Year || Settings.MyCompany.GetLatestResearch(researchWork.Spec, -1) >= researchWork.Year;
		}

		private static List<LegalWork> PendingPatentWorks()
		{
			return Settings.MyCompany.WorkItems
					.OfType<LegalWork>()
					.Where(lw => lw.Type == LegalWork.WorkType.Patent && !lw.Done)
					.ToList();
		}

		private static PatentOutcome TryPatentNow(LegalWork legalWork)
		{
			if (!CanPatentNow(legalWork))
			{
				return PatentOutcome.NotPatentable;
			}

			if (!Settings.MyCompany.CanMakeTransaction(-TechLevel.PatentPrice))
			{
				return PatentOutcome.Unaffordable;
			}

			if (legalWork.Progress < 1f)
			{
				legalWork.Progress = 1f;
			}

			legalWork.PatentNow();
			return PatentOutcome.Patented;
		}

		private static bool CanPatentNow(LegalWork legalWork)
		{
			TechLevel patent = legalWork.Patent;
			return !legalWork.Done
				&& legalWork.Type == LegalWork.WorkType.Patent
				&& Settings.MyCompany.WorkItems.Contains(legalWork)
				&& legalWork.GetNetworkDealState() == WorkItem.NetworkDealState.None
				&& patent != null
				&& patent.CanPatent
				&& TimeOfDay.Instance.Year - patent.Year <= TechLevel.PatentExpiration
				&& patent.PatentOwner == 0;
		}

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

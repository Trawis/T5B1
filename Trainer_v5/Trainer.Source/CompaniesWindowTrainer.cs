using System;
using System.Collections.Generic;
using UnityEngine;

namespace Trainer_v5
{
	internal static class CompaniesWindowTrainer
	{
		// The Companies window the controls were added to; the game recreates it with the HUD on save reloads.
		private static CompanyWindow _installedOn;

		public static void Reset()
		{
			_installedOn = null;
		}

		public static void Install()
		{
			var target = HUD.Instance?.companyWindow;
			if (target == null || target.Window == null || target == _installedOn)
				return;

			var components = new List<Component>
			{
				UIFactory.Button("CompanyAdjustFundsButton".LocDef("Adjust AI Funds"), () => WithSelectedCompany(CompanyFeatures.AdjustAIFunds)),
				UIFactory.Button("CompanyForceBankruptButton".LocDef("Force Bankrupt"), () => WithSelectedCompany(CompanyFeatures.AskForceBankrupt)),
				UIFactory.Button("CompanyTakeOverButton".LocDef("Take Over"), () => WithSelectedCompany(CompanyFeatures.TakeOver)),
				UIFactory.Button("CompanyMakeSubsidiaryButton".LocDef("Make Subsidiary"), () => WithSelectedCompany(CompanyFeatures.MakeSubsidiary)),
				UIFactory.Button("CompanySetReputationButton".LocDef("Set Reputation"), () => WithSelectedCompany(CompanyFeatures.SetAIBusinessReputation)),
			};

			const int width = 120, height = 32, spacing = 4;
			var windowRect = ((RectTransform)target.Window.transform).rect;
			var slots = DetailButtonLayout.Place(components.Count, windowRect.width, width, height, spacing);
			var placed = false;
			try
			{
				// Above the window's top edge, so no list row, filter, header or scrollbar of the native window is covered.
				for (var i = 0; i < components.Count; i++)
				{
					UIHelper.PlaceInElement(components[i].gameObject, target.Window.gameObject, slots[i]);
				}

				placed = true;
			}
			finally
			{
				if (!placed)
				{
					for (var i = 0; i < components.Count; i++)
					{
						UnityEngine.Object.Destroy(components[i].gameObject);
					}
				}
			}

			if (!placed)
				return;

			_installedOn = target;
		}

		// The company is captured at the click; each action checks it again before a deferred write.
		private static void WithSelectedCompany(Action<SimulatedCompany> action)
		{
			var company = RequireSelectedCompany();
			if (company != null)
			{
				action(company);
			}
		}

		// Exactly one row selected in the shown Companies list, holding a live AI company that is not going bankrupt.
		private static SimulatedCompany RequireSelectedCompany()
		{
			var window = HUD.Instance != null ? HUD.Instance.companyWindow : null;
			var selected = window != null && window.Window != null && window.Window.Shown && window.CompanyList != null
				? window.CompanyList.GetSelected<object>()
				: new object[0];
			if (selected.Length != 1)
			{
				Notification.ShowError("SelectOneCompany".LocDef("Select exactly one company in the Companies list for this action."));
				return null;
			}

			var company = selected[0] as SimulatedCompany;
			if (company == null || !CompanyFeatures.IsLiveAICompany(company))
			{
				Notification.ShowError("SelectAICompany".LocDef("Select an AI company. Your own company and its subsidiaries are not accepted."));
				return null;
			}

			if (company.Bankrupt)
			{
				Notification.ShowInfo(SharedMessages.CompanyAlreadyBankrupt(company.Name));
				return null;
			}

			return company;
		}
	}
}

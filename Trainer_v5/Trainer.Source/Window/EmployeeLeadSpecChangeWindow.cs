﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
// Removed using Trainer_v5.Window; as it seems unused now
using UnityEngine;
using UnityEngine.UI;
using Trainer_v5;

namespace Trainer_v5.Window
{
	public class EmployeeLeadSpecChangeWindow : MonoBehaviour
	{
		public static EmployeeLeadSpecChangeWindow Instance => _instance.Value;
		private static readonly Lazy<EmployeeLeadSpecChangeWindow> _instance = new Lazy<EmployeeLeadSpecChangeWindow>(() => new EmployeeLeadSpecChangeWindow());

		private GUIWindow _window;
		private Dictionary<string, SoftwareType> _softwareTypes;
		private Dictionary<string, Toggle> _specToggles;
		private Actor _actor;

		public void Show()
		{
			if (_window == null)
				CreateWindow();
			else
				_window.Toggle();
			Refresh();
		}

		public void Refresh()
		{
			var window = _window;
			if (window == null || !window.Shown)
			{
				_actor = null;
				return;
			}

			var selectedActors = SelectorController.Instance.Selected.OfType<Actor>();
			_actor = selectedActors.Any() ? selectedActors.First() : null;
			var employee = _actor?.employee;

			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit lead specialization for {employee?.Name ?? "Nobody"}";
		}

		private void CreateWindow()
		{
			var self = this;
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = "Edit lead specialization for Nobody";
			window.name = "EditLeadSpec";
			window.MainPanel.name = "MainPanel";

			var softwareTypes = MarketSimulation.Active.SoftwareTypes;
			var toggles = new Dictionary<string, Toggle>();
			var columnItems = new List<GameObject>();

			columnItems.Add(UIHelper.CreateLabel("Lead Spec", name: "LeadSpecTitle"));

			foreach (var pair in softwareTypes)
			{
				var toggleGO = UIHelper.CreateToggle(pair.Key, false, isOn => self.OnToggle(pair.Key, isOn));
				toggles[pair.Key] = toggleGO.GetComponent<Toggle>();
				columnItems.Add(toggleGO);
			}

			columnItems.Add(UIHelper.CreateButton("All", () => self.ToggleAll(true)));
			columnItems.Add(UIHelper.CreateButton("None", () => self.ToggleAll(false)));
			columnItems.Add(UIHelper.CreateButton("Set LeadSpec", () => self.SetLeadSpec()));

			columnItems.AddToWindow(window, Constants.FIRST_COLUMN);

			window.SetWindowSize(columnItems.Count, Constants.SECOND_COLUMN - 1);

			_window = window;
			_softwareTypes = softwareTypes;
			_specToggles = toggles;
		}

		private void OnToggle(string key, bool isOn)
		{
			_specToggles[key].isOn = isOn;
		}

		private void ToggleAll(bool isOn)
		{
			foreach (var toggle in _specToggles.Values)
				toggle.isOn = isOn;
		}

		private void SetLeadSpec()
		{
			if (_actor == null)
				return;
			var employee = _actor.employee;

			var selectTypes = _specToggles
				.Where(p => p.Value.isOn)
				.Select(p => _softwareTypes[p.Key])
				.ToArray();

			if (selectTypes.Length == 0)
			{
				Notification.ShowError("Select one or more LeadSpec.");
				return;
			}

			InputHelper.RequestFloat(
				"How many LeadSpec do you want?\nMin = 0, Max = 1.0",
				$"Set {selectTypes.Length} LeadSpec(s) for {employee.Name}",
				val =>
				{
					foreach (var type in selectTypes)
					{
#if DEBUG || SWINCBETA1_7 || SWINCBETA1_8 || SWINCBETA1_9 || SWINCBETA1_10
						employee.LeadSpecializationFix[type.ToString()] = val;
#else
						employee.LeadSpecialization[type] = val;
#endif
					}
				},
				min: 0, max: 1
				);
		}
	}
}

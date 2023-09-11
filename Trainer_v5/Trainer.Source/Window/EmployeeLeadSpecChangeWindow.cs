using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using Trainer_v5.Window;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Trainer.Source.Window
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
			foreach (var pair in softwareTypes)
			{
				var toggle = UIFactory.Toggle(pair.Key, false, isOn => self.OnToggle(pair.Key, isOn));
				toggles[pair.Key] = toggle;
			}

			var col1 = new VerticalLayout()
			{
				Gap = 2,
				Components = LayoutHelper.EnumerableOf(
					UIFactory.Label("Lead Spec", WindowStyles.TitleStyle),
					toggles.Values.ToArray(),
					UIFactory.Button("All", () => self.ToggleAll(true)),
					UIFactory.Button("None", () => self.ToggleAll(false)),
					UIFactory.Button("Set LeadSpec", () => self.SetLeadSpec())
					).ToList()
			};
			window.Add(col1, new Rect(4, 4, 160, 0));

			var maxHeight = new[] { col1.PreferHeight }.Max();
			window.SetMinSize(168, maxHeight);

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
						employee.LeadSpecializationFix[type.ToString()] = val;
				},
				min: 0, max: 1
				);
		}
	}
}

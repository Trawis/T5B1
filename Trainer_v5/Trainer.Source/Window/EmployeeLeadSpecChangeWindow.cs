using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using Trainer_v5.Window;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Trainer.Source.Window
{
	public class EmployeeLeadSpecChangeWindow
	{
		private static readonly Lazy<EmployeeLeadSpecChangeWindow> _instance = new Lazy<EmployeeLeadSpecChangeWindow>(() => new EmployeeLeadSpecChangeWindow());
		private GUIWindow _window;
		private Dictionary<string, SoftwareType> _softwareTypes;
		private Dictionary<string, Toggle> _specToggles;
		private EmployeeTarget _target;
		private bool _isRefreshing;

		public static EmployeeLeadSpecChangeWindow Instance => _instance.Value;

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
				_target = null;
				return;
			}

			TargetSource source;
			_target = EmployeeTarget.Resolve(out source);

			var employee = _target?.Employee;
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit lead specialization for {EmployeeTarget.Describe(_target, source)}";

			_isRefreshing = true;
			foreach (var pair in _specToggles)
			{
				float value;
				var isOn = employee != null && employee.LeadSpecializationFix.TryGetValue(pair.Key, out value) && value > 0f;
				pair.Value.isOn = isOn;
			}
			_isRefreshing = false;
		}

		private void CreateWindow()
		{
			var self = this;
			var window = UIHelper.SpawnWindow("Edit lead specialization for Nobody", "EditLeadSpec", "MainPanel");

			var softwareTypes = MarketSimulation.Active.SoftwareTypes;
			var toggles = new Dictionary<string, Toggle>();
			foreach (var pair in softwareTypes)
			{
				var toggle = UIFactory.Toggle(pair.Key, false, isOn => self.OnToggle(pair.Key, isOn));
				toggles[pair.Key] = toggle;
			}

			var typeKeys = softwareTypes.Keys.ToList();
			int half = (typeKeys.Count + 1) / 2;

			var col1Toggles = typeKeys.Take(half).Select(k => toggles[k].gameObject).ToArray();
			var col2Toggles = typeKeys.Skip(half).Select(k => toggles[k].gameObject).ToArray();

			var col1Header = new[] { UIFactory.Label("Lead Spec", WindowStyles.TitleStyle).gameObject };
			var col1Footer = new[]
			{
				UIFactory.Button("All",          () => self.ToggleAll(true)).gameObject,
				UIFactory.Button("None",         () => self.ToggleAll(false)).gameObject,
				UIFactory.Button("Set LeadSpec", () => self.SetLeadSpec()).gameObject
			};

			var col2Header = new[] { UIFactory.Label("", WindowStyles.TitleStyle).gameObject };
			var col2Footer = new GameObject[0];

			const int colWidth = 160, padding = 4, colGap = 8, gap = 2;

			var reservedRows = Math.Max(col1Header.Length + col1Footer.Length, col2Header.Length + col2Footer.Length);
			var maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, reservedRows);

			var col1Height = UIHelper.CreateScrollableColumn(
				window, new Rect(padding, padding, colWidth, 0),
				col1Header, col1Toggles, col1Footer,
				Constants.ELEMENT_HEIGHT, gap, maxVisibleRows);

			var col2Height = UIHelper.CreateScrollableColumn(
				window, new Rect(padding + colWidth + colGap, padding, colWidth, 0),
				col2Header, col2Toggles, col2Footer,
				Constants.ELEMENT_HEIGHT, gap, maxVisibleRows);

			int totalWidth = padding * 2 + colWidth * 2 + colGap;
			int totalHeight = UIHelper.GetColumnWindowHeight(col1Height, col2Height, padding);
			window.SetMinSize(totalWidth, totalHeight);

			_window = window;
			_softwareTypes = softwareTypes;
			_specToggles = toggles;
		}

		private void OnToggle(string key, bool isOn)
		{
			if (_isRefreshing)
				return;

			_specToggles[key].isOn = isOn;
		}

		private void ToggleAll(bool isOn)
		{
			foreach (var toggle in _specToggles.Values)
				toggle.isOn = isOn;
		}

		private void SetLeadSpec()
		{
			if (_target == null || !_target.IsStillValid())
			{
				Notification.ShowError("SelectEmployeeFirst".LocDef("Select an employee first (select one employee, then close and reopen this window)."));
				return;
			}
			var target = _target;
			var employee = target.Employee;

			var selectTypes = _specToggles
				.Where(p => p.Value.isOn)
				.Select(p => _softwareTypes[p.Key])
				.ToArray();

			if (selectTypes.Length == 0)
			{
				Notification.ShowError("SelectLeadSpecError".LocDef("Select one or more LeadSpec."));
				return;
			}

			InputHelper.RequestFloat(
				"How many LeadSpec do you want?\nMin = 0, Max = 1.0",
				$"Set {selectTypes.Length} LeadSpec(s) for {employee.Name}",
				val =>
				{
					if (!target.IsStillValid())
					{
						Notification.ShowError("EmployeeChangedNothingEdited".LocDef("The employee changed or is not available. Nothing was edited."));
						return;
					}

					foreach (var type in selectTypes)
					{
						employee.LeadSpecializationFix[type.ToString()] = val;
					}
				},
				min: 0, max: 1
			);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Window
{
	internal class EmployeeTraitChangeWindow : SingleEmployeeEditor
	{
		private static readonly Lazy<EmployeeTraitChangeWindow> _instance = new Lazy<EmployeeTraitChangeWindow>(() => new EmployeeTraitChangeWindow());
		private Dictionary<Employee.Trait, Toggle> _traitsToggles;

		public static EmployeeTraitChangeWindow Instance => _instance.Value;

		protected override string Title(string employee)
		{
			return SharedMessages.EditTraitsTitle(employee);
		}

		protected override void ShowEmployee(Employee employee)
		{
			foreach (var pair in _traitsToggles)
			{
				pair.Value.isOn = employee != null && employee.HasTrait(pair.Key);
			}
		}

		protected override GUIWindow CreateWindow()
		{
			var self = this;
			var window = UIHelper.SpawnWindow(SharedMessages.EditTraitsTitle(EmployeeTarget.Describe(null, TargetSource.None)), "EditTrait", "EditTraitPanel");

			var traits = EmployeeHelper.Traits.ToDictionary(t => t, t =>
				UIFactory.Toggle(t.ToString(), false, on => ToggleTrait(t, on)));

			var goodTraitsToggle = traits
				.Where(p => p.Key.IsGood())
				.Select(p => p.Value.gameObject)
				.ToArray();
			var neutralTraitsToggle = traits
				.Where(p => p.Key.IsNeutral())
				.Select(p => p.Value.gameObject)
				.ToArray();
			var badTraitsToggle = traits
				.Where(p => p.Key.IsBad())
				.Select(p => p.Value.gameObject)
				.ToArray();

			var col1Header = new[] { UIFactory.Label("GoodTraitsLabel".LocDef("Good"), WindowStyles.TitleStyle).gameObject };
			var col1Footer = new[] { UIFactory.Button(SharedMessages.EditorRefreshButton(), () => self.Refresh()).gameObject };
			var col2Header = new[] { UIFactory.Label("NeutralTraitsLabel".LocDef("Neutral"), WindowStyles.TitleStyle).gameObject };
			var col3Header = new[] { UIFactory.Label("BadTraitsLabel".LocDef("Bad"), WindowStyles.TitleStyle).gameObject };
			var noFooter = new GameObject[0];

			var maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, col1Header.Length + col1Footer.Length);

			var col1Height = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.FIRST_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				col1Header, goodTraitsToggle, col1Footer,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);
			var col2Height = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.SECOND_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				col2Header, neutralTraitsToggle, noFooter,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);
			var col3Height = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.THIRD_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				col3Header, badTraitsToggle, noFooter,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);

			window.SetMinSize(Constants.FOURTH_COLUMN - 1, UIHelper.GetColumnWindowHeight(UIHelper.GetColumnWindowHeight(col1Height, col2Height, 0), col3Height, 0));

			_traitsToggles = traits;
			return window;
		}

		private void ToggleTrait(Employee.Trait trait, bool on)
		{
			var employee = EditableEmployee();
			if (employee == null)
				return;

			var hasTrait = (employee.Traits & trait) > 0;
			if (hasTrait != on)
				employee.Traits ^= trait;
		}
	}
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;
using Trainer_v5;

namespace Trainer_v5.Window
{
	public class EmployeeTraitChangeWindow : MonoBehaviour
	{
		public static EmployeeTraitChangeWindow Instance => _instance.Value;
		private static readonly Lazy<EmployeeTraitChangeWindow> _instance = new Lazy<EmployeeTraitChangeWindow>(() => new EmployeeTraitChangeWindow());

		private GUIWindow _window;
		private Dictionary<Employee.Trait, Toggle> _traitsToggles;
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

			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit traits for {employee?.Name ?? "Nobody"}";

			foreach (var pair in _traitsToggles)
			{
				var isOn = employee?.HasTrait(pair.Key) ?? false;
				pair.Value.isOn = isOn;
			}
		}

		private void CreateWindow()
		{
			var self = this;
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = "Edit traits for Nobody";
			window.name = "EditTrait";
			window.MainPanel.name = "EditTraitPanel";

			var traits = EmployeeHelper.Traits.ToDictionary(t => t, t =>
				UIHelper.CreateToggle(t.ToString(), false, on => ToggleTrait(t, on)).GetComponent<Toggle>());

			var goodTraitsToggle = traits
				.Where(p => p.Key.IsGood())
				.Select(p => p.Value.gameObject);
			var neutralTraitsToggle = traits
				.Where(p => p.Key.IsNeutral())
				.Select(p => p.Value.gameObject);
			var badTraitsToggle = traits
				.Where(p => p.Key.IsBad())
				.Select(p => p.Value.gameObject);

			var col1 = new List<GameObject> { UIHelper.CreateLabel("Good", name: "GoodTitle") };
			col1.AddRange(goodTraitsToggle);
			col1.Add(UIHelper.CreateButton("Refresh", () => self.Refresh()));

			var col2 = new List<GameObject> { UIHelper.CreateLabel("Neutral", name: "NeutralTitle") };
			col2.AddRange(neutralTraitsToggle);

			var col3 = new List<GameObject> { UIHelper.CreateLabel("Bad", name: "BadTitle") };
			col3.AddRange(badTraitsToggle);

			col1.AddToWindow(window, Constants.FIRST_COLUMN);
			col2.AddToWindow(window, Constants.SECOND_COLUMN);
			col3.AddToWindow(window, Constants.THIRD_COLUMN);

			var maxRows = new[] { col1.Count, col2.Count, col3.Count }.Max();
			window.SetWindowSize(maxRows + 1, Constants.FOURTH_COLUMN - 1);

			_window = window;
			_traitsToggles = traits;
		}

		private void ToggleTrait(Employee.Trait trait, bool on)
		{
			if (_actor == null)
				return;
			var employee = _actor.employee;

			var hasTrait = (employee.Traits & trait) > 0;
			if (hasTrait != on)
				employee.Traits ^= trait;
		}
	}
}

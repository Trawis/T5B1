﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;
using Trainer_v5;

namespace Trainer_v5.Window
{
	public class EmployeeDemandChangeWindow : MonoBehaviour
	{
		public static EmployeeDemandChangeWindow Instance => _instance.Value;
		private static readonly Lazy<EmployeeDemandChangeWindow> _instance = new Lazy<EmployeeDemandChangeWindow>(() => new EmployeeDemandChangeWindow());
		
		private GUIWindow _window;
		private Dictionary<LeadDesignDemands.Demand, Toggle> _demandToggles;
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

			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit demands for {employee?.Name ?? "Nobody"}";

			foreach (var pair in _demandToggles)
			{
				var isOn = employee?.HasDemanded(pair.Key) ?? false;
				pair.Value.isOn = isOn;
			}
		}

		private void CreateWindow()
		{
			var self = this;
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = "Edit demands for Nobody";
			window.name = "EditDemands";
			window.MainPanel.name = "EditDemandsPanel";

			var demands = EmployeeHelper.Demands.ToDictionary(t => t, t =>
				UIHelper.CreateToggle(t.ToString(), false, on => ToggleDemand(t, on)).GetComponent<Toggle>());

			var col1 = new List<GameObject> { UIHelper.CreateLabel("Demands", name: "DemandsTitle") };
			col1.AddRange(demands.Values.Select(e => e.gameObject));
			col1.Add(UIHelper.CreateButton("Refresh", () => self.Refresh()));

			col1.AddToWindow(window, Constants.FIRST_COLUMN);

			var maxRows = col1.Count;
			window.SetWindowSize(maxRows + 1, Constants.SECOND_COLUMN - 1);

			_window = window;
			_demandToggles = demands;
		}

		private void ToggleDemand(LeadDesignDemands.Demand demand, bool on)
		{
			if (_actor == null)
				return;
			var employee = _actor.employee;

			var has = (employee.DemandResults & demand) > 0;
			if (has != on)
				employee.DemandResults ^= demand;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;
using Utils = Trainer_v5.Utilities;

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
			window.name = "EdiTrait";
			window.MainPanel.name = "EdiTraitPanel";

			var demands = EmployeeHelper.Demands.ToDictionary(t => t, t => 
				UIFactory.Toggle(t.ToString(), false, on => ToggleDemand(t, on)));

			var col1 = new List<GameObject> { UIFactory.Label("Demands", WindowStyles.TitleStyle).gameObject };
			col1.AddRange(demands.Values.Select(e => e.gameObject));
			col1.Add(UIFactory.Button("Refresh", () => self.Refresh()).gameObject);

			Utils.CreateGameObjects(Constants.FIRST_COLUMN,  col1.ToArray(), window);

			var maxRows = new[] { col1.Count }.Max();
			Utils.SetWindowSize(maxRows + 1, Constants.SECOND_COLUMN - 1, window);

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
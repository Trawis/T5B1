using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Window
{
	public class EmployeeDemandChangeWindow
	{
		public static EmployeeDemandChangeWindow Instance => _instance.Value;

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

			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit demands for {EmployeeTarget.Describe(_target, source)}";

			_isRefreshing = true;
			foreach (var pair in _demandToggles)
			{
				var isOn = employee?.HasDemanded(pair.Key) ?? false;
				pair.Value.isOn = isOn;
			}
			_isRefreshing = false;
		}

		private static readonly Lazy<EmployeeDemandChangeWindow> _instance = new Lazy<EmployeeDemandChangeWindow>(() => new EmployeeDemandChangeWindow());

		private GUIWindow _window;
		private Dictionary<LeadDesignDemands.Demand, Toggle> _demandToggles;
		private EmployeeTarget _target;
		private bool _isRefreshing;

		private void CreateWindow()
		{
			var self = this;
			var window = UIHelper.SpawnWindow("Edit demands for Nobody", "EditDemands", "EditDemandsPanel");

			var demands = EmployeeHelper.Demands.ToDictionary(t => t, t =>
				UIFactory.Toggle(t.ToString(), false, on => ToggleDemand(t, on)));

			var col1 = new List<GameObject> { UIFactory.Label("Demands", WindowStyles.TitleStyle).gameObject };
			col1.AddRange(demands.Values.Select(e => e.gameObject));
			col1.Add(UIFactory.Button("Refresh", () => self.Refresh()).gameObject);

			col1.AddToWindow(window, Constants.FIRST_COLUMN);

			var maxRows = new[] { col1.Count }.Max();
			window.SetWindowSize(maxRows + 1, Constants.SECOND_COLUMN - 1);

			_window = window;
			_demandToggles = demands;
		}

		private void ToggleDemand(LeadDesignDemands.Demand demand, bool on)
		{
			if (_isRefreshing)
				return;

			if (_target == null || !_target.IsStillValid())
			{
				if (!_isRefreshing)
					Notification.ShowError("The employee changed or is not available. Press Refresh.");
				return;
			}
			var employee = _target.Employee;

			var choice = LeadDesignDemands.GetChoice(demand);
			if (choice == null)
			{
				Notification.ShowError("The game has no demand choice for " + demand + ", so it was not changed.");
				Refresh();
				return;
			}

			var flag = (uint)(int)demand;
			var sibling = (uint)(int)(choice.Choice1 == demand ? choice.Choice2 : choice.Choice1);
			var has = employee.HasDemanded(demand);

			if (on && !has)
			{
				GrantDemand(employee, choice, demand, sibling);
			}
			else if (on || has)
			{
				// Already granted (repairs state an older trainer left half-written) or being removed: the game has
				// no removal path, so the three bit sets are changed together.
				var current = new DemandBits((uint)(int)employee.DemandResults, employee.DemandsMet, employee.DemandsRequested);
				var next = DemandBits.Apply(current, flag, sibling, choice.ID, on);
				employee.DemandResults = (LeadDesignDemands.Demand)(int)next.Results;
				employee.DemandsMet = next.Met;
				employee.DemandsRequested = next.Requested;
			}

			// Show what the game now reports, including the paired choice that was switched off.
			Refresh();
		}

		// Granting goes through the game's own AcceptDemand, so the demand's effect runs (role change for Exclusive
		// Lead, leaving the desk for Private Office) and the bits are set the way the game sets them. AcceptDemand
		// also charges the employer for the Luxury Car; the trainer gives that money back.
		private static void GrantDemand(Employee employee, LeadDesignDemands.DemandChoice choice, LeadDesignDemands.Demand demand, uint sibling)
		{
			employee.DemandResults = (LeadDesignDemands.Demand)((int)employee.DemandResults & ~(int)sibling);

			var index = choice.GetChoiceIndex(demand);
			employee.AcceptDemand(choice, index, false);

			var cost = index == 0 ? choice.Cost1 : choice.Cost2;
			if (cost > 0f && employee.MyEmployer != null)
			{
				employee.MyEmployer.MakeTransaction(cost, Company.TransactionCategory.Deals);
			}
		}
	}
}

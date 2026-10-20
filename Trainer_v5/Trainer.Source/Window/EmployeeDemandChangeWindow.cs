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
		private static readonly Lazy<EmployeeDemandChangeWindow> _instance = new Lazy<EmployeeDemandChangeWindow>(() => new EmployeeDemandChangeWindow());
		private GUIWindow _window;
		private Dictionary<LeadDesignDemands.Demand, Toggle> _demandToggles;
		private EmployeeTarget _target;
		private bool _isRefreshing;

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
			TargetSource source;
			if (!EmployeeTarget.TryResolveFor(window, out _target, out source))
				return;

			var employee = _target?.Employee;

			window.SetTitle(SharedMessages.EditDemandsTitle(EmployeeTarget.Describe(_target, source)));

			_isRefreshing = true;
			foreach (var pair in _demandToggles)
			{
				var isOn = employee?.HasDemanded(pair.Key) ?? false;
				pair.Value.isOn = isOn;
			}
			_isRefreshing = false;
		}

		private void CreateWindow()
		{
			var self = this;
			var window = UIHelper.SpawnWindow(SharedMessages.EditDemandsTitle(EmployeeTarget.Describe(null, TargetSource.None)), "EditDemands", "EditDemandsPanel");

			var demands = EmployeeHelper.Demands.ToDictionary(t => t, t =>
				UIFactory.Toggle(t.ToString(), false, on => ToggleDemand(t, on)));

			var header = new[] { UIFactory.Label("DemandsLabel".LocDef("Demands"), WindowStyles.TitleStyle).gameObject };
			var footer = new[] { UIFactory.Button(SharedMessages.EditorRefreshButton(), () => self.Refresh()).gameObject };
			var maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, header.Length + footer.Length);

			var height = UIHelper.CreateScrollableColumn(
				window, new Rect(Constants.FIRST_COLUMN, 0, Constants.ELEMENT_WIDTH, 0),
				header, demands.Values.Select(e => e.gameObject).ToArray(), footer,
				Constants.ELEMENT_HEIGHT, 0, maxVisibleRows);

			window.SetMinSize(Constants.SECOND_COLUMN - 1, UIHelper.GetColumnWindowHeight(height, 0, 0));

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
					Notification.ShowError(SharedMessages.EmployeeChangedPressRefresh());
				return;
			}
			var employee = _target.Employee;

			var choice = LeadDesignDemands.GetChoice(demand);
			if (choice == null)
			{
				Notification.ShowError(string.Format("NoDemandChoice".LocDef("The game has no demand choice for {0}, so it was not changed."), demand));
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
				// Repairs half-written state or removes (the game has no removal path): the three bit sets change together.
				var current = new DemandBits((uint)(int)employee.DemandResults, employee.DemandsMet, employee.DemandsRequested);
				var next = DemandBits.Apply(current, flag, sibling, choice.ID, on);
				employee.DemandResults = (LeadDesignDemands.Demand)(int)next.Results;
				employee.DemandsMet = next.Met;
				employee.DemandsRequested = next.Requested;
			}

			// Show what the game now reports, including the paired choice that was switched off.
			Refresh();
		}

		// Grants via the game's AcceptDemand so the effect runs; its cost is charged as in the game (a refund would pay twice with the dismissal refund).
		private static void GrantDemand(Employee employee, LeadDesignDemands.DemandChoice choice, LeadDesignDemands.Demand demand, uint sibling)
		{
			employee.DemandResults = (LeadDesignDemands.Demand)((int)employee.DemandResults & ~(int)sibling);

			var index = choice.GetChoiceIndex(demand);
			employee.AcceptDemand(choice, index, false);
		}
	}
}

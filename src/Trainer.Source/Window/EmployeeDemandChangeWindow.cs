using System;
using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5.Window
{
	internal class EmployeeDemandChangeWindow : SingleEmployeeEditor
	{
		private static readonly Lazy<EmployeeDemandChangeWindow> _instance = new Lazy<EmployeeDemandChangeWindow>(() => new EmployeeDemandChangeWindow());
		private Dictionary<LeadDesignDemands.Demand, Toggle> _demandToggles;

		public static EmployeeDemandChangeWindow Instance => _instance.Value;

		protected override string Title(string employee)
		{
			return SharedMessages.EditDemandsTitle(employee);
		}

		protected override void ShowEmployee(Employee employee)
		{
			foreach (var pair in _demandToggles)
			{
				pair.Value.isOn = employee != null && employee.HasDemanded(pair.Key);
			}
		}

		protected override GUIWindow CreateWindow()
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

			_demandToggles = demands;
			return window;
		}

		private void ToggleDemand(LeadDesignDemands.Demand demand, bool on)
		{
			var employee = EditableEmployee();
			if (employee == null)
				return;

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

			Refresh();
		}

		// Grants via AcceptDemand so the effect runs; its cost stays charged (a refund would pay twice with the dismissal refund).
		private static void GrantDemand(Employee employee, LeadDesignDemands.DemandChoice choice, LeadDesignDemands.Demand demand, uint sibling)
		{
			employee.DemandResults = (LeadDesignDemands.Demand)((int)employee.DemandResults & ~(int)sibling);

			var index = choice.GetChoiceIndex(demand);
			employee.AcceptDemand(choice, index, false);
		}
	}
}

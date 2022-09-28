using System.Collections.Generic;
using System.Linq;
using Trainer_v5.SDK;
using UnityEngine;
using UnityEngine.UI;
using Utils = Trainer_v5.Utilities;


namespace Trainer_v5.Window
{
	public class EmployeeTraitChangeWindow : MonoBehaviour
	{
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

			// get select actor
			var selectedActors = SelectorController.Instance.Selected.OfType<Actor>();
			_actor = selectedActors.Any() ? selectedActors.First() : null;
			var employee = _actor?.employee;

			// refresh title
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = $"Edit traits for {employee?.Name ?? "Nobody"}";

			// refresh toggles
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
			window.name = "EdiTrait";
			window.MainPanel.name = "EdiTraitPanel";

			var traits = EmployeeHelper.Traits.ToDictionary(t => t, t => 
				UIFactory.Toggle(t.ToString(), false, on => ToggleTrait(t, on)));

			var goodTraitsToggle = traits
				.Where(p => p.Key.IsGood())
				.Select(p => p.Value.gameObject);
			var neutralTraitsToggle = traits
				.Where(p => p.Key.IsNeutral())
				.Select(p => p.Value.gameObject);
			var badTraitsToggle = traits
				.Where(p => p.Key.IsBad())
				.Select(p => p.Value.gameObject);

			var col1 = new List<GameObject> { UIFactory.Label("Good", WindowStyles.TitleStyle).gameObject };
			col1.AddRange(goodTraitsToggle);
			col1.Add(UIFactory.Button("Refresh", () => self.Refresh()).gameObject);

			var col2 = new List<GameObject> { UIFactory.Label("Neutral", WindowStyles.TitleStyle).gameObject };
			col2.AddRange(neutralTraitsToggle);

			var col3 = new List<GameObject> { UIFactory.Label("Bad", WindowStyles.TitleStyle).gameObject };
			col3.AddRange(badTraitsToggle);

			Utils.CreateGameObjects(Constants.FIRST_COLUMN,  col1.ToArray(), window);
			Utils.CreateGameObjects(Constants.SECOND_COLUMN, col2.ToArray(), window);
			Utils.CreateGameObjects(Constants.THIRD_COLUMN,  col3.ToArray(), window);

			var maxRows = new[] { col1.Count, col2.Count, col3.Count }.Max();
			Utils.SetWindowSize(maxRows + 1, Constants.FOURTH_COLUMN - 1, window);

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
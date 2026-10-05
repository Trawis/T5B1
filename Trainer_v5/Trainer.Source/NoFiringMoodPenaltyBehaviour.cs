using System;
using System.Collections.Generic;
using System.Linq;

namespace Trainer_v5
{
	public class NoFiringMoodPenaltyBehaviour : ModBehaviour
	{
		private readonly HashSet<Employee> _knownEmployees = new HashSet<Employee>();
		private readonly HashSet<Employee> _currentEmployees = new HashSet<Employee>();
		private readonly Dictionary<Employee, HashSet<string>> _thoughtSnapshots = new Dictionary<Employee, HashSet<string>>();
		private bool _initialized;

		private static bool IsEnabled =>
			Helpers.GetProperty(Helpers.Settings, "Experimental") &&
			Helpers.GetProperty(Helpers.Settings, "NoFiringMoodPenalty");

		private void Update()
		{
			if (!isActiveAndEnabled || !Helpers.IsGameLoaded || GameSettings.Instance.MyCompany == null || !IsEnabled)
			{
				ResetTracking();
				return;
			}

			Helpers.TryExecute("NoFiringMoodPenalty", ApplyNoFiringMoodPenalty);
		}

		private void ApplyNoFiringMoodPenalty()
		{
			CollectCurrentEmployees();

			if (!_initialized)
			{
				CaptureCurrentState();
				return;
			}

			bool employeeLeft = _knownEmployees.Any(employee => !_currentEmployees.Contains(employee));
			if (employeeLeft)
			{
				RemoveNewNegativeThoughts();
			}

			CaptureCurrentState();
		}

		private void CollectCurrentEmployees()
		{
			_currentEmployees.Clear();

			for (int i = 0; i < GameSettings.Instance.sActorManager.Actors.Count; i++)
			{
				Actor actor = GameSettings.Instance.sActorManager.Actors[i];
				if (actor == null || actor.employee == null || actor.employee.MyEmployer != GameSettings.Instance.MyCompany)
				{
					continue;
				}

				_currentEmployees.Add(actor.employee);
			}
		}

		private void RemoveNewNegativeThoughts()
		{
			foreach (Employee employee in _currentEmployees)
			{
				HashSet<string> previousThoughts;
				if (!_thoughtSnapshots.TryGetValue(employee, out previousThoughts))
				{
					continue;
				}

				foreach (var thought in employee.Thoughts.Values.ToList())
				{
					if (previousThoughts.Contains(thought.Thought))
					{
						continue;
					}

					if (thought.Mood.Negative || thought.Mood.Sue || !string.IsNullOrEmpty(thought.Mood.QuitReason))
					{
						employee.Thoughts.Remove(thought.Thought);
					}
				}
			}
		}

		private void CaptureCurrentState()
		{
			var departedEmployees = _thoughtSnapshots.Keys
				.Where(employee => !_currentEmployees.Contains(employee))
				.ToList();

			for (int i = 0; i < departedEmployees.Count; i++)
			{
				_thoughtSnapshots.Remove(departedEmployees[i]);
			}

			foreach (Employee employee in _currentEmployees)
			{
				HashSet<string> snapshot;
				if (!_thoughtSnapshots.TryGetValue(employee, out snapshot))
				{
					snapshot = new HashSet<string>();
					_thoughtSnapshots[employee] = snapshot;
				}

				snapshot.Clear();
				foreach (string thought in employee.Thoughts.Keys)
				{
					snapshot.Add(thought);
				}
			}

			_knownEmployees.Clear();
			foreach (Employee employee in _currentEmployees)
			{
				_knownEmployees.Add(employee);
			}

			_initialized = true;
		}

		private void ResetTracking()
		{
			if (!_initialized && _knownEmployees.Count == 0 && _currentEmployees.Count == 0 && _thoughtSnapshots.Count == 0)
			{
				return;
			}

			_initialized = false;
			_knownEmployees.Clear();
			_currentEmployees.Clear();
			_thoughtSnapshots.Clear();
		}

		public override void OnActivate()
		{
			ResetTracking();
		}

		public override void OnDeactivate()
		{
			ResetTracking();
		}
	}
}

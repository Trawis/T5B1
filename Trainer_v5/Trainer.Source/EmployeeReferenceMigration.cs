using System;
using System.Collections.Generic;

namespace Trainer_v5
{
	// Creativity edits install a new Employee; every game holder of the old object is re-pointed here (public members only).
	// Holders are first collected into a plan without changing anything, then the plan is applied, so a failure while
	// looking things up cannot leave the game half migrated.
	internal static class EmployeeReferenceMigration
	{
		public static void Migrate(Employee oldEmployee, Employee newEmployee)
		{
			var plan = new List<Action>();
			PlanFriendships(plan, oldEmployee, newEmployee);

			var settings = GameSettings.Instance;
			if (settings != null)
			{
				PlanGame(plan, settings, oldEmployee, newEmployee);
			}

			var market = MarketSimulation.Active;
			if (market != null)
			{
				PlanSet(plan, market.FreeLeads, oldEmployee, newEmployee);
			}

			Apply(plan);
		}

		public static void Apply(List<Action> plan)
		{
			for (int i = 0; i < plan.Count; i++)
			{
				plan[i]();
			}
		}

		// Friendships are keyed by object pairs (equality is by reference), so the old pairs are removed and set again.
		public static void PlanFriendships(List<Action> plan, Employee oldEmployee, Employee newEmployee)
		{
			var all = Employee.GetAllFriendships();
			if (all == null)
			{
				return;
			}

			var others = new List<Employee>();
			var amounts = new List<float>();
			foreach (var pair in all)
			{
				if (pair.Key.E1 == oldEmployee)
				{
					others.Add(pair.Key.E2);
					amounts.Add(pair.Value);
				}
				else if (pair.Key.E2 == oldEmployee)
				{
					others.Add(pair.Key.E1);
					amounts.Add(pair.Value);
				}
			}

			for (int i = 0; i < others.Count; i++)
			{
				var other = others[i];
				var amount = amounts[i];
				plan.Add(() =>
				{
					Employee.DeleteFriendship(oldEmployee, other);
					Employee.SetFriendship(newEmployee, other, amount);
				});
			}
		}

		public static void PlanCompany(List<Action> plan, SimulatedCompany simulated, Employee oldEmployee, Employee newEmployee)
		{
			if (simulated.LeadDesigner == oldEmployee)
			{
				plan.Add(() => simulated.LeadDesigner = newEmployee);
			}

			var company = simulated as Company;
			if (company == null)
			{
				return;
			}

			PlanList(plan, company.NetworkEmployees, oldEmployee, newEmployee);
			if (company.WorkItems == null)
			{
				return;
			}

			foreach (var item in company.WorkItems)
			{
				var work = item as SoftwareWorkItem;
				if (work != null)
				{
					PlanWork(plan, work, oldEmployee, newEmployee);
				}
			}
		}

		public static void PlanWork(List<Action> plan, SoftwareWorkItem work, Employee oldEmployee, Employee newEmployee)
		{
			PlanKey(plan, work.LeadWork, oldEmployee, newEmployee);

			SoftwareWorkItem.FeatureProgress progress;
			if (work.NewWorking != null && work.NewWorking.TryGetValue(oldEmployee, out progress))
			{
				plan.Add(() =>
				{
					work.NewWorking.Remove(oldEmployee);
					work.NewWorking.Add(newEmployee, progress);
				});
			}

			var design = work as DesignDocument;
			if (design != null && design.LeadDesigner == oldEmployee)
			{
				plan.Add(() => design.LeadDesigner = newEmployee);
			}
		}

		public static void PlanProduct(List<Action> plan, SoftwareProduct product, Employee oldEmployee, Employee newEmployee)
		{
			if (product.LeadDesigner == oldEmployee)
			{
				plan.Add(() => product.LeadDesigner = newEmployee);
			}
		}

		public static void PlanKey<TValue>(List<Action> plan, Dictionary<Employee, TValue> dictionary, Employee oldKey, Employee newKey)
		{
			if (dictionary != null && dictionary.ContainsKey(oldKey))
			{
				plan.Add(() => ReplaceKeyOf(dictionary, oldKey, newKey));
			}
		}

		public static void PlanList<T>(List<Action> plan, List<T> list, T oldItem, T newItem)
		{
			if (list != null && list.Contains(oldItem))
			{
				plan.Add(() => ReplaceInList(list, oldItem, newItem));
			}
		}

		public static void PlanSet<T>(List<Action> plan, HashSet<T> set, T oldItem, T newItem)
		{
			if (set != null && set.Contains(oldItem))
			{
				plan.Add(() => ReplaceInSet(set, oldItem, newItem));
			}
		}

		public static bool ReplaceKeyOf<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey oldKey, TKey newKey)
		{
			TValue value;
			if (dictionary == null || !dictionary.TryGetValue(oldKey, out value))
			{
				return false;
			}

			dictionary.Remove(oldKey);
			dictionary[newKey] = value;
			return true;
		}

		public static bool ReplaceInList<T>(List<T> list, T oldItem, T newItem)
		{
			if (list == null)
			{
				return false;
			}

			var replaced = false;
			for (int i = 0; i < list.Count; i++)
			{
				if (EqualityComparer<T>.Default.Equals(list[i], oldItem))
				{
					list[i] = newItem;
					replaced = true;
				}
			}
			return replaced;
		}

		public static bool ReplaceInSet<T>(HashSet<T> set, T oldItem, T newItem)
		{
			if (set == null || !set.Remove(oldItem))
			{
				return false;
			}

			set.Add(newItem);
			return true;
		}

		// The simulation's companies and products, and every actor's interaction memory.
		private static void PlanGame(List<Action> plan, GameSettings settings, Employee oldEmployee, Employee newEmployee)
		{
			if (settings.simulation != null)
			{
				foreach (var simulated in settings.simulation.Companies.Values)
				{
					PlanCompany(plan, simulated, oldEmployee, newEmployee);
				}

				foreach (var product in settings.simulation.GetAllProducts(false))
				{
					PlanProduct(plan, product, oldEmployee, newEmployee);
				}
			}

			if (settings.sActorManager != null)
			{
				var actors = settings.sActorManager.Actors;
				for (int i = 0; i < actors.Count; i++)
				{
					PlanSet(plan, actors[i].HasInteractedWith, oldEmployee, newEmployee);
				}
			}
		}
	}
}

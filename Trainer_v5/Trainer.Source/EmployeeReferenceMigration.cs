using System.Collections.Generic;

namespace Trainer_v5
{
	// Creativity has no setter, so the editor installs a new Employee object (#183). The game keeps employees in
	// collections by object identity, so every holder of the old object has to be pointed at the new one. Each
	// holder below was found in the game assembly; everything goes through public members.
	internal static class EmployeeReferenceMigration
	{
		public static void Migrate(Employee oldEmployee, Employee newEmployee)
		{
			MigrateFriendships(oldEmployee, newEmployee);
			MigrateCompanies(oldEmployee, newEmployee);
			MigrateProducts(oldEmployee, newEmployee);
			MigrateActors(oldEmployee, newEmployee);

			var market = MarketSimulation.Active;
			if (market != null)
			{
				ReplaceInSet(market.FreeLeads, oldEmployee, newEmployee);
			}
		}

		// Friendships are keyed by object pairs (equality is by reference), so the old pairs are removed and set again.
		private static void MigrateFriendships(Employee oldEmployee, Employee newEmployee)
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
				Employee.DeleteFriendship(oldEmployee, others[i]);
				Employee.SetFriendship(newEmployee, others[i], amounts[i]);
			}
		}

		private static void MigrateCompanies(Employee oldEmployee, Employee newEmployee)
		{
			var simulation = GameSettings.Instance.simulation;
			foreach (var simulated in simulation.Companies.Values)
			{
				if (simulated.LeadDesigner == oldEmployee)
				{
					simulated.LeadDesigner = newEmployee;
				}

				var company = simulated as Company;
				if (company == null)
				{
					continue;
				}

				ReplaceInList(company.NetworkEmployees, oldEmployee, newEmployee);
				if (company.WorkItems == null)
				{
					continue;
				}

				foreach (var item in company.WorkItems)
				{
					var work = item as SoftwareWorkItem;
					if (work == null)
					{
						continue;
					}

					ReplaceKey(work.LeadWork, oldEmployee, newEmployee);
					MigrateFeatureWork(work, oldEmployee, newEmployee);

					var design = work as DesignDocument;
					if (design != null && design.LeadDesigner == oldEmployee)
					{
						design.LeadDesigner = newEmployee;
					}
				}
			}
		}

		private static void MigrateFeatureWork(SoftwareWorkItem work, Employee oldEmployee, Employee newEmployee)
		{
			SoftwareWorkItem.FeatureProgress progress;
			if (work.NewWorking != null && work.NewWorking.TryGetValue(oldEmployee, out progress))
			{
				work.NewWorking.Remove(oldEmployee);
				work.NewWorking.Add(newEmployee, progress);
			}
		}

		private static void MigrateProducts(Employee oldEmployee, Employee newEmployee)
		{
			foreach (var product in GameSettings.Instance.simulation.GetAllProducts(false))
			{
				if (product.LeadDesigner == oldEmployee)
				{
					product.LeadDesigner = newEmployee;
				}
			}
		}

		// The interaction memory is a per-actor set of employees. (Complaint objects are notifications, not state kept
		// on the employee: Employee.ActiveComplaint is only a flag.)
		private static void MigrateActors(Employee oldEmployee, Employee newEmployee)
		{
			var actors = GameSettings.Instance.sActorManager.Actors;
			for (int i = 0; i < actors.Count; i++)
			{
				ReplaceInSet(actors[i].HasInteractedWith, oldEmployee, newEmployee);
			}
		}

		public static bool ReplaceKey<TValue>(Dictionary<Employee, TValue> dictionary, Employee oldKey, Employee newKey)
		{
			return ReplaceKeyOf(dictionary, oldKey, newKey);
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
	}
}

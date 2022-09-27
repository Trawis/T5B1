using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils = Trainer_v5.Utilities;


namespace Trainer_v5
{
	public class EmployeeSkillChangeWindow : MonoBehaviour
	{
		private static readonly string _title = "Employee Skill Change, by Trawis";

		public static GUIWindow Window { get; set; }

		public static void Show()
		{
			if (Window == null)
			{
				Window = CreateWindow();
			}
			else
			{
				Window.Toggle();
			}
		}


		private static GUIWindow CreateWindow()
		{
			var window = WindowManager.SpawnWindow();
			window.InitialTitle = window.TitleText.text = window.NonLocTitle = _title;
			window.name = "EditEmployee";
			window.MainPanel.name = "EditEmployeePanel";

			var firstColumn = FirstColumn().ToArray();
			var secondColumn = SecondColumn().ToArray();

			Utils.AddLabel("Roles", new Rect(10, 5, 150, 32), window);
			Utils.AddLabel("Specializations", new Rect(161, 5, 150, 32), window);
			Utils.CreateGameObjects(Constants.FIRST_COLUMN, firstColumn, window);
			Utils.CreateGameObjects(Constants.SECOND_COLUMN, secondColumn, window);

			var maxRows = Math.Max(firstColumn.Length, secondColumn.Length);
			Utils.SetWindowSize(maxRows, Constants.X_EMPLOYEESKILLCHANGE_WINDOW, window);

			return window;
		}


		private static IEnumerable<GameObject> FirstColumn()
		{
			yield return UIFactory.EmptyBox();

			var rolesList = Helpers.RolesList;
			foreach (var role in rolesList)
			{
				var isOn = rolesList.GetOrDefault(role.Key);
				yield return UIFactory.Toggle(role.Key, isOn, a => rolesList.Toggle(role.Key));
			}

			yield return UIFactory.Button("Set Skills", TrainerBehaviour.SetSkillPerEmployee);
		}


		private static IEnumerable<GameObject> SecondColumn()
		{
			yield return UIFactory.EmptyBox();

			var specs = Helpers.SpecializationsList;
			foreach (var spec in specs)
			{
				var key = spec.Key;
				var isOn = specs.GetOrDefault(key);
				yield return UIFactory.Toggle(key, isOn, a => specs.Toggle(key));
			}
		}
	}

}
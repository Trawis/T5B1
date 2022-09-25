using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
				CreateWindow();
			}
			else
			{
				Window.Toggle();
			}
		}

		private static void CreateWindow()
		{
			Window = WindowManager.SpawnWindow();
			Window.InitialTitle = Window.TitleText.text = Window.NonLocTitle = _title;
			Window.name = "EmployeeSkillChange";
			Window.MainPanel.name = "EmployeeSkillChangePanel";

			List<GameObject> roleToggles = new List<GameObject>();
			List<GameObject> specializationToggles = new List<GameObject>();
			Utils.AddEmptyBox(roleToggles);
			Utils.AddEmptyBox(specializationToggles);

			Utils.AddLabel("Roles", new Rect(10, 5, 150, 32), Window);
			Utils.AddLabel("Specializations", new Rect(161, 5, 150, 32), Window);

			var rolesList = Helpers.RolesList;
			foreach (var role in rolesList)
			{
				Utils.AddToggle(role.Key, Helpers.GetProperty(rolesList, role.Key),
														 a => Helpers.SetProperty(rolesList, role.Key,
																 !Helpers.GetProperty(rolesList, role.Key)),
														 roleToggles);
			}

			Utils.AddButton("Set Skills", TrainerBehaviour.SetSkillPerEmployee, roleToggles);

			var specializationsList = Helpers.SpecializationsList;
			foreach (var specialization in specializationsList)
			{
				Utils.AddToggle(specialization.Key,
												Helpers.GetProperty(specializationsList, specialization.Key),
												a => Helpers.SetProperty(specializationsList, specialization.Key,
														!Helpers.GetProperty(specializationsList, specialization.Key)),
												specializationToggles);
			}

			Utils.CreateGameObjects(Constants.FIRST_COLUMN, roleToggles.ToArray(), Window);
			Utils.CreateGameObjects(Constants.SECOND_COLUMN, specializationToggles.ToArray(), Window);

			int[] columnsCount = new int[]
			{
				roleToggles.Count(),
				specializationToggles.Count()
			};

			Utils.SetWindowSize(columnsCount.Max(), Constants.X_EMPLOYEESKILLCHANGE_WINDOW, Window);
		}
	}
}
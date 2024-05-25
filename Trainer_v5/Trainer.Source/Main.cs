﻿using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class Main : ModMeta
	{
		public override string Name => "TrainerByTrawis";
		public static Button TrainerButton { get; set; }
		public static Button SkillChangeButton { get; set; }

		public override void Initialize(ModController.DLLMod parentMod)
		{
			base.Initialize(parentMod);
		}

		public static void OpenSettingsWindow()
		{
			if (!SettingsWindow.Shown)
			{
				SettingsWindow.Toggle();
			}
		}

		public static void CloseSettingsWindow()
		{
			if (SettingsWindow.Shown)
			{
				SettingsWindow.Toggle();
			}
		}

		public static void CreateUIButtons()
		{
			TrainerButton = Utilities.CreateUIButton(() => SettingsWindow.Toggle(), Helpers.TrainerVersion, "TrainerButton");
			SkillChangeButton = Utilities.CreateUIButton(() => EmployeeSkillChangeWindow.Show(), "Skill Change", "EmployeeSkillButton");

			Utilities.AddElementToElement(TrainerButton.gameObject, "MainPanel/Holder/FanPanel", new Rect(164, 0, 100, 32));
			Utilities.AddElementToElement(SkillChangeButton.gameObject, "ActorWindow/ContentPanel/Panel", new Rect(0, 0, 100, 32));
		}

		public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
		{
			Text label = WindowManager.SpawnLabel();
			label.text = "Please load the game and press 'Trainer' button.";

			WindowManager.AddElementToElement(label.gameObject, parent.gameObject, new Rect(0, 0, 400, 128),
					new Rect(0, 0, 0, 0));
		}

		public override WriteDictionary Serialize(GameReader.LoadMode mode)
		{
			var data = new WriteDictionary();
			foreach (var setting in Helpers.Settings)
			{
				data[setting.Key] = Helpers.GetProperty(Helpers.Settings, setting.Key);
			}

			foreach (var store in Helpers.StoresSettings)
			{
				data[store.Key] = Helpers.GetProperty(Helpers.StoresSettings, store.Key);
			}

			return data;
		}

		public override void Deserialize(WriteDictionary data, GameReader.LoadMode mode)
		{
			var settings = Helpers.Settings.Keys.ToList();
			foreach (var setting in settings)
			{
				Helpers.SetProperty(Helpers.Settings, setting, data.Get(setting, Helpers.GetProperty(Helpers.Settings, setting)));
			}

			var stores = Helpers.StoresSettings.Keys.ToList();
			foreach (var store in stores)
			{
				Helpers.SetProperty(Helpers.StoresSettings, store, data.Get(store, Helpers.GetProperty(Helpers.StoresSettings, store)));
			}
		}
	}
}
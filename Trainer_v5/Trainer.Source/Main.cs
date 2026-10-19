using System.Linq;
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
			TrainerButton = UIFactory.Button(Helpers.TrainerVersion, () => SettingsWindow.Toggle(), "TrainerButton");
			SkillChangeButton = UIFactory.Button("Skill Change", () => EmployeeSkillChangeWindow.Show(), "EmployeeSkillButton");

			TrainerButton.gameObject.AddToElement("MainPanel/Holder/FanPanel", new Rect(164, 0, 100, 32));
			SkillChangeButton.gameObject.AddToElement("ActorWindow/ContentPanel/Panel", new Rect(0, 0, 100, 32));
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
			// A full load replaces the whole game, so a key the save lacks takes its declared default; building and
			// company loads are partial and keep the values already in use.
			var isFullLoad = mode == GameReader.LoadMode.Full;
			var toggleDefaults = SettingKeys.CreateToggleDefaults();
			var settings = Helpers.Settings.Keys.ToList();
			foreach (var setting in settings)
			{
				var fallback = isFullLoad ? toggleDefaults[setting] : Helpers.GetProperty(Helpers.Settings, setting);
				Helpers.SetProperty(Helpers.Settings, setting, data.Get(setting, fallback));
			}

			var storeDefaults = SettingKeys.CreateStoreDefaults();
			var stores = Helpers.StoresSettings.Keys.ToList();
			foreach (var store in stores)
			{
				var fallback = isFullLoad ? storeDefaults[store] : Helpers.GetProperty(Helpers.StoresSettings, store);
				Helpers.SetProperty(Helpers.StoresSettings, store, data.Get(store, fallback));
			}
		}
	}
}

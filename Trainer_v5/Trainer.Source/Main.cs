using System.Linq;
using Trainer_v5.Experimental;
using Trainer_v5.Window;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class Main : ModMeta
	{
		private ModController.DLLMod _mod;

		public override string Name => "TrainerByTrawis";
		public static Button TrainerButton { get; set; }
		public static Button SkillChangeButton { get; set; }

		public override void Initialize(ModController.DLLMod parentMod)
		{
			base.Initialize(parentMod);
			_mod = parentMod;

			// The game has already read the mod's own settings file; this runs when the mod loads, before any game.
			ExperimentalOptions.Load(parentMod.Settings);
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
			// Both buttons are tracked in the statics as soon as they exist, so a retry after a failure (or a second
			// scene load) removes what is left before creating a new pair instead of stacking a second one.
			DestroyUIButtons();

			TrainerButton = UIFactory.Button(Helpers.TrainerVersion, () => SettingsWindow.Toggle(), "TrainerButton");
			SkillChangeButton = UIFactory.Button("SkillChangeButton".LocDef("Skill Change"), () => EmployeeSkillChangeWindow.Show(), "EmployeeSkillButton");

			// A button whose parent element is missing is dropped rather than left loose in the scene.
			if (!TrainerButton.gameObject.AddToElement("MainPanel/Holder/FanPanel", new Rect(164, 0, 100, 32)))
			{
				DestroyButton(TrainerButton);
				TrainerButton = null;
			}

			if (!SkillChangeButton.gameObject.AddToElement("ActorWindow/ContentPanel/Panel", new Rect(0, 0, 100, 32)))
			{
				DestroyButton(SkillChangeButton);
				SkillChangeButton = null;
			}
		}

		public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
		{
			// The game builds this once per Options window and keeps it, so the toggle is the only writer of the value.
			float y = 0;
			if (_mod != null)
			{
				Toggle toggle = UIFactory.Toggle("EnableExperimentalFeatures".LocDef("Enable Experimental Features"), ExperimentalOptions.Enabled, SetExperimentalEnabled);
				WindowManager.AddElementToElement(toggle.gameObject, parent.gameObject, new Rect(0, y, 400, Constants.ELEMENT_HEIGHT),
					new Rect(0, 0, 0, 0));
				y += Constants.ELEMENT_HEIGHT;
			}

			Text note = WindowManager.SpawnLabel();
			note.text = "ExperimentalNone".LocDef("No experimental features are available yet, so turning this on changes nothing today. The choice applies to every save game.");
			WindowManager.AddElementToElement(note.gameObject, parent.gameObject, new Rect(0, y, 400, 64),
				new Rect(0, 0, 0, 0));
			y += 64;

			Text label = WindowManager.SpawnLabel();
			label.text = "OptionsLoadHint".LocDef("Please load the game and press 'Trainer' button.");
			WindowManager.AddElementToElement(label.gameObject, parent.gameObject, new Rect(0, y, 400, 64),
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

		private void SetExperimentalEnabled(bool enabled)
		{
			ExperimentalOptions.Set(enabled, _mod.SaveSetting);
		}

		private static void DestroyUIButtons()
		{
			DestroyButton(TrainerButton);
			TrainerButton = null;
			DestroyButton(SkillChangeButton);
			SkillChangeButton = null;
		}

		private static void DestroyButton(Button button)
		{
			if (button != null)
				Object.Destroy(button.gameObject);
		}
	}
}

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

		internal const string ModName = "TrainerByTrawis";

		public override string Name => ModName;
		public static Button TrainerButton { get; set; }

		public override void Initialize(ModController.DLLMod parentMod)
		{
			base.Initialize(parentMod);
			_mod = parentMod;

			ExperimentalOptions.Load(parentMod.Settings);
			LoggingOptions.Load(parentMod.Settings);
			GameVersionCheck.LogRunningVersion();
		}

		public static void OpenSettingsWindow()
		{
			SettingsWindow.Open();
		}

		public static void CloseSettingsWindow()
		{
			SettingsWindow.Close();
		}

		public static void CreateUIButtons()
		{
			DestroyUIButtons();

			TrainerButton = UIFactory.Button(Helpers.TrainerVersion, () => SettingsWindow.Toggle(), "TrainerButton");

			if (!TrainerButton.gameObject.AddToElement("MainPanel/Holder/FanPanel", new Rect(164, 0, 100, 32)))
			{
				DestroyButton(TrainerButton);
				TrainerButton = null;
			}
		}

		public override void ConstructOptionsScreen(RectTransform parent, bool inGame)
		{
			float y = 0;
			if (_mod != null)
			{
				Toggle toggle = UIFactory.Toggle("EnableExperimentalFeatures".LocDef("Enable Experimental Features"), ExperimentalOptions.Enabled, SetExperimentalEnabled);
				WindowManager.AddElementToElement(toggle.gameObject, parent.gameObject, new Rect(0, y, 400, Constants.ELEMENT_HEIGHT),
					new Rect(0, 0, 0, 0));
				y += Constants.ELEMENT_HEIGHT;
			}

			Text note = WindowManager.SpawnLabel();
			note.text = "ExperimentalNone".LocDef("Shows additional, unfinished options in Trainer Settings. Turning it off switches them off. The choice applies to every save game.");
			WindowManager.AddElementToElement(note.gameObject, parent.gameObject, new Rect(0, y, 400, 64),
				new Rect(0, 0, 0, 0));
			y += 64;

			if (_mod != null)
			{
				Toggle debugToggle = UIFactory.Toggle("DebugLogging".LocDef("Debug Logging"), LoggingOptions.DebugEnabled, SetDebugLoggingEnabled);
				WindowManager.AddElementToElement(debugToggle.gameObject, parent.gameObject, new Rect(0, y, 400, Constants.ELEMENT_HEIGHT),
					new Rect(0, 0, 0, 0));
				y += Constants.ELEMENT_HEIGHT;
			}

			Text debugNote = WindowManager.SpawnLabel();
			debugNote.text = "DebugLoggingHint".LocDef("Adds trace lines to the dev console. Errors are always logged. The choice applies to every save game.");
			WindowManager.AddElementToElement(debugNote.gameObject, parent.gameObject, new Rect(0, y, 400, 64),
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
			var isFullLoad = mode == GameReader.LoadMode.Full;
			ApplySaved(data, isFullLoad);
			if (isFullLoad)
			{
				SettingsSession.FullLoadApplied();
			}
		}

		internal static void ApplyForGame(WriteDictionary modData)
		{
			WriteDictionary saved = modData == null ? null : modData.Get<WriteDictionary>(ModName, null);
			ApplySaved(saved ?? new WriteDictionary(), true);
		}

		internal static void ApplySaved(WriteDictionary data, bool isFullLoad)
		{
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
			if (!enabled)
			{
				ExperimentalToggles.TurnOffAll(Helpers.Settings);
			}

			Helpers.TryExecute("RebuildSettingsWindow", SettingsWindow.Rebuild);
		}

		private void SetDebugLoggingEnabled(bool enabled)
		{
			LoggingOptions.Set(enabled, _mod.SaveSetting);
		}

		private static void DestroyUIButtons()
		{
			DestroyButton(TrainerButton);
			TrainerButton = null;
		}

		private static void DestroyButton(Button button)
		{
			if (button != null)
				Object.Destroy(button.gameObject);
		}
	}
}

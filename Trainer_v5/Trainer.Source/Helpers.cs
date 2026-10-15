using System;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public static class Helpers
	{
		// Keys already reported as unregistered, so a typo'd key hit every frame logs once. Shared with Extensions.
		private static readonly HashSet<string> _loggedUnregisteredKeys;

		private static readonly FeatureGuard _featureGuard;

		public static bool IsGameLoaded => GameSettings.Instance != null && HUD.Instance != null;
		public static string Version => "5.3.5";
		public static string TrainerVersion => $"Trainer v{Version}";
		public static bool IsDebug => false;
		public static string DiscordUrl => "https://discord.com/invite/J584aG";

		public static Random Random { get; set; }
		public static bool RewardIsGained { get; set; }
		public static bool DealIsPushed { get; set; }
		public static string ProductPriceName { get; set; }
		public static Dictionary<string, bool> SpecializationsList { get; set; }

		public static Dictionary<string, object> EfficiencySelectItems => new Dictionary<string, object>
		{
			{"Default", null},
			{"100%", 1},
			{"200%", 2},
			{"500%", 5},
			{"1000%", 10},
			{"2000%", 20},
			{"4000%", 40},
			{"8000%", 80}
		};

		public static Dictionary<string, bool> Settings { get; private set; }

		public static Dictionary<string, bool> RolesList { get; } = new Dictionary<string, bool>
		{
			{"Lead", false},
			{"Service", false},
			{"Programmer", false},
			{"Artist", false},
			{"Designer", false}
		};

		public static Dictionary<string, object> StoresSettings { get; private set; }

		// The game's mod loader mis-parses calls in member initializers, so they are assigned here.
		static Helpers()
		{
			Settings = SettingKeys.CreateToggleDefaults();
			StoresSettings = SettingKeys.CreateStoreDefaults();
			_loggedUnregisteredKeys = new HashSet<string>();
			_featureGuard = new FeatureGuard(
				() => UnityEngine.Time.realtimeSinceStartup,
				(featureId, ex) =>
				{
					ex.LogException(featureId);
					UnityEngine.Debug.LogException(ex);
				});
		}

		#region methods

		public static bool GetProperty(Dictionary<string, bool> properties, string key)
		{
			bool value;
			if (properties.TryGetValue(key, out value))
			{
				return value;
			}
			LogUnregisteredKey("GetProperty", key);
			return false;
		}

		public static object GetProperty(Dictionary<string, object> properties, string key)
		{
			object value;
			if (properties.TryGetValue(key, out value))
			{
				return value;
			}
			LogUnregisteredKey("GetProperty", key);
			return null;
		}

		public static bool GetProperty(Dictionary<string, bool> properties, ToggleKey key) => GetProperty(properties, key.Key);

		public static object GetProperty(Dictionary<string, object> properties, StoreKey key) => GetProperty(properties, key.Key);

		public static void SetProperty(Dictionary<string, bool> properties, string key, bool value)
		{
			if (!properties.ContainsKey(key))
			{
				LogUnregisteredKey("SetProperty", key);
				return;
			}
			properties[key] = value;
		}

		public static void SetProperty(Dictionary<string, object> properties, string key, object value)
		{
			if (!properties.ContainsKey(key))
			{
				LogUnregisteredKey("SetProperty", key);
				return;
			}
			properties[key] = value;
		}

		// Isolates one feature's exceptions (#123); returns whether the action completed without throwing.
		public static bool TryExecute(string featureId, Action action) => _featureGuard.TryExecute(featureId, action);

		public static void ShowDiscordInvite(bool displayAsPopup = false)
		{
			string message = $"Join us on our discord server\n{DiscordUrl}";
			if (displayAsPopup)
			{
				HUD.Instance.AddPopupMessage(message, "Cogs", PopupManager.PopUpAction.None, 0, 0, 0, 0);
			}
			else
			{
				WindowManager.SpawnDialog(message, false, DialogWindow.DialogType.Information);
			}
		}

		// A key missing here was never registered (a typo); a registered key absent from an old save never reaches these accessors.
		internal static void LogUnregisteredKey(string source, string key)
		{
			string message = $"{source}: setting key '{key}' is not registered - check for a typo in the key name. Falling back to the default value.";
			if (_loggedUnregisteredKeys.Add(message))
			{
				message.Log();
			}
		}

		internal static bool IsGameReady(bool requireSelector = false) =>
			IsGameLoaded && (!requireSelector || SelectorController.Instance != null);

		#endregion

		#region extensions

		public static Employee.EmployeeRole ToEmployeeRole(this string str)
		{
			Employee.EmployeeRole role;
			if (Enum.TryParse(str, out role))
				return role;
			$"ToEmployeeRole: unknown role '{str}', defaulting to Programmer".Log();
			return Employee.EmployeeRole.Programmer;
		}

		#endregion

		// No reliable build number is committed, so the vendored Beta 1 target is reported generically.
		public static string GetGameVersion() => "Beta 1";
	}
}

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
		public static string Version => "5.6.1";
		public static string TrainerVersion => $"Trainer v{Version}";
		public static string DiscordUrl => "https://discord.com/invite/J584aG";

		public static Dictionary<string, bool> Settings { get; private set; }

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

		public static bool TryExecute(string featureId, Action action) => _featureGuard.TryExecute(featureId, action);

		public static void ShowDiscordInvite(bool displayAsPopup = false)
		{
			string message = string.Format("DiscordInvite".LocDef("Join us on our discord server\n{0}"), DiscordUrl);
			if (displayAsPopup)
			{
				Notification.Popup(message);
			}
			else
			{
				Notification.ShowInfo(message);
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

		// Scheduled work waits for this. GameReady is raised just before DoneLoading is set (and after pre-simulation for a
		// new game), so GameReady initialization must not be gated on it.
		internal static bool IsSimulationReady()
		{
			GameSettings game = GameSettings.Instance;
			SelectorController selector = SelectorController.Instance;

			return SimulationReady(
				game != null,
				HUD.Instance != null,
				selector != null,
				selector != null && selector.DoneLoading,
				game != null && game.PreSimActive,
				game != null && game.PreSimFinished,
				TimeOfDay.Instance != null);
		}

		internal static bool SimulationReady(bool hasGame, bool hasHud, bool hasSelector, bool doneLoading, bool preSimActive, bool preSimFinished, bool hasClock)
		{
			return hasGame && hasHud && hasSelector && hasClock && doneLoading && !preSimActive && !preSimFinished;
		}

		internal static bool RequireGameReady(string action, bool requireSelector = false)
		{
			if (IsGameReady(requireSelector))
			{
				return true;
			}

			string message = string.Format("GameNotReady".LocDef("{0}: the game is not ready yet, nothing was changed."), action);
			message.Log();
			if (HUD.Instance != null)
			{
				Notification.ShowError(message);
			}

			return false;
		}

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

		public static string GetGameVersion() => GameVersionCheck.Running();
	}
}

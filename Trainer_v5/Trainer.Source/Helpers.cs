using System;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public static class Helpers
	{
		public static bool IsGameLoaded => GameSettings.Instance != null && HUD.Instance != null;
		public static string Version => "5.2.7";
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

		public static Dictionary<string, bool> Settings { get; } = new Dictionary<string, bool>
		{
			{"NoStress", false},
			{"NoVacation", false},
			{"FullRoomBrightness", false},
			{"CleanRooms", false},
			{"FullEnvironment", false},
			{"NoiseReduction", false},
			{"FreeStaff", false},
			{"TemperatureLock", false},
			{"NoWaterElectricity", false},
			{"NoNeeds", false},
			{"FreeEmployees", false},
			{"LockAge", false},
			{"MoreHostingDeals", false},
			{"IncreaseCourierCapacity", false},
			{"ReduceISPCost", false},
			{"IncreasePrintSpeed", false},
			{"FreePrint", false},
			{"IncreaseBookshelfSkill", false},
			{"NoMaintenance", false},
			{"NoSickness", false},
			{"FullSatisfaction", false},
			{"DisableSkillDecay", false},
			{"DisableBurglars", false},
			{"DisableFires", false},
			{"NoServerCost", false},
			{"ReduceExpansionCost", false},
			{"NoEducationCost", false},
			{"IncreaseWalkSpeed", false},
			{"AutoEndDesign", false},
			{"AutoEndResearch", false},
			{"AutoEndPatent", false},
			{"ReduceBoxPrice", false},
			{"DisableFurnitureStealing", false},
			{"MoreInspiration", false},
			{"MoreCreativity", false},
			{"AutoResearchStart", false},
			{"DigitalDistributionMonopol", false},
			{"DisableFireInspection", false},
			{"DisableForcePause", false},
			{"DisableForceFreeze", false},
			{"AutoAcceptHostingDeals", false},
			{"Experimental", false},
		};

		public static Dictionary<string, bool> RolesList { get; } = new Dictionary<string, bool>
		{
			{"Lead", false},
			{"Service", false},
			{"Programmer", false},
			{"Artist", false},
			{"Designer", false}
		};

		public static Dictionary<string, object> StoresSettings { get; } = new Dictionary<string, object>
		{
			{"EfficiencyStore", null},
			{"LeadEfficiencyStore", null}
		};

		#region methods

		// Keys reported as unregistered already, so a typo'd key hit every frame
		// doesn't flood the console. Shared with Extensions so all the property
		// accessors dedupe against the same set.
		private static readonly HashSet<string> _loggedUnregisteredKeys = new HashSet<string>();

		// A missing key here means the caller asked for a key that was never added
		// to the dictionary it's operating on, which happens when code references a
		// setting name that doesn't exist (e.g. a typo). It is distinct from a
		// registered key that is simply absent from an old save's serialized data -
		// that case never reaches these accessors, since save deserialization only
		// looks up keys it already enumerated from the live settings dictionary.
		internal static void LogUnregisteredKey(string source, string key)
		{
			string message = $"{source}: setting key '{key}' is not registered - check for a typo in the key name. Falling back to the default value.";
			if (_loggedUnregisteredKeys.Add(message))
			{
				message.Log();
			}
		}

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

		public static int GetIndex(List<KeyValuePair<string, object>> values, Dictionary<string, object> properties, string store, int valueType)
		{
			switch (valueType)
			{
				case 1:
					return values.FindIndex(x => x.Value.MakeInt() == GetProperty(properties, store).MakeInt());
				case 2:
					return values.FindIndex(x => x.Value.MakeFloat() == GetProperty(properties, store).MakeFloat());
				case 3:
					return values.FindIndex(x => x.Value.MakeString() == GetProperty(properties, store).MakeString());
				case 4:
					return values.FindIndex(x => x.Value.MakeBool() == GetProperty(properties, store).MakeBool());
				default:
					"Method GetIndex received an unknown value type as parameter".Log();
					return -1;
			}
		}

		public static void TryExecute(Action action)
		{
			try
			{
				action.Invoke();
			}
			catch (Exception ex)
			{
				ex.LogException();
				UnityEngine.Debug.LogException(ex);
			}
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

		// T5B1 targets whichever Software Inc Beta 1 build the assemblies under
		// Trainer.Libraries/ were vendored from; there is no reliable way to
		// read an exact build number from committed information, so this
		// reports the current target generically instead of guessing one.
		public static string GetGameVersion() => "Beta 1";
	}
}
﻿using System;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public static class Helpers
	{
		public static bool IsGameLoaded => GameSettings.Instance != null && HUD.Instance != null;
		public static string Version => "5.2.1";
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

		public static bool GetProperty(Dictionary<string, bool> properties, string key)
		{
			bool value;
			if (properties.TryGetValue(key, out value))
			{
				return value;
			}
			return false;
		}

		public static object GetProperty(Dictionary<string, object> properties, string key)
		{
			object value;
			if (properties.TryGetValue(key, out value))
			{
				return value;
			}
			return null;
		}

		public static void SetProperty(Dictionary<string, bool> properties, string key, bool value)
		{
			properties[key] = value;
		}

		public static void SetProperty(Dictionary<string, object> properties, string key, object value)
		{
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
			}
		}

		#endregion

		#region extensions

		public static Employee.EmployeeRole ToEmployeeRole(this string str)
		{
			return (Employee.EmployeeRole)Enum.Parse(typeof(Employee.EmployeeRole), str);
		}

		#endregion

		public static string GetGameVersion()
		{
#if !SWINCBETA && !SWINCRELEASE
			return "1.6";
#elif SWINCBETA1_7
			return "1.7";
#elif SWINCBETA1_8
			return "1.8";
#elif SWINCBETA1_9
			return "1.9";
#elif SWINCBETA1_10
			return "1.10";
#else
			return "UNKNOWN";
#endif
		}
	}
}
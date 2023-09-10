using System;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public static class Helpers
	{
		public static bool IsGameLoaded => GameSettings.Instance != null && HUD.Instance != null;
		public static string Version => "5.1.0";
		public static string TrainerVersion => $"Trainer v{Version}";
		public static bool IsDebug => false;
		public static string DiscordUrl => "https://discord.com/invite/J584aG";

		public static Random Random { get; set; }
		public static bool RewardIsGained { get; set; }
		public static bool DealIsPushed { get; set; }
		public static string ProductPriceName { get; set; }
		public static Dictionary<string, bool> SpecializationsList { get; set; }

		public static List<KeyValuePair<string, object>> EfficiencyValues
		{
			get
			{
				return new List<KeyValuePair<string, object>>
				{
				  new KeyValuePair<string, object>("Default", null),
				  new KeyValuePair<string, object>("100%", 1),
				  new KeyValuePair<string, object>("200%", 2),
				  new KeyValuePair<string, object>("500%", 5),
				  new KeyValuePair<string, object>("1000%", 10),
				  new KeyValuePair<string, object>("2000%", 20),
				  new KeyValuePair<string, object>("4000%", 40)
				};
			}
		}

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
			{"AutoDistributionDeals", false},
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
			{"MoreCreativity", false}
		};

		public static Dictionary<string, bool> RolesList { get; } = new Dictionary<string, bool>
		{
			{"Lead", false},
			{"Service", false},
			{"Programmer", false},
			{"Artist", false},
			{"Designer", false}
		};

		public static Dictionary<string, object> Stores { get; } = new Dictionary<string, object>
		{
			{"EfficiencyStore", 2},
			{"LeadEfficiencyStore", 4}
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
	}
}
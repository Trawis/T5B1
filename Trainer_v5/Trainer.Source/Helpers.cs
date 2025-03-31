﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using OrbCreationExtensions;

namespace Trainer_v5
{
	public enum ValueDataTypeEnum { Int = 1, Float = 2, String = 3, Bool = 4 }

	public static class Helpers
	{
		public static bool IsGameLoaded => GameSettings.Instance != null && HUD.Instance != null;
		public static string Version => "5.2.2";
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

		// Updated GetIndex to use ValueDataType enum
		public static int GetIndex(List<KeyValuePair<string, object>> values, Dictionary<string, object> properties, string store, ValueDataTypeEnum valueType)
		{
			try // Added try-catch for safety when accessing properties
			{
				object propertyValue = GetProperty(properties, store);
				if (propertyValue == null)
				{
					$"Property '{store}' not found or is null in GetIndex.".Log();
					return -1; // Or a default index like 0 if appropriate
				}

				switch (valueType)
				{
					case ValueDataTypeEnum.Int:
						int intValue = propertyValue.MakeInt();
						return values.FindIndex(x => x.Value != null && x.Value.MakeInt() == intValue);
					case ValueDataTypeEnum.Float:
						float floatValue = propertyValue.MakeFloat();
						return values.FindIndex(x => x.Value != null && x.Value.MakeFloat() == floatValue);
					case ValueDataTypeEnum.String:
						string stringValue = propertyValue.MakeString();
						// Handle potential nulls in the list values as well
						return values.FindIndex(x => x.Value != null && x.Value.MakeString() == stringValue);
					case ValueDataTypeEnum.Bool:
						bool boolValue = propertyValue.MakeBool();
						return values.FindIndex(x => x.Value != null && x.Value.MakeBool() == boolValue);
					default:
						$"Method GetIndex received an unknown value type: {valueType}".Log();
						return -1;
				}
			}
			catch (Exception ex)
			{
				// Log the error message first, then log the exception details
				$"Error in GetIndex for store '{store}' and type '{valueType}'".Log(false); // Log context without property name
				ex.LogException(); // Correctly call the extension method on the exception object
				return -1; // Return -1 or default index on error
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

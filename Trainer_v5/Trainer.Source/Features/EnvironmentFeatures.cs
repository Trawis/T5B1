using System.Collections.Generic;

namespace Trainer_v5
{
	internal static class EnvironmentFeatures
	{
		// RepairMe scales its repair step by this factor and calls the private FixNow when done: a large factor repairs in one pass.
		private const float RepairFactor = 1000000f;

		private static GameSettings Settings => GameSettings.Instance;

		// The game reads these flags directly, so they are bound when the setting changes rather than polled.
		internal static void ApplyForceLights(bool enabled)
		{
			Cheats.ForceLights = enabled;
		}

		internal static void ApplyShowRoomCeilings(bool enabled)
		{
			Cheats.CeilingMeshes = enabled;

			// Mirrors CameraScript.RefreshFlyMode: marking rooms dirty regenerates the ceiling meshes.
			if (Helpers.IsGameLoaded)
			{
				foreach (Room room in Settings.sRoomManager.Rooms)
				{
					room.DirtyOuterMesh = true;
					room.DirtyInnerMesh = true;
				}
			}
		}

		// HashList.GetEnumerator boxes a list enumerator, so the per-frame furniture passes index the list instead of using foreach.
		internal static void ApplyNoiseReduction()
		{
			HashList<Furniture> allFurniture = Settings.sRoomManager.AllFurniture;
			for (int i = 0; i < allFurniture.Count; i++)
			{
				Furniture furniture = allFurniture[i];
				furniture.ActorNoise = 0f;
				furniture.EnvironmentNoise = 0f;
				furniture.FinalNoise = 0f;
				furniture.Noisiness = 0;
			}

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				Settings.sActorManager.Actors[i].Noisiness = 0;
			}
		}

		internal static void ApplyDisableActiveFires()
		{
			HashList<Furniture> allFurniture = Settings.sRoomManager.AllFurniture;
			for (int i = 0; i < allFurniture.Count; i++)
			{
				Furniture furniture = allFurniture[i];
				if (!furniture.Parent.IsOnFire)
				{
					continue;
				}

				if (furniture.Parent.Temperature > 40f)
				{
					furniture.Parent.Temperature = 21f;
				}
				furniture.Parent.StopFire();
			}
		}

		internal static void ApplyCleanRooms()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				ApplyCleanRoomsToRoom(Settings.sRoomManager.Rooms[i]);
			}
		}

		internal static void ApplyNoSickness()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				ApplyNoSicknessToRoom(Settings.sRoomManager.Rooms[i]);
			}

			// The sick list is global to the day, not per actor: clear it once per pass.
			TimeOfDay.Instance.Sick.Clear();

			for (int i = 0; i < Settings.sActorManager.Actors.Count; i++)
			{
				ApplyNoSicknessToActor(Settings.sActorManager.Actors[i]);
			}
		}

		internal static void ApplyTemperatureLock()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Settings.sRoomManager.Rooms[i].Temperature = 21f;
			}
		}

		internal static void ApplyNoMaintenance()
		{
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				if (furniture.Type == "Chair" && furniture.Comfort < Constants.CHAIR_COMFORT_LOW)
				{
					furniture.Comfort = Constants.CHAIR_COMFORT_HIGH;
				}

				if (furniture.HasUpg && (furniture.upg.Quality < 0.8f || furniture.upg.Broken))
				{
					furniture.upg.RepairMe(RepairFactor);
				}
			}
		}

		internal static void RestoreFullEnvironment()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Room room = Settings.sRoomManager.Rooms[i];
				room.RecalculateStateVariables(false);
				if (Helpers.GetProperty(Helpers.Settings, SettingKeys.FullRoomBrightness))
				{
					ApplyFullRoomBrightnessToRoom(room);
				}
			}
		}

		internal static void RestoreFullRoomBrightness()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				Room room = Settings.sRoomManager.Rooms[i];
				room.RecalculateStateVariables(false);
				if (Helpers.GetProperty(Helpers.Settings, SettingKeys.FullEnvironment))
				{
					ApplyFullEnvironmentToRoom(room);
				}
			}
		}

		internal static void ApplyDisableBurglars()
		{
			RemoveActorsOfGroup("Burglars");
		}

		internal static void ApplyDisableFireInspection()
		{
			RemoveActorsOfGroup("FireInspector");

			Settings.ActiveFireReport.Reset();
			Settings.PassedFireInspection = true;
		}

		internal static void ApplyDisableFurnitureStealing()
		{
			Settings.sRoomManager.AllFurniture.ForEach(x => x.CanSteal = false);
		}

		internal static void ApplyFullEnvironment()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				ApplyFullEnvironmentToRoom(Settings.sRoomManager.Rooms[i]);
			}
		}

		internal static void ApplyFullRoomBrightness()
		{
			for (int i = 0; i < Settings.sRoomManager.Rooms.Count; i++)
			{
				ApplyFullRoomBrightnessToRoom(Settings.sRoomManager.Rooms[i]);
			}
		}

		internal static void ApplyIncreaseBookshelfSkill()
		{
			Settings.sRoomManager.AllFurniture.ForEach(ApplyIncreaseBookshelfSkillToFurniture);
		}

		internal static void ApplyNoWaterElectricityToAllFurniture()
		{
			Settings.sRoomManager.AllFurniture.ForEach(ApplyNoWaterElectricityToFurniture);
		}

		internal static void ApplyDisableFiresFireStarter()
		{
			Settings.sRoomManager.AllFurniture.ForEach(ApplyDisableFiresFireStarterToFurniture);
		}

		// Despawned only matters for employees, so burglars and inspectors are removed with Writeable.DestroyGO (what AI.GuestDespawn calls); Actor.OnDestroy then unregisters them from ActorManager.Others.
		private static void RemoveActorsOfGroup(string group)
		{
			var actors = new List<Actor>(Settings.sActorManager.Others[group]);
			for (int i = 0; i < actors.Count; i++)
			{
				Settings.sActorManager.RemoveFromAwaiting(actors[i]);
				actors[i].DestroyGO();
			}
		}

		private static void ApplyCleanRoomsToRoom(Room room)
		{
			room.ClearDirt();
			room.Smell = 0f;
		}

		private static void ApplyNoSicknessToRoom(Room room)
		{
			room.GermCount = 0f;
		}

		private static void ApplyNoSicknessToActor(Actor actor)
		{
			if (actor.SpecialState == Actor.HomeState.Sick)
			{
				actor.SpecialState = Actor.HomeState.Default;
			}

			actor.GermAdd = 0f;
			actor.GermCount = 0f;
			actor.SickDays = 0;
		}

		private static void ApplyFullEnvironmentToRoom(Room room)
		{
			room.FurnEnvironment = Constants.ENV_FULL;
		}

		private static void ApplyFullRoomBrightnessToRoom(Room room)
		{
			room.IndirectLighting = Constants.ROOM_BRIGHTNESS_FULL;
		}

		private static void ApplyIncreaseBookshelfSkillToFurniture(Furniture furniture)
		{
			if (furniture.Type == "Bookshelf")
			{
				furniture.AuraValues[1] = Constants.BOOKSHELF_AURA_BOOSTED;
			}
		}

		private static void ApplyNoWaterElectricityToFurniture(Furniture furniture)
		{
			furniture.Water = 0;
			furniture.Wattage = 0;
		}

		private static void ApplyDisableFiresFireStarterToFurniture(Furniture furniture)
		{
			if (furniture.HasUpg && furniture.upg.FireStarter > 0.0f)
			{
				furniture.upg.FireStarter = 0.0f;
			}
		}
	}
}

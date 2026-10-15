namespace Trainer_v5
{
	internal static class EnvironmentFeatures
	{
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

		internal static void ApplyNoiseReduction()
		{
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
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
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
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

		// RepairMe adds one small step (scaled by this factor) to RepairTime and calls the private FixNow once it
		// reaches 1 - Quality. FixNow restores quality, clears Broken and the broken-furniture list, so a large
		// factor finishes the repair in one hour through the game's own path.
		private const float RepairFactor = 1000000f;

		internal static void ApplyNoMaintenance()
		{
			foreach (Furniture furniture in Settings.sRoomManager.AllFurniture)
			{
				switch (furniture.Type)
				{
					case "Chair":
						if (furniture.Comfort < Constants.CHAIR_COMFORT_LOW)
						{
							furniture.Comfort = Constants.CHAIR_COMFORT_HIGH;
						}
						goto case "Ventilation";
					case "CCTV":
					case "Computer":
					case "Lamp":
					case "Server":
					case "Product Printer":
					case "Radiator":
					case "Sink":
					case "Toilet":
					case "Ventilation":
						break;
					default:
						break;
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
			foreach (var burglar in Settings.sActorManager.Others["Burglars"])
			{
				burglar.Despawned = true;
				Settings.sActorManager.RemoveFromAwaiting(burglar);
			}
		}

		internal static void ApplyDisableFireInspection()
		{
			foreach (var fireInspector in Settings.sActorManager.Others["FireInspector"])
			{
				fireInspector.Despawned = true;
				Settings.sActorManager.RemoveFromAwaiting(fireInspector);
			}

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

		private static GameSettings Settings => GameSettings.Instance;

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
			TimeOfDay.Instance.Sick.Clear();

			if (actor.SpecialState == Actor.HomeState.Sick)
			{
				actor.SpecialState = Actor.HomeState.Default;
				actor.WasSick = true;
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

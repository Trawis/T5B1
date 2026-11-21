using System;
using System.Collections.Generic;
using Tyd;

namespace Trainer_v5.Experimental
{
	// 2 is what the game gives two equal personalities; the graph is saved with the game, so this is never undone.
	internal static class AmazingCompatibility
	{
		private const string ActionName = "Amazing Compatibility";
		internal const float Maximum = 2f;

		internal static void Run()
		{
			if (!ExperimentalOptions.Enabled || !Helpers.RequireGameReady(ActionName))
				return;

			GameSettings game = GameSettings.Instance;
			PersonalityGraph graph = game.Personalities;
			if (graph == null)
			{
				Notification.ShowError(Failed());
				return;
			}

			int count = PairsToRaise(graph).Count;
			if (count == 0)
			{
				Notification.ShowInfo(AlreadyApplied());
				return;
			}

			Confirmation.AskAndApply(ActionName, string.Format("AmazingCompatibilityConfirm".LocDef("Raise {0} personality pair(s) to the game's maximum compatibility?\nThis permanently changes the personality compatibility of this save, including its autosaves and every later save of it. There is no undo: disabling or removing the trainer does not restore it.\nIt affects current and future employees and makes HR personality matching for new hires neutral.\nBack up your save first."), count), () => Confirmation.ChangedUnless(ReferenceEquals(graph, game.Personalities)), () => Apply(game, graph));
		}

		// The remaining pairs are computed again, so pairs raised while the question was open are not counted.
		private static string Apply(GameSettings game, PersonalityGraph graph)
		{
			List<KeyValuePair<string, string>> pairs = PairsToRaise(graph);
			if (pairs.Count == 0)
			{
				Notification.ShowInfo(AlreadyApplied());
				return null;
			}

			PersonalityGraph source = BuildSource(graph, pairs);
			if (source == null)
			{
				Notification.ShowError(Failed());
				return null;
			}

			graph.MergeWith(source);
			foreach (Team team in game.sActorManager.Teams.Values)
			{
				team.CalculateCompatibility();
			}

			return string.Format("AmazingCompatibilityApplied".LocDef("Trainer: {0} personality pair(s) raised to maximum compatibility."), pairs.Count);
		}

		// Each pair of different personalities once, only where the graph is below the maximum, so nothing is lowered.
		internal static List<KeyValuePair<string, string>> PairsToRaise(PersonalityGraph graph)
		{
			var pairs = new List<KeyValuePair<string, string>>();
			List<string> names = graph.PersonalityTraits;
			for (int i = 0; i < names.Count; i++)
			{
				for (int j = i + 1; j < names.Count; j++)
				{
					if (graph[names[i], names[j]] < Maximum)
					{
						pairs.Add(new KeyValuePair<string, string>(names[i], names[j]));
					}
				}
			}

			return pairs;
		}

		// The game's reader requires traits for every named personality; any failure returns null before the live graph changes.
		internal static PersonalityGraph BuildSource(PersonalityGraph graph, List<KeyValuePair<string, string>> pairs)
		{
			try
			{
				var relationships = new Dictionary<string, TydList>();
				var order = new List<string>();
				foreach (var pair in pairs)
				{
					AddPersonality(relationships, order, pair.Key);
					AddPersonality(relationships, order, pair.Value);
					relationships[pair.Key].AddChild(new TydList(null, new TydNode[] { new TydString(null, pair.Value), new TydString(null, "2") }));
				}

				var personalities = new TydList("Personalities");
				foreach (string name in order)
				{
					Employee.Trait[] traits;
					if (!graph.Traits.TryGetValue(name, out traits) || traits == null || traits.Length == 0)
						return null;

					var traitNodes = new TydNode[traits.Length];
					for (int i = 0; i < traits.Length; i++)
					{
						traitNodes[i] = new TydString(null, traits[i].ToString());
					}

					personalities.AddChild(new TydTable(null, new TydNode[] { new TydString("Name", name), new TydList("Traits", traitNodes), relationships[name] }));
				}

				return new PersonalityGraph(new TydTable("PersonalityGraph", new TydNode[] { personalities }));
			}
			catch (Exception e)
			{
				(ActionName + ": " + e.Message).Log();
				return null;
			}
		}

		private static void AddPersonality(Dictionary<string, TydList> relationships, List<string> order, string name)
		{
			if (!relationships.ContainsKey(name))
			{
				relationships[name] = new TydList("Relationships");
				order.Add(name);
			}
		}

		private static string AlreadyApplied()
		{
			return "AmazingCompatibilityAlready".LocDef("Every personality pair is already at the game's maximum compatibility. Nothing was changed.");
		}

		private static string Failed()
		{
			return "AmazingCompatibilityFailed".LocDef("The personality compatibility could not be read or built. Nothing was changed.");
		}
	}
}

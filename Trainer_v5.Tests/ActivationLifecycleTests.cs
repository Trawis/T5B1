using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Xunit;

namespace Trainer_v5.Tests
{
	// Trainer UI and callbacks act only while the mod is active and only for the game they were made in.
	public class ActivationLifecycleTests
	{
		private static string Behaviour()
		{
			return SourceCode.Read("TrainerBehaviour.cs");
		}

		[Fact]
		public void ABindingRunsWhileTheTrainerIsActive()
		{
			TrainerLifecycle.Activate();
			var runs = 0;
			var bound = TrainerLifecycle.Bind(() => runs++);

			bound();
			bound();

			Assert.Equal(2, runs);
		}

		[Fact]
		public void DeactivationStopsEveryEarlierBinding()
		{
			TrainerLifecycle.Activate();
			var runs = 0;
			var button = TrainerLifecycle.Bind(() => runs++);
			string answer = null;
			var dialog = TrainerLifecycle.Bind<string>(text => answer = text);

			TrainerLifecycle.Deactivate();
			button();
			dialog("100");

			Assert.False(TrainerLifecycle.Active);
			Assert.Equal(0, runs);
			Assert.Null(answer);
		}

		[Fact]
		public void ReactivationDoesNotReviveAStaleBindingButNewOnesWork()
		{
			TrainerLifecycle.Activate();
			var stale = 0;
			var fresh = 0;
			var old = TrainerLifecycle.Bind(() => stale++);

			TrainerLifecycle.Deactivate();
			TrainerLifecycle.Activate();
			var current = TrainerLifecycle.Bind(() => fresh++);
			old();
			current();

			Assert.Equal(0, stale);
			Assert.Equal(1, fresh);
		}

		[Fact]
		public void ABindingMadeWhileInactiveNeverRuns()
		{
			TrainerLifecycle.Deactivate();
			var runs = 0;
			var bound = TrainerLifecycle.Bind(() => runs++);

			bound();
			TrainerLifecycle.Activate();
			bound();

			Assert.Equal(0, runs);
		}

		[Fact]
		public void AGameChangeStopsTheBindingsOfThePreviousGame()
		{
			TrainerLifecycle.Activate();
			var runs = 0;
			int value = 0;
			var previous = TrainerLifecycle.Bind(() => runs++);
			var previousDialog = TrainerLifecycle.Bind<int>(v => value = v);
			var generation = TrainerLifecycle.Generation;

			TrainerLifecycle.NewGame();
			previous();
			previousDialog(5);

			Assert.True(TrainerLifecycle.Active);
			Assert.False(TrainerLifecycle.IsCurrent(generation));
			Assert.Equal(0, runs);
			Assert.Equal(0, value);
		}

		[Fact]
		public void TheGeneratedValueIsPassedThrough()
		{
			TrainerLifecycle.Activate();
			var seen = new List<bool>();
			var bound = TrainerLifecycle.Bind<bool>(seen.Add);

			bound(true);
			bound(false);

			Assert.Equal(new[] { true, false }, seen);
		}

		[Fact]
		public void DeactivationStopsTheTrainerBeforeTearingDownItsUi()
		{
			var deactivate = SourceCode.Body(Behaviour(), "public override void OnDeactivate()");

			SourceCode.AssertInOrder(deactivate, "TrainerLifecycle.Deactivate();", "TearDownUI();", "callbacks die before the UI goes");
			SourceCode.AssertInOrder(deactivate, "TearDownUI();", "UnsubscribeFromEvents();", "the UI goes with the subscriptions");
		}

		[Fact]
		public void ActivationInTheMainSceneInstallsTheUiForTheRunningGame()
		{
			var source = Behaviour();
			var activate = SourceCode.Body(source, "public override void OnActivate()");
			var install = SourceCode.Body(source, "private void InstallForRunningGame()");

			SourceCode.AssertInOrder(activate, "TrainerLifecycle.Activate();", "InstallForRunningGame", "controls are bound to the new session");
			SourceCode.AssertInOrder(activate, "SubscribeToEvents();", "InstallForRunningGame", "a later GameReady is still received");
			SourceCode.AssertInOrder(install, "TearDownUI();", "Main.CreateUIButtons();", "a reinstall leaves exactly one set");
			SourceCode.AssertInOrder(install, "if (Helpers.IsSimulationReady())", "OnGameReady(", "GameReady is replayed only once it has fired");
		}

		[Fact]
		public void TearDownCoversEveryTrainerOwnedWindowAndControl()
		{
			var tearDown = SourceCode.Body(Behaviour(), "private static void TearDownUI()");

			Assert.Contains("Main.DestroyUIButtons", tearDown);
			Assert.Contains("SettingsWindow.Discard", tearDown);
			Assert.Contains("EmployeesWindowTrainer.Discard", tearDown);
			Assert.Contains("EmployeeTraitChangeWindow.Instance.Discard()", tearDown);
			Assert.Contains("EmployeeDemandChangeWindow.Instance.Discard()", tearDown);
			Assert.Contains("EmployeeLeadSpecChangeWindow.Instance.Discard()", tearDown);
			Assert.Contains("EmployeeSkillChangeWindow.Discard", tearDown);
		}

		[Theory]
		[InlineData("SettingsWindow.cs", "public static void Discard()", "Window != null")]
		[InlineData("Window/EmployeeSkillChangeWindow.cs", "public static void Discard()", "Window != null")]
		[InlineData("Window/EmployeeTraitChangeWindow.cs", "public void Discard()", "_window != null")]
		[InlineData("Window/EmployeeDemandChangeWindow.cs", "public void Discard()", "_window != null")]
		[InlineData("Window/EmployeeLeadSpecChangeWindow.cs", "public void Discard()", "_window != null")]
		[InlineData("EmployeesWindowTrainer.cs", "public static void Discard()", "control != null")]
		public void DiscardSkipsObjectsAlreadyDestroyedAndForgetsThem(string file, string signature, string check)
		{
			var discard = SourceCode.Body(SourceCode.Read(file.Split('/')), signature);

			SourceCode.AssertInOrder(discard, check, "Destroy(", "a destroyed or missing object is skipped");
			Assert.Matches(@"(Reset\(\);|= null;)", discard);
		}

		[Fact]
		public void EnteringOrLeavingAGameStartsANewSessionWithoutTheOldUi()
		{
			var source = Behaviour();
			var mainScene = SourceCode.Span(source, "case SceneRoute.MainScene:", "break;");
			var leave = SourceCode.Body(source, "private void LeaveGame()");

			SourceCode.AssertInOrder(mainScene, "TrainerLifecycle.NewGame();", "TearDownUI();", "old callbacks die first");
			SourceCode.AssertInOrder(mainScene, "TearDownUI();", "Main.CreateUIButtons();", "the old UI goes before the new one");
			SourceCode.AssertInOrder(leave, "TrainerLifecycle.NewGame();", "TearDownUI();", "old callbacks die first");
		}

		[Fact]
		public void TheEmployeesToolbarRemembersItsControlsSoAReinstallReplacesThem()
		{
			var install = SourceCode.Body(SourceCode.Read("EmployeesWindowTrainer.cs"), "public static void Install()");

			SourceCode.AssertInOrder(install, "if (!placed) return;", "Discard();", "only a complete set replaces the old one");
			SourceCode.AssertInOrder(install, "Discard();", "_controls = components;", "the old set is removed before the new one is kept");
			SourceCode.AssertInOrder(install, "_controls = components;", "_installedOn = target;", "both are recorded together");
		}

		[Fact]
		public void TrainerControlsAreBoundAndOnlyTheModOptionsAreExempt()
		{
			var factory = SourceCode.Read("UIFactory.cs");
			var options = SourceCode.Body(SourceCode.Read("Main.cs"), "public override void ConstructOptionsScreen(");

			Assert.Contains("TrainerLifecycle.Bind(", SourceCode.Body(factory, "public static Button Button("));
			Assert.Contains("TrainerLifecycle.Bind<bool>(", SourceCode.Body(factory, "public static Toggle Toggle("));
			Assert.Contains("TrainerLifecycle.Bind<string>(", SourceCode.Body(factory, "public static InputField InputBox("));
			Assert.Equal(2, Regex.Matches(options, "whileInactive: true").Count);
			SourceCode.AssertInOrder(SourceCode.Body(factory, "public static Toggle Toggle("), "if (whileInactive)", "AddListener(action)", "only an Options toggle is unbound");
		}

		[Fact]
		public void DialogAnswersAndConfirmationsAreBound()
		{
			Assert.Contains("TrainerLifecycle.Bind(onFinish)", SourceCode.Body(SourceCode.Read("InputHelper.cs"), "public static void RequestText("));
			var ask = SourceCode.Body(SourceCode.Read("Confirmation.cs"), "internal static void Ask(");
			SourceCode.AssertInOrder(ask, "TrainerLifecycle.Bind(", "ShowMessageBox(", "the answer is bound before the dialog opens");
			Assert.Contains("() => confirm()", ask);
		}

		[Fact]
		public void NoCallbackReachesTheGameUiOutsideTheBoundHelpers()
		{
			var allowed = new Dictionary<string, string[]>
			{
				{ "UIFactory.cs", new[] { "AddListener" } },
				{ "InputHelper.cs", new[] { "SpawnInputDialog" } },
				{ "Confirmation.cs", new[] { "ShowMessageBox" } },
				// The close button only clears Shown; the drop-down listener runs a bound action.
				{ "SettingsWindow.cs", new[] { "AddListener" } },
			};

			foreach (var file in Directory.GetFiles(RepositoryFiles.SourceDirectory, "*.cs", SearchOption.AllDirectories))
			{
				var name = Path.GetFileName(file);
				var source = SourceCode.Normalize(File.ReadAllText(file));
				foreach (var api in new[] { "AddListener", "SpawnInputDialog", "ShowMessageBox" })
				{
					string[] apis;
					if (allowed.TryGetValue(name, out apis) && Array.IndexOf(apis, api) >= 0)
					{
						continue;
					}

					Assert.False(source.Contains(api), name + " calls " + api + " directly instead of a bound helper");
				}
			}

			var settings = SourceCode.Read("SettingsWindow.cs");
			Assert.Equal(2, Regex.Matches(settings, "AddListener").Count);
			Assert.Contains("AddListener(() => select())", settings);
		}

		[Fact]
		public void HotkeysDoNothingWhileTheTrainerIsInactive()
		{
			var update = SourceCode.Body(Behaviour(), "private void Update()");

			SourceCode.AssertInOrder(update, "!TrainerLifecycle.Active", "KeyCode.F1", "the hotkeys are behind the active check");
		}
	}
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class SettingsWindow : MonoBehaviour
	{
		private static readonly string _title = $"Trainer Settings, by Trawis (v{Helpers.Version}) | GAME VERSION: {Helpers.GetGameVersion()}";

		// The page last shown, kept for the running game only (not a save key). Reset() leaves it alone.
		private static SettingsTab _activeTab;
		// True while the controls are being set from the stored values; the toggle handler must not flip a setting then.
		private static bool _refreshing;
		private static Button[] _tabButtons;
		private static GameObject[] _tabPages;
		private static List<ToggleBinding> _toggleBindings;
		private static GUICombobox _efficiencyComboBox;
		private static GUICombobox _leadEfficiencyComboBox;

		public static GUIWindow Window { get; set; }
		public static bool Shown { get; set; }

		public static void Toggle()
		{
			if (Window == null)
			{
				Init();
			}
			else
			{
				Window.Toggle();
				if (Window.Shown)
				{
					RefreshFromSettings();
				}
			}

			Shown = Window.Shown;
		}

		// The game destroys its windows with the scene; a stale Shown would make F1 do nothing in the next game.
		// No control outlives it, so the lists used to refresh them are dropped too.
		public static void Reset()
		{
			Window = null;
			Shown = false;
			_refreshing = false;
			_tabButtons = null;
			_tabPages = null;
			_toggleBindings = null;
			_efficiencyComboBox = null;
			_leadEfficiencyComboBox = null;
		}

		private static void Init()
		{
			Window = UIHelper.SpawnWindow(_title, "TrainerSettings", "TrainerSettingsPanel");

			if (Window.name == "TrainerSettings")
			{
				var closeButtons = Window.GetComponentsInChildren<Button>()
				  .Where(x => x.name == "CloseButton")
				  .ToList();

				if (closeButtons.Count == 1)
				{
					closeButtons[0].onClick.AddListener(() => Shown = false);
				}
				else if (closeButtons.Count == 0)
				{
					"SettingsWindow: no CloseButton found on the spawned window; close button listener was not attached.".Log();
				}
				else
				{
					$"SettingsWindow: expected exactly one CloseButton but found {closeButtons.Count}; close button listener was not attached.".Log();
				}
			}

			_toggleBindings = new List<ToggleBinding>();
			var actionCells = CreateSectionLists();
			var toggleCells = CreateSectionLists();
			var settingsCells = new List<GameObject>();

			BuildEfficiency(settingsCells);
			BuildActions(actionCells, settingsCells);
			BuildToggles(toggleCells);

			var actionDomains = SettingsWindowState.ActionDomains();
			var toggleDomains = SettingsWindowState.ToggleDomains();
			int actionRows = SettingsWindowState.PageRows(CountCells(actionCells, actionDomains), SettingsWindowState.Columns);
			int toggleRows = SettingsWindowState.PageRows(CountCells(toggleCells, toggleDomains), SettingsWindowState.Columns);
			int maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, 1);
			int visibleRows = SettingsWindowState.VisibleRows(System.Math.Max(actionRows, toggleRows), SettingsWindowState.SettingsPageRows, maxVisibleRows);

			var actionsPage = CreateTabPage("ActionsPage", actionRows, visibleRows);
			PlaceSections(actionsPage.Content, actionCells, actionDomains);
			var togglesPage = CreateTabPage("TogglesPage", toggleRows, visibleRows);
			PlaceSections(togglesPage.Content, toggleCells, toggleDomains);
			var settingsPage = CreateTabPage("SettingsPage", SettingsWindowState.SettingsPageRows, visibleRows);
			PlaceSettingsPage(settingsPage.Content, settingsCells);

			_tabPages = new[] { actionsPage.Root, togglesPage.Root, settingsPage.Root };
			BuildTabStrip();
			SetActiveTab(_activeTab);

			// Title bar and tab strip rows, then the content rows.
			Window.SetWindowSize(visibleRows + 1, SettingsWindowState.WindowWidth);
		}

		// Shows one page and hides the others. Everything was built once in Init, so this creates nothing and adds no listeners.
		private static void SetActiveTab(SettingsTab tab)
		{
			_activeTab = SettingsWindowState.ClampTab(tab);
			if (_tabPages == null || _tabButtons == null)
			{
				return;
			}

			int active = SettingsWindowState.PageIndex(_activeTab);
			for (int i = 0; i < _tabPages.Length; i++)
			{
				_tabPages[i].SetActive(i == active);
				MarkTab(_tabButtons[i], i == active);
			}
		}

		// The shown page has a disabled, bold button; the others stay clickable.
		private static void MarkTab(Button button, bool isActive)
		{
			button.interactable = !isActive;
			button.GetComponentInChildren<Text>().fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
		}

		// The window outlives a close, so the controls can be behind the stored values (a partial load changes them).
		// The UI version has no way to set a toggle without raising its change event, hence the guard.
		private static void RefreshFromSettings()
		{
			if (_toggleBindings == null)
			{
				return;
			}

			var settings = Helpers.Settings;
			_refreshing = true;
			try
			{
				foreach (var binding in _toggleBindings)
				{
					if (binding.Control != null)
					{
						binding.Control.isOn = settings.Get(binding.Key);
					}
				}

				RefreshEfficiency(_efficiencyComboBox, SettingKeys.EfficiencyStore);
				RefreshEfficiency(_leadEfficiencyComboBox, SettingKeys.LeadEfficiencyStore);
			}
			finally
			{
				_refreshing = false;
			}
		}

		private static void RefreshEfficiency(GUICombobox comboBox, StoreKey key)
		{
			if (comboBox != null)
			{
				comboBox.UpdateSelectionSupressEvents(Helpers.EfficiencySelectItems.GetIndex(Helpers.StoresSettings, key, 2));
			}
		}

		private static void BuildTabStrip()
		{
			var labels = new[] { "TabActions".LocDef("Actions"), "TabToggles".LocDef("Toggles"), "TabSettings".LocDef("Settings") };
			var names = new[] { "ActionsTab", "TogglesTab", "SettingsTab" };
			_tabButtons = new Button[labels.Length];
			for (int i = 0; i < labels.Length; i++)
			{
				_tabButtons[i] = CreateTabButton(i, labels[i], names[i]);
			}
		}

		private static Button CreateTabButton(int index, string label, string name)
		{
			var tab = SettingsTab.Actions + index;
			var button = UIFactory.Button(label, () => SetActiveTab(tab), name);
			WindowManager.AddElementToWindow(button.gameObject, Window, UIHelper.GetGridSlot(index, SettingsWindowState.Columns, 0), new Rect(0, 0, 0, 0));
			return button;
		}

		private static void BuildActions(List<GameObject>[] actionCells, List<GameObject> settingsCells)
		{
			// The three product buttons that read the name sit next to the field; the static keeps the text across rebuilds.
			actionCells[(int)TrainerDomain.Product].Add(UIHelper.CreateInputBox(SettingsWindowState.ProductNameFieldText(Helpers.ProductPriceName, "ProductName".LocDef("Product Name Here")), boxText => Helpers.ProductPriceName = boxText));

			foreach (var row in SettingActionRows.Create())
			{
				var button = UIHelper.CreateButton(row.LocKey.LocDef(row.DefaultText), row.Action);
				if (row.Tab == SettingsTab.Settings)
				{
					settingsCells.Add(button);
				}
				else
				{
					actionCells[(int)row.Domain].Add(button);
				}
			}
		}

		private static void BuildToggles(List<GameObject>[] toggleCells)
		{
			var rows = SettingToggleRows.Create();
			rows.AddRange(SettingToggleRows.CreateImmediate());
			foreach (var row in rows)
			{
				toggleCells[(int)row.Domain].Add(CreateSettingToggle(row));
			}
		}

		private static GameObject CreateSettingToggle(SettingToggleRow row)
		{
			var key = row.Key;
			var control = UIHelper.CreateToggle(row.LocKey.LocDef(row.DefaultText), Helpers.Settings.Get(key), a => OnToggleChanged(key));
			_toggleBindings.Add(new ToggleBinding(control.GetComponent<Toggle>(), key));
			return control;
		}

		// A toggle flips its setting; the three immediate ones also apply the new value to the running game.
		private static void OnToggleChanged(ToggleKey key)
		{
			if (_refreshing)
			{
				return;
			}

			var settings = Helpers.Settings;
			settings.Toggle(key);
			bool enabled = settings.Get(key);
			if (key == SettingKeys.UnlimitedSubsidiaries)
			{
				CompanyFeatures.ApplyUnlimitedSubsidiaries(enabled);
			}
			else if (key == SettingKeys.ForceLights)
			{
				EnvironmentFeatures.ApplyForceLights(enabled);
			}
			else if (key == SettingKeys.ShowRoomCeilings)
			{
				EnvironmentFeatures.ApplyShowRoomCeilings(enabled);
			}
		}

		// Adds the label and drop-down of both efficiency settings first, the order PlaceSettingsPage lays them out in.
		private static void BuildEfficiency(List<GameObject> settingsCells)
		{
			var storesSettings = Helpers.StoresSettings;
			var efficiencySelectItems = Helpers.EfficiencySelectItems;

			settingsCells.Add(UIHelper.CreateLabel("EfficiencyLabel".LocDef("Efficiency")));
			_efficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, SettingKeys.EfficiencyStore, 2)
			);
			var efficiency = _efficiencyComboBox;
			efficiency.OnSelectedChanged.AddListener(() => storesSettings.Set(SettingKeys.EfficiencyStore, efficiencySelectItems.GetAt(efficiency.Selected).Value));
			settingsCells.Add(efficiency.gameObject);

			settingsCells.Add(UIHelper.CreateLabel("LeadEfficiencyLabel".LocDef("Lead Efficiency")));
			_leadEfficiencyComboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, SettingKeys.LeadEfficiencyStore, 2)
			);
			var leadEfficiency = _leadEfficiencyComboBox;
			leadEfficiency.OnSelectedChanged.AddListener(() => storesSettings.Set(SettingKeys.LeadEfficiencyStore, efficiencySelectItems.GetAt(leadEfficiency.Selected).Value));
			settingsCells.Add(leadEfficiency.gameObject);
		}

		// Efficiency label and drop-down, lead efficiency label and drop-down, a gap, then the other Settings buttons.
		// The drop-downs open from a fixed place because this page never scrolls.
		private static void PlaceSettingsPage(GameObject container, List<GameObject> settingsCells)
		{
			var zeroAnchors = new Rect(0, 0, 0, 0);
			for (int i = 0; i < settingsCells.Count; i++)
			{
				int row = i < 4 ? i : i + 1;
				WindowManager.AddElementToElement(settingsCells[i], container, UIHelper.GetGridSlot(row, 1, 0), zeroAnchors);
			}
		}

		private static List<GameObject>[] CreateSectionLists()
		{
			var sections = new List<GameObject>[(int)TrainerDomain.Environment + 1];
			for (int i = 0; i < sections.Length; i++)
			{
				sections[i] = new List<GameObject>();
			}

			return sections;
		}

		private static int[] CountCells(List<GameObject>[] sections, TrainerDomain[] order)
		{
			var counts = new int[order.Length];
			for (int i = 0; i < order.Length; i++)
			{
				counts[i] = sections[(int)order[i]].Count;
			}

			return counts;
		}

		// A header row per section that has controls, then its controls filled row by row.
		private static void PlaceSections(GameObject container, List<GameObject>[] sections, TrainerDomain[] order)
		{
			var zeroAnchors = new Rect(0, 0, 0, 0);
			int headerWidth = UIHelper.GetGridColumnX(SettingsWindowState.Columns - 1) + Constants.ELEMENT_WIDTH - Constants.FIRST_COLUMN;
			int row = 0;
			foreach (var domain in order)
			{
				var cells = sections[(int)domain];
				if (cells.Count == 0)
				{
					continue;
				}

				var header = UIFactory.Label(DomainLabel(domain), new TextStyle { Alignment = TextAnchor.MiddleCenter, FontStyle = FontStyle.Bold });
				header.name = domain + "_Header";
				WindowManager.AddElementToElement(header.gameObject, container, new Rect(Constants.FIRST_COLUMN, row * Constants.ELEMENT_HEIGHT, headerWidth, Constants.ELEMENT_HEIGHT), zeroAnchors);
				row++;

				for (int i = 0; i < cells.Count; i++)
				{
					WindowManager.AddElementToElement(cells[i], container, UIHelper.GetGridSlot(i, SettingsWindowState.Columns, row), zeroAnchors);
				}

				row += SettingsWindowState.SectionRows(cells.Count, SettingsWindowState.Columns);
			}
		}

		private static string DomainLabel(TrainerDomain domain)
		{
			switch (domain)
			{
				case TrainerDomain.Company:
					return "DomainCompany".LocDef("Company");
				case TrainerDomain.Employee:
					return "DomainEmployee".LocDef("Employee");
				case TrainerDomain.Product:
					return "DomainProduct".LocDef("Product");
				case TrainerDomain.Work:
					return "DomainWork".LocDef("Work");
				case TrainerDomain.Environment:
					return "DomainEnvironment".LocDef("Environment");
				default:
					return string.Empty;
			}
		}

		// One page below the tab strip. A page taller than the window scrolls on its own (the same ScrollRect set-up the
		// window used for its columns); a page that fits gets no scroll area. Each page keeps its own scroll position.
		private static TabPage CreateTabPage(string name, int rows, int visibleRows)
		{
			var zeroAnchors = new Rect(0, 0, 0, 0);
			var viewport = WindowManager.SpawnPanel();
			viewport.gameObject.name = name;
			WindowManager.AddElementToWindow(viewport.gameObject, Window, new Rect(0, Constants.ELEMENT_HEIGHT, SettingsWindowState.WindowWidth, visibleRows * Constants.ELEMENT_HEIGHT), zeroAnchors);
			if (rows <= visibleRows)
			{
				return new TabPage(viewport.gameObject, viewport.gameObject);
			}

			viewport.gameObject.AddComponent<RectMask2D>();
			var scrollRect = viewport.gameObject.AddComponent<ScrollRect>();

			var content = WindowManager.SpawnPanel();
			content.gameObject.name = name + "Content";
			WindowManager.AddElementToElement(content.gameObject, viewport.gameObject, new Rect(0, 0, SettingsWindowState.WindowWidth, rows * Constants.ELEMENT_HEIGHT), zeroAnchors);

			scrollRect.content = content;
			scrollRect.horizontal = false;
			scrollRect.vertical = true;
			scrollRect.movementType = ScrollRect.MovementType.Clamped;
			scrollRect.scrollSensitivity = Constants.ELEMENT_HEIGHT;

			return new TabPage(viewport.gameObject, content.gameObject);
		}

		// A toggle control and the setting it shows. Not generic (see Workshop Source Build).
		private sealed class ToggleBinding
		{
			public readonly Toggle Control;
			public readonly ToggleKey Key;

			public ToggleBinding(Toggle control, ToggleKey key)
			{
				Control = control;
				Key = key;
			}
		}

		// The object a page is shown and hidden by, and the one its controls are added to.
		private sealed class TabPage
		{
			public readonly GameObject Root;
			public readonly GameObject Content;

			public TabPage(GameObject root, GameObject content)
			{
				Root = root;
				Content = content;
			}
		}
	}
}

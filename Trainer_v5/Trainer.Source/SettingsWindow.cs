using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class SettingsWindow : MonoBehaviour
	{
		private static readonly string _title = $"Trainer Settings, by Trawis (v{Helpers.Version}) | GAME VERSION: {Helpers.GetGameVersion()}";

		// The page last shown, kept for the running game only (not a save key).
		private static SettingsTab _activeTab;
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

			Window.SetMinSize(SettingsWindowState.WindowWidth, SettingsWindowState.WindowHeight(visibleRows));
			Window.LockWindowSize();
		}

		// Everything is built once in Init, so a tab switch creates nothing and adds no listeners.
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

		private static void MarkTab(Button button, bool isActive)
		{
			button.interactable = !isActive;
			button.GetComponentInChildren<Text>().fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
		}

		// The UI cannot set a toggle without raising its change event, hence the _refreshing guard.
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
			UIHelper.PlaceInWindow(button.gameObject, Window, UIHelper.GetGridSlot(index, SettingsWindowState.Columns, 0));
			return button;
		}

		private static void BuildActions(List<GameObject>[] actionCells, List<GameObject> settingsCells)
		{
			// Helpers.ProductPriceName is static so the field text survives a rebuild.
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

		// Efficiency controls come first: the order PlaceSettingsPage lays them out in.
		private static void BuildEfficiency(List<GameObject> settingsCells)
		{
			_efficiencyComboBox = AddEfficiencyRow(settingsCells, "EfficiencyLabel".LocDef("Efficiency"), SettingKeys.EfficiencyStore);
			_leadEfficiencyComboBox = AddEfficiencyRow(settingsCells, "LeadEfficiencyLabel".LocDef("Lead Efficiency"), SettingKeys.LeadEfficiencyStore);
		}

		private static GUICombobox AddEfficiencyRow(List<GameObject> settingsCells, string label, StoreKey key)
		{
			var storesSettings = Helpers.StoresSettings;
			var efficiencySelectItems = Helpers.EfficiencySelectItems;

			var labelCell = UIHelper.CreateLabel(label);
			labelCell.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
			settingsCells.Add(labelCell);
			var comboBox = UIHelper.CreateComboBox(
				selectableItems: efficiencySelectItems,
				selection: efficiencySelectItems.GetIndex(storesSettings, key, 2)
			);
			comboBox.OnSelectedChanged.AddListener(() => storesSettings.Set(key, efficiencySelectItems.GetAt(comboBox.Selected).Value));
			settingsCells.Add(comboBox.gameObject);
			return comboBox;
		}

		// The drop-downs open from a fixed place because this page never scrolls.
		private static void PlaceSettingsPage(GameObject container, List<GameObject> settingsCells)
		{
			for (int i = 0; i < settingsCells.Count; i++)
			{
				var slot = new Rect(
					UIHelper.GetGridColumnX(SettingsWindowState.SettingsCellColumn(i)),
					SettingsWindowState.SettingsCellRow(i) * Constants.ELEMENT_HEIGHT,
					Constants.ELEMENT_WIDTH,
					Constants.ELEMENT_HEIGHT);
				UIHelper.PlaceInElement(settingsCells[i], container, slot);
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

		private static void PlaceSections(GameObject container, List<GameObject>[] sections, TrainerDomain[] order)
		{
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
				UIHelper.PlaceInElement(header.gameObject, container, new Rect(Constants.FIRST_COLUMN, row * Constants.ELEMENT_HEIGHT, headerWidth, Constants.ELEMENT_HEIGHT));
				row++;

				for (int i = 0; i < cells.Count; i++)
				{
					UIHelper.PlaceInElement(cells[i], container, UIHelper.GetGridSlot(i, SettingsWindowState.Columns, row));
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

		// A page taller than the window scrolls on its own; one that fits gets no scroll area.
		private static TabPage CreateTabPage(string name, int rows, int visibleRows)
		{
			var viewport = WindowManager.SpawnPanel();
			viewport.gameObject.name = name;
			UIHelper.PlaceInWindow(viewport.gameObject, Window, new Rect(0, Constants.ELEMENT_HEIGHT, SettingsWindowState.WindowWidth, visibleRows * Constants.ELEMENT_HEIGHT));
			if (rows <= visibleRows)
			{
				return new TabPage(viewport.gameObject, viewport.gameObject);
			}

			var content = WindowManager.SpawnPanel();
			content.gameObject.name = name + "Content";
			UIHelper.PlaceInElement(content.gameObject, viewport.gameObject, new Rect(0, 0, SettingsWindowState.WindowWidth, rows * Constants.ELEMENT_HEIGHT));

			UIHelper.MakeVerticallyScrollable(viewport.gameObject, content, Constants.ELEMENT_HEIGHT);

			return new TabPage(viewport.gameObject, content.gameObject);
		}

		// Not generic: the game's source-mod compiler emits invalid IL for fields of user-defined generic types.
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

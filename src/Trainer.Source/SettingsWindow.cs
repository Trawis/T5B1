using System.Collections.Generic;
using Trainer_v5.Experimental;
using UnityEngine;
using UnityEngine.UI;

namespace Trainer_v5
{
	public class SettingsWindow : MonoBehaviour
	{
		// The page last shown, kept for the running game only (not a save key).
		private static SettingsTab _activeTab;
		private static bool _refreshing;
		private static Button[] _tabButtons;
		private static GameObject[] _tabPages;
		private static List<ToggleBinding> _toggleBindings;
		private static GUICombobox _efficiencyComboBox;
		private static GUICombobox _leadEfficiencyComboBox;

		public static GUIWindow Window { get; set; }

		// Read from the native window each time: the game can close or destroy it without the trainer knowing.
		public static bool Shown => Window != null && Window.Shown;

		public static void Toggle()
		{
			if (Window == null)
			{
				Init();
			}
			else
			{
				Window.Toggle();
				if (Shown)
				{
					RefreshFromSettings();
				}
			}
		}

		// GUIWindow.Show opens a hidden window, focuses a visible one and expands a collapsed one.
		public static void Open()
		{
			if (Window == null)
			{
				Init();
			}
			else
			{
				Window.Show();
				RefreshFromSettings();
			}
		}

		public static void Close()
		{
			if (Shown)
			{
				Window.Close();
			}
		}

		// The game destroys its windows with the scene; the stale references are dropped with it.
		public static void Reset()
		{
			Window = null;
			_refreshing = false;
			_tabButtons = null;
			_tabPages = null;
			_toggleBindings = null;
			_efficiencyComboBox = null;
			_leadEfficiencyComboBox = null;
		}

		private static void Init()
		{
			Window = UIHelper.SpawnWindow(string.Format("SettingsWindowTitle".LocDef("Trainer Settings, by Trawis (v{0}) | GAME VERSION: {1}"), Helpers.Version, Helpers.GetGameVersion()), "TrainerSettings", "TrainerSettingsPanel");
			Build();
		}

		// Replaces the content of the existing window, so its position, shown and collapsed state stay native.
		public static void Rebuild()
		{
			if (Window == null || _tabButtons == null || _tabPages == null)
			{
				return;
			}

			foreach (var button in _tabButtons)
			{
				Destroy(button.gameObject);
			}

			foreach (var page in _tabPages)
			{
				Destroy(page);
			}

			// Locking the size of a collapsed window would expand it; collapse restores the size it had before.
			bool collapsed = Window.IsCollapsed;
			if (collapsed)
			{
				Window.ToggleCollapse();
			}

			Build();
			if (collapsed)
			{
				Window.ToggleCollapse();
			}
		}

		private static void Build()
		{
			_toggleBindings = new List<ToggleBinding>();
			var actionCells = CreateSectionLists();
			var experimentalActionCells = CreateSectionLists();
			var toggleCells = CreateSectionLists();
			var experimentalToggleCells = CreateSectionLists();
			var settingsCells = new List<GameObject>();

			BuildEfficiency(settingsCells);
			BuildActions(actionCells, experimentalActionCells, settingsCells);
			BuildToggles(toggleCells, experimentalToggleCells);

			var actionDomains = SettingsWindowState.ActionDomains();
			var toggleDomains = SettingsWindowState.ToggleDomains();
			int actionRows = PageRows(actionCells, experimentalActionCells, actionDomains);
			int toggleRows = PageRows(toggleCells, experimentalToggleCells, toggleDomains);
			int maxVisibleRows = UIHelper.GetMaxVisibleRows(Constants.ELEMENT_HEIGHT, 1);
			int visibleRows = SettingsWindowState.VisibleRows(System.Math.Max(actionRows, toggleRows), SettingsWindowState.SettingsPageRows, maxVisibleRows);

			var actionsPage = CreateTabPage("ActionsPage", actionRows, visibleRows);
			PlaceSections(actionsPage.Content, actionCells, experimentalActionCells, actionDomains);
			var togglesPage = CreateTabPage("TogglesPage", toggleRows, visibleRows);
			PlaceSections(togglesPage.Content, toggleCells, experimentalToggleCells, toggleDomains);
			var settingsPage = CreateTabPage("SettingsPage", SettingsWindowState.SettingsPageRows, visibleRows);
			PlaceSettingsPage(settingsPage.Content, settingsCells);

			_tabPages = new[] { actionsPage.Root, togglesPage.Root, settingsPage.Root };
			BuildTabStrip();
			SetActiveTab(_activeTab);

			Window.SetMinSize(SettingsWindowState.WindowWidth, SettingsWindowState.WindowHeight(visibleRows));
			Window.LockWindowSize();
		}

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
				comboBox.UpdateSelectionSupressEvents(EfficiencyChoices().GetIndex(Helpers.StoresSettings, key, 2));
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

		private static void BuildActions(List<GameObject>[] actionCells, List<GameObject>[] experimentalCells, List<GameObject> settingsCells)
		{
			actionCells[(int)TrainerDomain.Product].Add(UIHelper.CreateInputBox(SettingsWindowState.ProductNameFieldText(ProductFeatures.ProductNameInput, "ProductName".LocDef("Product Name Here")), boxText => ProductFeatures.ProductNameInput = boxText));

			var rows = SettingActionRows.Create();
			int regularCount = rows.Count;
			if (ExperimentalOptions.Enabled)
			{
				rows.AddRange(SettingActionRows.CreateExperimental());
			}

			for (int i = 0; i < rows.Count; i++)
			{
				var row = rows[i];
				var button = UIHelper.CreateButton(row.LocKey.LocDef(row.DefaultText), row.Action);
				if (i >= regularCount)
				{
					experimentalCells[(int)row.Domain].Add(button);
				}
				else if (row.Tab == SettingsTab.Settings)
				{
					settingsCells.Add(button);
				}
				else
				{
					actionCells[(int)row.Domain].Add(button);
				}
			}
		}

		private static void BuildToggles(List<GameObject>[] toggleCells, List<GameObject>[] experimentalCells)
		{
			var rows = SettingToggleRows.Create();
			rows.AddRange(SettingToggleRows.CreateImmediate());
			int regularCount = rows.Count;
			if (ExperimentalOptions.Enabled)
			{
				rows.AddRange(SettingToggleRows.CreateExperimental());
			}

			for (int i = 0; i < rows.Count; i++)
			{
				(i >= regularCount ? experimentalCells : toggleCells)[(int)rows[i].Domain].Add(CreateSettingToggle(rows[i]));
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

		private static Dictionary<string, object> EfficiencyChoices()
		{
			return new Dictionary<string, object>
			{
				{ "Default", null },
				{ "100%", 1 },
				{ "200%", 2 },
				{ "500%", 5 },
				{ "1000%", 10 },
				{ "2000%", 20 },
				{ "4000%", 40 },
				{ "8000%", 80 }
			};
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
			var efficiencySelectItems = EfficiencyChoices();

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
			PlaceHeader(container, DomainLabel(TrainerDomain.Employee), TrainerDomain.Employee + "_Header", 0);
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

		private static int PageRows(List<GameObject>[] sections, List<GameObject>[] experimentalSections, TrainerDomain[] order)
		{
			return SettingsWindowState.PageRowsWithExperimental(
				SettingsWindowState.PageRows(CountCells(sections, order), SettingsWindowState.Columns),
				SettingsWindowState.PageRows(CountCells(experimentalSections, order), SettingsWindowState.Columns));
		}

		private static void PlaceSections(GameObject container, List<GameObject>[] sections, List<GameObject>[] experimentalSections, TrainerDomain[] order)
		{
			int row = PlaceSections(container, sections, order, 0);
			if (SettingsWindowState.PageRows(CountCells(experimentalSections, order), SettingsWindowState.Columns) > 0)
			{
				PlaceHeader(container, "SectionExperimental".LocDef("Experimental"), "Experimental_Header", row);
				PlaceSections(container, experimentalSections, order, row + 1);
			}
		}

		private static int PlaceSections(GameObject container, List<GameObject>[] sections, TrainerDomain[] order, int row)
		{
			foreach (var domain in order)
			{
				var cells = sections[(int)domain];
				if (cells.Count == 0)
				{
					continue;
				}

				PlaceHeader(container, DomainLabel(domain), domain + "_Header", row);
				row++;

				for (int i = 0; i < cells.Count; i++)
				{
					UIHelper.PlaceInElement(cells[i], container, UIHelper.GetGridSlot(i, SettingsWindowState.Columns, row));
				}

				row += SettingsWindowState.SectionRows(cells.Count, SettingsWindowState.Columns);
			}

			return row;
		}

		private static void PlaceHeader(GameObject container, string text, string name, int row)
		{
			int headerWidth = UIHelper.GetGridColumnX(SettingsWindowState.Columns - 1) + Constants.ELEMENT_WIDTH - Constants.FIRST_COLUMN;
			var header = UIFactory.Label(text, new TextStyle { Alignment = TextAnchor.MiddleCenter, FontStyle = FontStyle.Bold });
			header.name = name;
			UIHelper.PlaceInElement(header.gameObject, container, new Rect(Constants.FIRST_COLUMN, row * Constants.ELEMENT_HEIGHT, headerWidth, Constants.ELEMENT_HEIGHT));
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

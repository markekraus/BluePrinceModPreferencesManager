using UnityEngine;
using UniverseLib.UI;
using UniverseLib.UI.Panels;
using System;
using MelonLoader;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UniverseLib;
using UniverseLib.UI.Models;
using UnityEngine.UI;

namespace BluePrinceModPreferencesManager.UI;

public class UIManager : PanelBase
{
    private static UIBase uiBase;

    public static UIManager Instance { get; private set; }
    public static bool ShowMenu
    {
        get => uiBase != null && uiBase.Enabled;
        set
        {
            if (uiBase == null || !uiBase.RootObject || uiBase.Enabled == value)
                return;
            UniversalUI.SetUIActive(Melon.GUID, value);
            Instance.SetActive(value);
        }
    }

    private static readonly Dictionary<string, CategoryInfo> _categoryInfos = new();
    private static readonly HashSet<CachedPreference> editingEntries = new();
    internal static GameObject CategoryListContent;
    internal static GameObject ConfigEditorContent;
    private static CategoryInfo _currentCategory;
    private static Color _disabledColor = new Color(0.17f, 0.25f, 0.17f);
    private static Color _activeColor = new Color(0, 0.45f, 0.05f);
    private static string currentFilter;
    public static bool ShowHiddenConfigs { get; internal set; }
    public override string Name => $"<b>{Melon.Name}</b> <i>{Melon.Version}</i>";
    public override int MinWidth => 750;
    public override int MinHeight => 750;
    public override Vector2 DefaultAnchorMin => new(0.2f, 0.02f);
    public override Vector2 DefaultAnchorMax => new(0.8f, 0.98f);
    private static ButtonRef saveButton;
    public UIManager(UIBase owner) : base(owner) =>
        Instance = this;
    #region UI Construction    
    protected override void ConstructPanelContent()
    {
        UIFactory.SetLayoutGroup<VerticalLayoutGroup>(ContentRoot, true, false, true, true);
        ConstructTitleBar();
        ConstructSaveButton();
        ConstructToolbar();
        ConstructEditorViewport();
    }
    private void ConstructTitleBar()
    {
        Text titleText = TitleBar.transform.GetChild(0).GetComponent<Text>();
        titleText.text = $"<b><color=#4cd43d>{Melon.Name}</color></b> <i><color=#ff3030>v{Melon.Version}</color></i>";

        Button closeButton = TitleBar.GetComponentInChildren<Button>();
        RuntimeHelper.SetColorBlock(closeButton, new(1, 0.2f, 0.2f), new(1, 0.6f, 0.6f), new(0.3f, 0.1f, 0.1f));

        Text hideText = closeButton.GetComponentInChildren<Text>();
        hideText.color = Color.white;
        hideText.resizeTextForBestFit = true;
        hideText.resizeTextMinSize = 8;
        hideText.resizeTextMaxSize = 14;
    }
    private void ConstructSaveButton()
    {
        saveButton = UIFactory.CreateButton(ContentRoot, "SaveButton", "Save Preferences");
        saveButton.OnClick += SavePreferences;
        UIFactory.SetLayoutElement(saveButton.Component.gameObject, minHeight: 35, flexibleWidth: 9999);
        RuntimeHelper.SetColorBlock(
            saveButton.Component,
            new Color(0.1f, 0.3f, 0.1f),
            new Color(0.2f, 0.5f, 0.2f),
            new Color(0.1f, 0.2f, 0.1f),
            new Color(0.2f, 0.2f, 0.2f));

        saveButton.Component.interactable = false;
    }
    private void ConstructToolbar()
    {
        var toolbarGroup = UIFactory.CreateHorizontalGroup(
            ContentRoot,
            "Toolbar",
            false, false, true, true, 4,
            new Vector4(3, 3, 3, 3),
            new Color(0.1f, 0.1f, 0.1f));

        var toggleObj = UIFactory.CreateToggle(
            toolbarGroup,
            "HiddenConfigsToggle",
            out Toggle toggle,
            out Text toggleText);
        toggle.isOn = false;
        toggle.onValueChanged.AddListener(SetHiddenConfigVisibility);
        toggleText.text = "Show Advanced Settings";
        UIFactory.SetLayoutElement(toggleObj, minWidth: 280, minHeight: 25, flexibleHeight: 0, flexibleWidth: 0);

        var inputField = UIFactory.CreateInputField(toolbarGroup, "FilterInput", "Search...");
        UIFactory.SetLayoutElement(inputField.Component.gameObject, flexibleWidth: 9999, minHeight: 25);
        inputField.OnValueChanged += FilterConfigs;
    }
    private void ConstructEditorViewport()
    {
        var horizontalGroup = UIFactory.CreateHorizontalGroup(
            ContentRoot,
            "Main",
            true, true, true, true, 2, default,
            new Color(0.08f, 0.08f, 0.08f));

        var ctgList = UIFactory.CreateScrollView(
            horizontalGroup,
            "CategoryList",
            out GameObject
            categoryContent,
            out _,
            new Color(0.1f, 0.1f, 0.1f));
        UIFactory.SetLayoutElement(ctgList, minWidth: 300, flexibleWidth: 0);
        CategoryListContent = categoryContent;
        UIFactory.SetLayoutGroup<VerticalLayoutGroup>(categoryContent, spacing: 3);

        var editor = UIFactory.CreateScrollView(
            horizontalGroup,
            "ConfigEditor",
            out GameObject editorContent,
            out _,
            new Color(0.05f, 0.05f, 0.05f));
        UIFactory.SetLayoutElement(editor, flexibleWidth: 9999);
        ConfigEditorContent = editorContent;
    }
    #endregion
    public static void FilterConfigs(string search)
    {
        currentFilter = search.ToLower();
        RefreshFilter();
    }
    public static void SetHiddenConfigVisibility(bool show)
    {
        if (ShowHiddenConfigs == show)
            return;

        ShowHiddenConfigs = show;

        foreach (var entry in _categoryInfos)
        {
            var info = entry.Value;

            if (info.IsCompletelyHidden)
                info.ListButton.Component.gameObject.SetActive(ShowHiddenConfigs);
        }

        if (_currentCategory is not null && !ShowHiddenConfigs && _currentCategory.IsCompletelyHidden)
            UnsetActiveCategory();

        RefreshFilter();
    }
    public static void SavePreferences()
    {
        try { MelonPreferences.Save(); }
        catch (Exception ex)
        {
            LogError("Failed to save Melon preferences");
            LogException(ex);
        }

        foreach (var editingEntry in editingEntries)
            editingEntry.OnSaveOrUndo();

        editingEntries.Clear();
        saveButton.Component.interactable = false;
    }

    internal static void Init()
    {
        uiBase = UniversalUI.RegisterUI(Melon.GUID, null);
        CreateMenu();
        Canvas.ForceUpdateCanvases();
        ShowMenu = false;
        LogMsg("UI initialized.");
    }

    private static void CreateMenu()
    {
        if (Instance != null)
        {
            LogWarning("An instance of BluePrinceModPreferencesManager already exists, cannot create another!");
            return;
        }

        _ = new UIManager(uiBase);

        MelonCoroutines.Start(SetupCategories());
    }

    private static IEnumerator SetupCategories()
    {
        yield return null;
        var categories = MelonPreferences.Categories.OrderBy(it => it.DisplayName);
        foreach (var category in categories)
        {
            if (_categoryInfos.ContainsKey(category.Identifier)) continue;

            var info = new CategoryInfo(category);
            var button = UIFactory.CreateButton(CategoryListContent, "BUTTON_" + category.Identifier, category.DisplayName);
            UIFactory.SetLayoutElement(button.Component.gameObject, flexibleWidth: 9999, minHeight: 30, flexibleHeight: 0);
            RuntimeHelper.SetColorBlock(button.Component, _disabledColor, new Color(0.7f, 1f, 0.7f), new Color(0, 0.25f, 0));
            info.ListButton = button;

            if (category.Entries.All(it => it.IsHidden))
            {
                button.Component.gameObject.SetActive(false);
                info.IsCompletelyHidden = true;
            }

            var content = UIFactory.CreateVerticalGroup(
                ConfigEditorContent,
                "CATEGORY_" + category.Identifier,
                true, false, true, true, 4,
                default, new Color(0.05f, 0.05f, 0.05f));

            foreach (var preference in category.Entries)
            {
                var cache = new CachedPreference(preference, content);
                cache.Enable();

                var obj = cache.UIRoot;

                info.Preferences.Add(new PreferenceInfo()
                {
                    Preference = preference,
                    Content = obj
                });

                if (preference.IsHidden)
                    obj.SetActive(false);
            }
        }
    }

    public static void SetActiveCategory(string categoryIdentifier)
    {
        if (!_categoryInfos.ContainsKey(categoryIdentifier)) return;

        UnsetActiveCategory();
        var info = _categoryInfos[categoryIdentifier];
        _currentCategory = info;
        info.Content.SetActive(true);
        RuntimeHelper.SetColorBlock(info.ListButton.Component, _activeColor);
        RefreshFilter();
    }
    internal static void UnsetActiveCategory()
    {
        if (_currentCategory == null) return;

        RuntimeHelper.SetColorBlock(_currentCategory.ListButton.Component, _disabledColor);
        _currentCategory.Content.SetActive(false);
        _currentCategory = null;
    }
    internal static void RefreshFilter()
    {
        if (_currentCategory == null) return;

        foreach (var entry in _currentCategory.Preferences)
        {
            if (entry.IsHidden && !ShowHiddenConfigs)
            {
                entry.Content.SetActive(false);
                continue;
            }
            var preference = entry.Preference;
            var isFound =
                string.IsNullOrEmpty(currentFilter)
                || preference.DisplayName.ToLower().Contains(currentFilter)
                || (preference.Description?.ToLower().Contains(currentFilter) ?? false);
            entry.Content.SetActive(isFound);
        }
    }

    public static void OnEntryUndo(CachedPreference preference)
    {
        if (editingEntries.Contains(preference))
            editingEntries.Remove(preference);

        if (!editingEntries.Any())
            saveButton.Component.interactable = false;
    }
    public static void OnEntryEdit(CachedPreference preference)
    {
        if (!editingEntries.Contains(preference))
            editingEntries.Add(preference);

        if (!saveButton.Component.interactable)
            saveButton.Component.interactable = true;
    }
    public override void SetDefaultSizeAndPosition()
    {
        base.SetDefaultSizeAndPosition();
        Rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1000);
    }
    protected override void OnClosePanelClicked()
    {
        base.OnClosePanelClicked();
        ShowMenu = false;
    }
}
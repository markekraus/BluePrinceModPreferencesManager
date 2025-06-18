using System;
using BluePrinceModPreferencesManager.UI.InteractiveValue;
using BluePrinceModPreferencesManager.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;
using UniverseLib.Utility;

namespace BluePrinceModPreferencesManager;

public class CachedPreference
{
    public MelonPreferences_Entry Preference { get; }
    public GameObject ParentContent;
    public InteractiveValue InteractiveValue;
    public Type FallbackType => Preference.GetReflectedType();
    public GameObject PreferenceHolder;
    public GameObject ContentGroup;
    public GameObject SubContentGroup;
    public bool UIConstructed;
    internal GameObject UIRoot;
    public GameObject parentContent;
    public RectTransform ContentRect;
    public Text mainLabel;
    internal ButtonRef undoButton;
    internal ButtonRef defaultButton;
    public CachedPreference(MelonPreferences_Entry config, GameObject parent)
    {
        Preference = config;
        ParentContent = parent;
        EnsureConfigValid();
        Preference.OnEntryValueChangedUntyped.Subscribe(UpdateValue);

        CreateValue(config.BoxedValue, FallbackType);
    }
    private void UpdateValue(object oldValue, object newValue) =>
        UpdateValue();
    private void UpdateValue()
    {
        EnsureConfigValid();
        InteractiveValue.Value = Preference.BoxedEditedValue;
        InteractiveValue.OnValueUpdated();
        InteractiveValue.RefreshSubContentState();
    }
    private void EnsureConfigValid()
    {
        if (Preference.BoxedValue is not null) return;

        // MelonLoader does not support null config values.
        if (FallbackType == typeof(string))
            Preference.BoxedValue = "";
        else if (FallbackType.IsArray)
            Preference.BoxedValue = Array.CreateInstance(FallbackType.GetElementType(), 0);
        else
            Preference.BoxedValue = Activator.CreateInstance(FallbackType);

        Preference.BoxedEditedValue = Preference.BoxedValue;
    }
    public void CreateValue(object value, Type fallbackType)
    {
        InteractiveValue = InteractiveValue.Create(value, fallbackType);
        InteractiveValue.Owner = this;
        InteractiveValue.mainContentParent = ContentGroup;
        InteractiveValue.subContentParent = SubContentGroup;
    }

    public void Enable()
    {
        if (!UIConstructed)
        {
            ConstructUI();
            UpdateValue();
        }

        UIRoot.SetActive(true);
        UIRoot.transform.SetAsLastSibling();
    }
    public void Disable()
    {
        if (UIRoot)
            UIRoot.SetActive(false);
    }
    public void Destroy()
    {
        if (UIRoot)
            GameObject.Destroy(UIRoot);
    }
    internal void ConstructUI()
    {
        UIConstructed = true;
        CreateUIRoot();
        CreateContentGroup();
        CreatePreferenceHolder();
        CreateMainLabel();
        CreateUndoButton();
        CreateDefaultButton();
        CreateDescriptionLabel();
        CreateSunContentGroup();

        if (InteractiveValue is not null)
        {
            InteractiveValue.mainContentParent = ContentGroup;
            InteractiveValue.subContentParent = SubContentGroup;
        }
    }

    private void CreateSunContentGroup()
    {
        SubContentGroup = UIFactory.CreateVerticalGroup(
                    ContentGroup,
                    "CacheObjectBase.SubContent",
                    true, false, true, true, 0, default,
                    new Color(1, 1, 1, 0));
        UIFactory.SetLayoutElement(SubContentGroup, minHeight: 30, flexibleHeight: 9999, minWidth: 125, flexibleWidth: 9000);
        SubContentGroup.SetActive(false);
    }

    private void CreateDescriptionLabel()
    {
        if (Preference.Description is null) return;

        var desc = UIFactory.CreateLabel(
            ContentGroup,
            "Description",
            $"<i>{Preference.Description}</i>",
            TextAnchor.MiddleLeft,
            Color.grey);
        UIFactory.SetLayoutElement(desc.gameObject, minWidth: 250, minHeight: 18, flexibleWidth: 9999, flexibleHeight: 0);
    }

    private void CreateDefaultButton()
    {
        defaultButton = UIFactory.CreateButton(
                    PreferenceHolder,
                    "DefaultButton",
                    "Default",
                    new Color(0.3f, 0.3f, 0.3f));
        defaultButton.OnClick += RevertToDefault;
        UIFactory.SetLayoutElement(defaultButton.Component.gameObject, minWidth: 80, minHeight: 22, flexibleWidth: 0);
    }

    private void CreateUndoButton()
    {
        undoButton = UIFactory.CreateButton(
                    PreferenceHolder,
                    "UndoButton",
                    "Undo",
                    new Color(0.3f, 0.3f, 0.3f));
        undoButton.OnClick += UndoEdits;
        undoButton.Component.gameObject.SetActive(false);
        UIFactory.SetLayoutElement(undoButton.Component.gameObject, minWidth: 80, minHeight: 22, flexibleWidth: 0);
    }

    private void CreateMainLabel()
    {
        mainLabel = UIFactory.CreateLabel(
                    PreferenceHolder,
                    "ConfigLabel",
                    Preference.DisplayName,
                    TextAnchor.MiddleLeft,
                    new Color(0.7f, 1, 0.7f));
        mainLabel.text += $" <i>({SignatureHighlighter.Parse(Preference.GetReflectedType(), false)})</i>";
        UIFactory.SetLayoutElement(mainLabel.gameObject, minWidth: 200, minHeight: 22, flexibleWidth: 9999, flexibleHeight: 0);
    }

    private void CreatePreferenceHolder()
    {
        PreferenceHolder = UIFactory.CreateHorizontalGroup(
                    ContentGroup,
                    "PreferenceHolder",
                    false, false, true, true, 5, default,
                    new Color(1, 1, 1, 0), TextAnchor.MiddleLeft);
        UIFactory.SetLayoutElement(PreferenceHolder, minHeight: 30, flexibleHeight: 0);
    }

    private void CreateContentGroup()
    {
        ContentGroup = UIFactory.CreateVerticalGroup(
                    UIRoot, "ConfigHolder",
                    false, false, true, true, 5,
                    new Vector4(2, 2, 5, 5),
                    new Color(0.12f, 0.12f, 0.12f));
    }

    private void CreateUIRoot()
    {
        UIRoot = UIFactory.CreateVerticalGroup(
                    parentContent,
                    "Preference_" + Preference.Identifier,
                    true, false, true, true, 0,
                    default, new Color(1, 1, 1, 0));
        ContentRect = UIRoot.GetComponent<RectTransform>();
        ContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);
        UIFactory.SetLayoutElement(UIRoot, minHeight: 25, flexibleHeight: 9999, minWidth: 100, flexibleWidth: 5000);
    }

    public void UndoEdits()
    {
        Preference.BoxedEditedValue = Preference.BoxedValue;
        InteractiveValue.Value = Preference.BoxedValue;
        InteractiveValue.OnValueUpdated();

        OnSaveOrUndo();
    }
    internal void OnSaveOrUndo()
    {
        undoButton.Component.gameObject.SetActive(false);
        UIManager.OnEntryUndo(this);
    }

    public void RevertToDefault()
    {
        Preference.ResetToDefault();
        Preference.BoxedEditedValue = Preference.BoxedValue;
        UpdateValue();
        OnSaveOrUndo();
    }
    public void SetPreferenceValueFromInteractiveValue()
    {
        if (Preference.Validator is not null)
            InteractiveValue.Value = Preference.Validator.EnsureValid(InteractiveValue.Value);

        var edited = Preference.BoxedEditedValue;
        if ((edited is null && InteractiveValue.Value is null) || (edited is not null && edited.Equals(InteractiveValue.Value)))
            return;

        Preference.BoxedEditedValue = InteractiveValue.Value;
        UIManager.OnEntryEdit(this);
        undoButton.Component.gameObject.SetActive(true);
    }
}

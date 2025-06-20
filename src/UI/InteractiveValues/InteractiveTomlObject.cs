using System;
using Tomlet;
using Tomlet.Models;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

public class InteractiveTomlObject : InteractiveValue
{
    public TomlValue TomlValue;
    internal InputFieldRef valueInput;
    internal Text placeholderText;
    internal GameObject hiddenObject;
    internal Text hiddenText;
    public InteractiveTomlObject(object value, Type valueType) :
        base(value, valueType) { }
    public override bool SupportsType(Type type) => true;
    internal override void OnValueUpdated()
    {
        base.OnValueUpdated();

        try
        {
            TomlValue = TomletMain.ValueFrom(Value.GetActualType(), Value);
        }
        catch (Exception ex)
        {
            LogError("Failed to convert value to TomlValue.");
            LogException(ex);
            return;
        }

        try
        {
            valueInput.Text = TomlValue.SerializedValue;
            placeholderText.text = valueInput.Text;
        }
        catch
        {
            RescueTomlValue();
        }
    }
    private void RescueTomlValue()
    {
        LogWarning("Attempting to rescue TomlObject...");

        try
        {
            var tomlType = TomlValue.GetType();
            if (tomlType.Name == "TomlArray" && TomlValue is TomlArray tomlArray)
                valueInput.Text = tomlArray.SerializeTableArray(Owner.Preference.DisplayName);
            else if (TomlValue is TomlTable tomlTable)
                valueInput.Text = tomlTable.SerializeNonInlineTable(null, false);
            else
                throw new NotSupportedException($"Unsupported TOML Value Type '{tomlType.FullName}'");
            placeholderText.text = valueInput.Text;
            LogMsg("Rescue Complete.");
        }
        catch (Exception ex)
        {
            LogError($"Unable to edit config '{Owner.Preference.DisplayName}' due to an error with the Mapper!");
            LogException(ex);
        }
    }
    internal void SetValueFromInput()
    {
        try
        {
            Value = TomletMain.To(Value.GetActualType(), TomlValue);
            Owner.SetPreferenceValueFromInteractiveValue();
            valueInput.Component.textComponent.color = Color.white;
        }
        catch
        {
            valueInput.Component.textComponent.color = Color.red;
        }
    }
    public override void RefreshUIForValue()
    {
        if (!hiddenObject.gameObject.activeSelf)
            hiddenObject.gameObject.SetActive(true);
    }
    #region UI Construction
    public override void ConstructUI(GameObject parent)
    {
        base.ConstructUI(parent);
        CreateHiddenObject();
        ConfigureHiddenText();
        ConfigureHiddenFitter();
        CreateValueInput();
        ConfigurePlaceholderText();
        OnValueUpdated();
        valueInput.OnValueChanged += OnValueUpdated;
    }
    private void CreateHiddenObject()
    {
        hiddenObject = UIFactory.CreateLabel(
            mainContent,
            "HiddenLabel", "",
            TextAnchor.MiddleLeft).gameObject;
        hiddenObject.SetActive(false);
        UIFactory.SetLayoutElement(hiddenObject, minHeight: 25, flexibleHeight: 500, minWidth: 250, flexibleWidth: 9000);
        UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(hiddenObject, true, true, true, true);
    }
    private void ConfigureHiddenText()
    {
        hiddenText = hiddenObject.GetComponent<Text>();
        hiddenText.color = Color.clear;
        hiddenText.fontSize = 14;
        hiddenText.raycastTarget = false;
        hiddenText.supportRichText = false;
    }
    private void ConfigureHiddenFitter()
    {
        var hiddenFitter = hiddenObject.AddComponent<ContentSizeFitter>();
        hiddenFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    private void CreateValueInput()
    {
        valueInput = UIFactory.CreateInputField(hiddenObject, "StringInputField", "...");
        UIFactory.SetLayoutElement(valueInput.Component.gameObject, minWidth: 120, minHeight: 25, flexibleWidth: 5000, flexibleHeight: 5000);
        valueInput.Component.lineType = InputField.LineType.MultiLineNewline;
        valueInput.Component.textComponent.supportRichText = false;
    }
    private void ConfigurePlaceholderText()
    {
        placeholderText = valueInput.Component.placeholder.GetComponent<Text>();
        placeholderText.supportRichText = false;
    }
    #endregion
    private void OnValueUpdated(string value)
    {
        hiddenText.text = value ?? "";
        LayoutRebuilder.ForceRebuildLayoutImmediate(Owner.ContentRect);
        SetValueFromInput();
    }
}
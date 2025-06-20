using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

public enum ColorProperty { R, G, B, A }

public class InteractiveColorProperty
{
    public readonly ColorProperty Property;
    public string FieldName => Property.ToString();
    public readonly InteractiveColor Owner;
    private readonly Slider Slider;
    private readonly InputFieldRef inputFieldObject;
    private InputField InputField => inputFieldObject.Component;
    private string InputText
    {
        get => InputField.text;
        set => InputField.SetTextWithoutNotify(value);
    }
    private float SliderValue
    {
        get => Slider.value;
        set => Slider.SetValueWithoutNotify(value);
    }
    private Color ColorImageValue
    {
        get => Owner.ColorImageValue;
        set => Owner.ColorImageValue = value;
    }
    private readonly GameObject row;
    private readonly Text label;
    private readonly GameObject sliderObject;
    public InteractiveColorProperty(ColorProperty Property, InteractiveColor Owner)
    {
        this.Property = Property;
        this.Owner = Owner;
        row = CreateRow();
        label = CreateRowLabel();
        inputFieldObject = CreateInputField();
        (sliderObject, Slider) = CreateSlider();
    }
    #region UI Construction
    private GameObject CreateRow() =>
        UIFactory.CreateHorizontalGroup(
            Owner.grid,
            $"EditorRow_{FieldName}",
            false, true, true, true, 5, default,
            new Color(1, 1, 1, 0));
    private Text CreateRowLabel()
    {
        var label = UIFactory.CreateLabel(
            row,
            "RowLabel",
            $"{FieldName}:",
            TextAnchor.MiddleRight,
            Color.cyan);
        UIFactory.SetLayoutElement(label.gameObject, minWidth: 17, flexibleWidth: 0, minHeight: 25);
        return label;
    }
    private InputFieldRef CreateInputField()
    {
        var inputFieldObject = UIFactory.CreateInputField(row, "InputField", "...");
        UIFactory.SetLayoutElement(inputFieldObject.Component.gameObject, minWidth: 40, minHeight: 25, flexibleWidth: 0);
        var inputField = inputFieldObject.Component;
        inputField.characterValidation =
            Owner.Value is Color32
                ? InputField.CharacterValidation.Integer
                : InputField.CharacterValidation.Decimal;
        inputField.onValueChanged.AddListener(InputFieldValueChanged);
        return inputFieldObject;
    }
    private (GameObject, Slider) CreateSlider()
    {
        var sliderObject = UIFactory.CreateSlider(row, "Slider", out Slider slider);
        UIFactory.SetLayoutElement(sliderObject, minHeight: 25, minWidth: 70, flexibleWidth: 999, flexibleHeight: 0);
        slider.minValue = 0;
        (slider.maxValue, slider.value) =
            Owner.Value switch {
                Color32 color => (255, GetValueFromColor(ref color)),
                Color color => (1, GetValueFromColor(ref color)),
                _ => throw new NotImplementedException()};
        slider.onValueChanged.AddListener(SliderValueChanged);
        return (sliderObject, slider);
    }
    #endregion
    #region Factory
    public static List<InteractiveColorProperty> CreateAll(InteractiveColor Owner) =>
        new() {
            new(ColorProperty.R, Owner),
            new(ColorProperty.G, Owner),
            new(ColorProperty.B, Owner),
            new(ColorProperty.A, Owner)};
    #endregion
    #region Callbacks
    private void SliderValueChanged(float value)
    {
        try {
            switch (Owner.Value) {
                case Color32 color: SetValueToColor((byte)value, ref color); break;
                case Color color: SetValueToColor(value, ref color); break;}}
        catch (Exception ex) {
            LogError($"Failed to set color value '{value}' on Color.{Property} for property '{Owner.Owner.Preference.Category.Identifier}.{Owner.Owner.Preference.Identifier}'");
            LogException(ex);}
    }
    private void InputFieldValueChanged(string value)
    {
        switch (Owner.Value) {
            case Color32 color: SetValueToColor(byte.Parse(value), ref color); break;
            case Color color: SetValueToColor(float.Parse(value), ref color); break;}
    }
    private void SetValueToColor(float value, ref Color color)
    {
        switch (Property) {
            case ColorProperty.R: color.r = value; break;
            case ColorProperty.G: color.g = value; break;
            case ColorProperty.B: color.b = value; break;
            case ColorProperty.A: color.a = value; break;}
        Owner.Value = color;
        SliderValue = value;
        InputText = $"{value}";
        ColorImageValue = color;
        Owner.Owner.SetPreferenceValueFromInteractiveValue();
    }
    void SetValueToColor(byte value, ref Color32 color)
    {
        switch (Property) {
            case ColorProperty.R: color.r = value; break;
            case ColorProperty.G: color.g = value; break;
            case ColorProperty.B: color.b = value; break;
            case ColorProperty.A: color.a = value; break;}
        Owner.Value = color;
        SliderValue = value;
        InputText = $"{value}";
        ColorImageValue = color;
        Owner.Owner.SetPreferenceValueFromInteractiveValue();
    }
    private float GetValueFromColor(ref Color color) =>
        Property switch {
            ColorProperty.R => color.r,
            ColorProperty.G => color.g,
            ColorProperty.B => color.b,
            ColorProperty.A => color.a,
            _ => throw new System.NotImplementedException()};
    private float GetValueFromColor(ref Color32 color) =>
        Property switch {
            ColorProperty.R => color.r,
            ColorProperty.G => color.g,
            ColorProperty.B => color.b,
            ColorProperty.A => color.a,
            _ => throw new System.NotImplementedException()};
    internal void RefreshColorUI() =>
        (InputText, SliderValue) = Owner.Value switch {
            Color32 color => RefreshColorUI(ref color),
            Color color => RefreshColorUI(ref color),
            _ => throw new NotImplementedException()};
    private (string, float) RefreshColorUI(ref Color color) =>
        Property switch {
            ColorProperty.R => (color.r.ToString(), color.r),
            ColorProperty.G => (color.g.ToString(), color.g),
            ColorProperty.B => (color.b.ToString(), color.b),
            ColorProperty.A => (color.a.ToString(), color.a),
            _ => throw new NotImplementedException()};
    private (string, float) RefreshColorUI(ref Color32 color) =>
        Property switch {
            ColorProperty.R => (color.r.ToString(), color.r),
            ColorProperty.G => (color.g.ToString(), color.g),
            ColorProperty.B => (color.b.ToString(), color.b),
            ColorProperty.A => (color.a.ToString(), color.a),
            _ => throw new NotImplementedException()};
    #endregion        
}
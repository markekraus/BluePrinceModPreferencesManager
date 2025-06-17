using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace BluePrinceModPreferencesManager.UI;

public enum ColorProperty { R, G, B, A }

public class InteractiveColorProperty
{
    public readonly ColorProperty Property;
    public string FieldName => Property.ToString();
    public readonly InteractiveColor Owner;
    private Slider Slider;
    private InputFieldRef inputFieldObject;
    private InputField InputField;
    private GameObject row;
    private Text label;
    private GameObject sliderObject;
    public InteractiveColorProperty(ColorProperty Property, InteractiveColor Owner)
    {
        this.Property = Property;
        this.Owner = Owner;
        CreateRow();
        CreateRowLabel();
        CreateInputField();
        CreateSlider();
    }
    #region UI Construction
    private void CreateRow()
    {
        row = UIFactory.CreateHorizontalGroup(
            Owner.grid,
            $"EditorRow_{FieldName}",
            false, true, true, true, 5, default,
            new Color(1, 1, 1, 0));
    }
    private void CreateRowLabel()
    {
        label = UIFactory.CreateLabel(
            row,
            "RowLabel",
            $"{FieldName}:",
            TextAnchor.MiddleRight,
            Color.cyan);
        UIFactory.SetLayoutElement(label.gameObject, minWidth: 17, flexibleWidth: 0, minHeight: 25);
    }
    private void CreateInputField()
    {
        inputFieldObject = UIFactory.CreateInputField(row, "InputField", "...");
        UIFactory.SetLayoutElement(inputFieldObject.Component.gameObject, minWidth: 40, minHeight: 25, flexibleWidth: 0);
        InputField = inputFieldObject.Component;
        InputField.characterValidation =
            Owner.Value is Color
                ? InputField.CharacterValidation.Decimal
                : InputField.CharacterValidation.Integer;
        InputField.onValueChanged.AddListener(InputFieldValueChanged);
    }
    private void CreateSlider()
    {
        sliderObject = UIFactory.CreateSlider(row, "Slider", out Slider Slider);
        UIFactory.SetLayoutElement(sliderObject, minHeight: 25, minWidth: 70, flexibleWidth: 999, flexibleHeight: 0);
        Slider.minValue = 0;
        (Slider.maxValue, Slider.value) =
            Owner.Value switch {
                Color color => (1, GetValueFromColor(ref color)),
                Color32 color => (255, GetValueFromColor(ref color)),
                _ => throw new NotImplementedException()};
        Slider.onValueChanged.AddListener(SliderValueChanged);
    }
    private void SliderValueChanged(float value)
    {
        try {
            switch (Owner.Value) {
                case Color color: SetValueToColor(value, ref color); break;
                case Color32 color: SetValueToColor((byte)value, ref color); break;}}
        catch (Exception ex) {
            LogError($"Failed to set color value '{value}' on Color.{Property} for property '{Owner.Owner.Preference.Category.Identifier}.{Owner.Owner.Preference.Identifier}'");
            LogException(ex);}
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
    private void InputFieldValueChanged(string value)
    {
        switch (Owner.Value) {
            case Color color: SetValueToColor(float.Parse(value), ref color); break;
            case Color32 color: SetValueToColor(byte.Parse(value), ref color); break;}
    }
    private void SetValueToColor(float value, ref Color color)
    {
        switch (Property) {
            case ColorProperty.R: color.r = value; break;
            case ColorProperty.G: color.g = value; break;
            case ColorProperty.B: color.b = value; break;
            case ColorProperty.A: color.a = value; break;}
        Owner.Value = color;
        Slider.value = value;
        InputField.text = $"{value}";
        Owner.colorImage.color = color;
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
        Slider.value = value;
        InputField.text = $"{value}";
        Owner.colorImage.color = color;
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
        InputField.text = Owner.Value switch {
            Color color => RefreshColorUI(ref color),
            Color32 color => RefreshColorUI(ref color),
            _ => throw new NotImplementedException()};
    private string RefreshColorUI(ref Color color) =>
        Property switch {
            ColorProperty.R => color.r.ToString(),
            ColorProperty.G => color.g.ToString(),
            ColorProperty.B => color.b.ToString(),
            ColorProperty.A => color.a.ToString(),
            _ => throw new NotImplementedException()};
    private string RefreshColorUI(ref Color32 color) =>
        Property switch {
            ColorProperty.R => color.r.ToString(),
            ColorProperty.G => color.g.ToString(),
            ColorProperty.B => color.b.ToString(),
            ColorProperty.A => color.a.ToString(),
            _ => throw new NotImplementedException()};
    #endregion        
}
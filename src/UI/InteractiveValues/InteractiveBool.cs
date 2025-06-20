using System;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

public class InteractiveBool : InteractiveValue
{
    internal Toggle toggle;
    private Text label;
    private GameObject toggleObject;
    public InteractiveBool(object value, Type valueType) : base(value, valueType) { }
    public override bool SupportsType(Type type) =>
        type == typeof(bool);
    public override void ConstructUI(GameObject parent)
    {
        base.ConstructUI(parent);
        CreateToggle();
        CreateLabel();
        RefreshUIForValue();
    }
    private void CreateToggle()
    {
        toggleObject = UIFactory.CreateToggle(
            mainContent,
            "InteractiveBoolToggle",
            out toggle,
            out _,
            new Color(0.1f, 0.1f, 0.1f));
        UIFactory.SetLayoutElement(toggleObject, minWidth: 24);
        toggle.onValueChanged.AddListener(ToggleValueChanged);
    }
    private void CreateLabel()
    {
        label = UIFactory.CreateLabel(mainContent, "TrueFalseLabel", "False", TextAnchor.MiddleLeft);
        UIFactory.SetLayoutElement(label.gameObject, minWidth: 60, minHeight: 25);
    }
    private void ToggleValueChanged(bool value)
    {
        Value = value;
        RefreshUIForValue();
        Owner.SetPreferenceValueFromInteractiveValue();
    }
    public override void RefreshUIForValue()
    {
        if (Value is not bool value) return;

        toggle.gameObject.SetActive(true);
        toggle.SetIsOnWithoutNotify(value);
        var color = value
            ? "6bc981"  // on
            : "c96b6b"; // off
        label.text = $"<color=#{color}>{value}</color>";
    }
}
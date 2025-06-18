using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib;
using UniverseLib.UI;

namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

internal record EnumMap(long Id, string Name);

internal class InteractiveEnum : InteractiveValue
{
    private static Dictionary<Type, List<EnumMap>> enumCache = new();
    private Type enumType;
    private Dictionary<string, Dropdown.OptionData> dropdownOptions = new();
    private GameObject dropdownObject;
    private Dropdown dropdown;
    private List<EnumMap> enumValues =>
        enumCache.TryGetValue(enumType, out List<EnumMap> value)
            ? value
            : null;
    public InteractiveEnum(object value, Type valueType) : base(value, valueType) =>
        GetNames();
    public override bool SupportsType(Type type) => type.IsEnum;
    private void GetNames()
    {
        enumType = Value?.GetType() ?? FallbackType;
        if (Value is null || enumCache.ContainsKey(enumType)) return;

        var names = new HashSet<string>();
        var values = new List<EnumMap>();

        // GetNames will not work for all enum types. Using GetValues instead.
        foreach (var value in Enum.GetValues(enumType))
        {
            var name = value.ToString();
            if (!names.Add(name)) continue;

            var backingType = Enum.GetUnderlyingType(enumType);
            long id;
            try
            {
                var unbox = Convert.ChangeType(value, backingType);
                id = (long)Convert.ChangeType(unbox, typeof(long));
            }
            catch (Exception ex)
            {
                LogError($"Could not Unbox underlying type {backingType.Name} from {enumType.FullName}");
                LogException(ex);
                continue;
            }
            values.Add(new(id, name));
        }
        enumCache[enumType] = values;
    }
    internal override void OnValueUpdated()
    {
        GetNames();
        base.OnValueUpdated();
    }
    public override void RefreshUIForValue()
    {
        base.RefreshUIForValue();

        if (this.GetType() != typeof(InteractiveEnum)) return;

        var option = Value.ToString();
        if (dropdownOptions.ContainsKey(option))
            dropdown.value = dropdown.options.IndexOf(dropdownOptions[option]);
    }
    public override void ConstructUI(GameObject parent)
    {
        base.ConstructUI(parent);

        if (this.GetType() != typeof(InteractiveEnum)) return;
        CreateDropdown();
        CreateDropdownOptions();
    }
    private void CreateDropdown()
    {
        dropdownObject = UIFactory.CreateDropdown(
            mainContent,
            "InteractiveDropdown",
            out dropdown,
            "", 14, null);
        UIFactory.SetLayoutElement(dropdownObject, minWidth: 400, minHeight: 25);
        dropdown.onValueChanged.AddListener(SetValueFromDropdown);
    }
    private void CreateDropdownOptions()
    {
        foreach (var enumValue in enumValues)
        {
            var opt = new Dropdown.OptionData(enumValue.Name);
            dropdown.options.Add(opt);
            dropdownOptions.Add(enumValue.Name, opt);
        }
    }
    private void SetValueFromDropdown(int _)
    {
        var index = dropdown.value;
        if (!Enum.TryParse(enumType, enumValues[index].Name, out object value)) return;
        Value = value;
        Owner.SetPreferenceValueFromInteractiveValue();
        RefreshUIForValue();
    }
}
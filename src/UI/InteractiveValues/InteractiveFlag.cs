using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib;

namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

internal record ToggleMap(EnumMap EnumMap, Toggle Toggle)
{
    public bool IsOn
    {
        get => Toggle.isOn;
        set => Toggle.SetIsOnWithoutNotify(value);
    }
    public string Name => EnumMap.Name;
    public long Id => EnumMap.Id;
    public bool isNoneFlag => Id == InteractiveFlag.noneFlag;
    public bool isAllFlag => Id == InteractiveFlag.allFlag;
}

internal class InteractiveFlag : InteractiveEnum
{
    public override bool HasSubContent => true;
    public override bool SubContentWanted => true;
    private GameObject groupObject;
    private readonly List<ToggleMap> toggleMaps;
    private object _valueChangeLock;
    private object _refreshUiLock;
    internal const long allFlag = -1;
    internal const long noneFlag = 0;
    public InteractiveFlag(object value, Type valueType) : base(value, valueType) =>
        toggleMaps = new();
    public override void ConstructUI(GameObject parent) => base.ConstructUI(parent);
    public override bool SupportsType(Type type)
            => type.IsEnum && type.GetCustomAttributes(true).Any(it => it is FlagsAttribute);
    public override void ConstructSubContent()
    {
        subContentConstructed = true;
        CreateGroupObject();
        CreateToggles();
    }
    private void CreateGroupObject()
    {
        groupObject = UIFactory.CreateVerticalGroup(
            subContentParent,
            "InteractiveFlagsContent",
            false, true, true, true, 5,
            new Vector4(3, 3, 3, 3),
            new Color(1, 1, 1, 0));
    }
    private void CreateToggles()
    {
        foreach (var value in enumValues)
        {
            var toggleObj = UIFactory.CreateToggle(
                groupObject,
                "FlagToggle",
                out Toggle toggle,
                out Text text,
                new Color(0.1f, 0.1f, 0.1f));
            UIFactory.SetLayoutElement(toggleObj, minWidth: 100, flexibleWidth: 2000, minHeight: 25);
            text.text = value.Name;
            var toggleMap = new ToggleMap(value, toggle);
            toggleMaps.Add(toggleMap);
            toggle.onValueChanged.AddListener(val => ValueChanged(val, toggleMap));
        }
    }
    private void ValueChanged(bool _, ToggleMap toggleMap)
    {
        SetValueFromToggles(toggleMap);
        RefreshUIForValue();
    }
    private void SetValueFromToggles(ToggleMap toggleMap)
    {
        lock (_valueChangeLock)
        {
            long flagBits =
                toggleMap.isNoneFlag || toggleMap.isAllFlag
                ? toggleMap.Id
                : toggleMaps.Select(t => t.IsOn ? t.Id : 0).Sum();
            Value = Enum.ToObject(enumType, flagBits);
        }
        Owner.SetPreferenceValueFromInteractiveValue();
    }
    public override void RefreshUIForValue()
    {
        base.RefreshUIForValue();
        if (!subContentConstructed) return;
        long enumValue = Convert.ToInt64(Value);
        lock (_refreshUiLock)
            foreach (var toggleMap in toggleMaps)
                toggleMap.IsOn = IsOn(enumValue, toggleMap.Id);
    }
    private bool IsOn(long enumValue, long flag) =>
        (enumValue == noneFlag && flag == noneFlag) ||
        (enumValue == allFlag && flag == allFlag) ||
        ((flag != allFlag) && (enumValue & flag) != 0);
    internal override void OnValueUpdated() =>
        base.OnValueUpdated();
    protected internal override void OnToggleSubContent(bool toggle)
    {
        base.OnToggleSubContent(toggle);
        RefreshUIForValue();
    }
}
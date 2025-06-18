using System;
using System.Collections.Generic;
using UnityEngine;
using UniverseLib;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace BluePrinceModPreferencesManager.U.InteractiveValue;

public abstract class InteractiveValue
{
    private static readonly HashSet<Type> customValueTypes = new();
    private static readonly List<InteractiveValue> customValueInstances = new();
    public object Value;
    public bool UIConstructed;
    protected internal GameObject mainContentParent;
    protected internal GameObject mainContent;
    public virtual bool HasSubContent => false;
    public virtual bool SubContentWanted => false;
    protected internal ButtonRef subExpandButton;
    protected internal GameObject subContentParent;
    protected internal bool subContentConstructed;
    public CachedPreference Owner;
    public readonly Type FallbackType;
    public InteractiveValue(object value, Type valueType)
    {
        Value = value;
        FallbackType = valueType;
    }

    internal virtual void OnValueUpdated()
    {
        if (!UIConstructed)
            ConstructUI(mainContentParent);

        RefreshUIForValue();
    }

    public virtual void ConstructUI(GameObject parent)
    {
        UIConstructed = true;

        mainContent = UIFactory.CreateHorizontalGroup(
            parent, $"InteractiveValue_{this.GetType().Name}",
            false, false, true, true, 4, default,
            new Color(1, 1, 1, 0), TextAnchor.UpperLeft);

        mainContent
            .GetComponent<RectTransform>()
            .SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 25);

        UIFactory.SetLayoutElement(mainContent, flexibleWidth: 9000, minWidth: 175, minHeight: 25, flexibleHeight: 0);

        // sub-content expand button
        if (HasSubContent)
        {
            subExpandButton = UIFactory.CreateButton(mainContent, "ExpandSubContentButton", "▲ Expand to edit", new Color(0.3f, 0.3f, 0.3f));
            subExpandButton.OnClick += ToggleSubContent;
            UIFactory.SetLayoutElement(subExpandButton.Component.gameObject, minHeight: 25, minWidth: 120, flexibleWidth: 0, flexibleHeight: 0);
        }
    }
    public void ToggleSubContent()
    {
        if (!subContentParent.activeSelf)
        {
            subContentParent.SetActive(true);
            subContentParent.transform.SetAsLastSibling();
            subExpandButton.ButtonText.text = "▼ Click to hide";
        }
        else
        {
            subContentParent.SetActive(false);
            subExpandButton.ButtonText.text = "▲ Expand to edit";
        }

        OnToggleSubContent(subContentParent.activeSelf);

        RefreshSubContentState();
    }
    protected internal virtual void OnToggleSubContent(bool toggle)
    {
        if (!subContentConstructed)
            ConstructSubContent();
    }
    public virtual void ConstructSubContent()
    {
        subContentConstructed = true;
    }

    public void RefreshSubContentState()
    {
        if (!HasSubContent) return;

        if (subExpandButton.Component.gameObject.activeSelf != SubContentWanted)
            subExpandButton.Component.gameObject.SetActive(SubContentWanted);

        if (!SubContentWanted && subContentParent.activeSelf)
            ToggleSubContent();
    }
    public virtual void RefreshUIForValue() { }

    internal static InteractiveValue Create(object value, Type fallbackType)
    {
        var type = value.GetActualType() ?? fallbackType;
        var valueType = GetValueForType(type);

        return (InteractiveValue)Activator.CreateInstance(valueType, new object[] { value, type });
    }

    private static Type GetValueForType(Type type)
    {
        // // Boolean
        // if (type == typeof(bool))
        //     return typeof(InteractiveBool);
        // // Number
        // else if (type.IsPrimitive || typeof(decimal).IsAssignableFrom(type))
        //     return typeof(InteractiveNumber);
        // // String
        // else if (type == typeof(string))
        //     return typeof(InteractiveString);
        // // KeyCode
        // else if (type == typeof(KeyCode) || type.FullName == "UnityEngine.InputSystem.Key")
        //     return typeof(InteractiveKeycode);
        // // Flags and Enum
        // else if (typeof(Enum).IsAssignableFrom(type))
        //     if (type.GetCustomAttributes(typeof(FlagsAttribute), true) is object[] fa && fa.Any())
        //         return typeof(InteractiveFlags);
        //     else
        //         return typeof(InteractiveEnum);
        // // Color
        // else if (type == typeof(Color) || type == typeof(Color32))
        //     return typeof(InteractiveColor);
        // // Vector / Rect
        // else if (InteractiveFloatStruct.IsTypeSupported(type))
        //     return typeof(InteractiveFloatStruct);
        // // TODO: This needs reworked with the registration system
        // // Custom defined handlers
        // else if (customIValueInstances.FirstOrDefault(it => it.SupportsType(type)) is InteractiveValue custom)
        //     return custom.GetType();
        // // fallback to default handler
        // else
        //     return typeof(InteractiveTomlObject);
        return typeof(InteractiveTomlObject);
    }
    public abstract bool SupportsType(Type type);
    // TODO: This registration system should be reworked.
    // Creating an instance just to run a method should not be needed
    // reduce to 1 hashset<Type>
    public static void RegisterIValueType<T>() where T : InteractiveValue
    {
        if (customValueTypes.Contains(typeof(T)))
            return;

        customValueInstances.Add((T)Activator.CreateInstance(typeof(T), new object[] { null, typeof(object) }));
        customValueTypes.Add(typeof(T));
    }
    public virtual void DestroySubContent()
    {
        if (subContentParent && HasSubContent)
        {
            for (int i = 0; i < subContentParent.transform.childCount; i++)
            {
                var child = subContentParent.transform.GetChild(i);
                if (child)
                    GameObject.Destroy(child.gameObject);
            }
        }

        subContentConstructed = false;
    }
}
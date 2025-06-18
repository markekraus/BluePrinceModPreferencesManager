using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;


namespace BluePrinceModPreferencesManager.UI.InteractiveValue;

public class InteractiveColor : InteractiveValue
{
    internal Image colorImage;
    private List<InteractiveColorProperty> colorProperties;
    internal GameObject baseHorizontalGroup;
    internal GameObject imageHolder;
    internal GameObject imageObject;
    internal GameObject editorGroup;
    internal GameObject grid;
    public InteractiveColor(object value, Type valueType) : base(value, valueType) { }
    public override bool SupportsType(Type type) =>
         type == typeof(Color) || type == typeof(Color32);
    #region UI Construction
    public override void ConstructUI(GameObject parent)
    {
        base.ConstructUI(parent);
        CreateBaseHorizontalGroup();
        CreateImageHolder();
        CreateImageObject();
        CreateEditorGroup();
        CreateEditorGrid();
        CreateColorPropertyInputs();
        RefreshUIForValue();
    }
    private void CreateBaseHorizontalGroup()
    {
        baseHorizontalGroup = UIFactory.CreateHorizontalGroup(
                        mainContent,
                        "ColorEditor",
                        false, false, true, true, 5, default,
                        new Color(1, 1, 1, 0), TextAnchor.MiddleLeft);
    }
    private void CreateImageHolder()
    {
        imageHolder = UIFactory.CreateVerticalGroup(
                    baseHorizontalGroup,
                    "ImgHolder",
                    true, true, true, true, 0,
                    new Vector4(1, 1, 1, 1),
                    new Color(0.08f, 0.08f, 0.08f));
        UIFactory.SetLayoutElement(imageHolder, minWidth: 50, minHeight: 25, flexibleWidth: 999, flexibleHeight: 0);
    }
    private void CreateImageObject()
    {
        imageObject = UIFactory.CreateUIObject("ColorImageHelper", imageHolder, new Vector2(100, 25));
        colorImage = imageObject.AddComponent<Image>();
        colorImage.color = Value is Color value ? value : (Color)(Color32)Value;
    }
    private void CreateEditorGroup()
    {
        // sliders / inputs
        editorGroup = UIFactory.CreateVerticalGroup(
            baseHorizontalGroup,
            "EditorsGroup",
            false, false, true, true, 3,
            new Vector4(3, 3, 3, 3),
            new Color(1, 1, 1, 0));
    }
    private void CreateEditorGrid()
    {
        grid = UIFactory.CreateGridGroup(
            editorGroup,
            "Grid",
            new Vector2(140, 25),
            new Vector2(2, 2),
            new Color(1, 1, 1, 0));
        UIFactory.SetLayoutElement(grid, minWidth: 580, minHeight: 25, flexibleWidth: 900);
    }
    private void CreateColorPropertyInputs() =>
        colorProperties = InteractiveColorProperty.CreateAll(this);
    #endregion
    #region Callbacks
    public override void RefreshUIForValue()
    {
        base.RefreshUIForValue();
        RefreshColorUI();
    }
    protected internal override void OnToggleSubContent(bool toggle)
    {
        base.OnToggleSubContent(toggle);
        RefreshColorUI();
    }
    private void RefreshColorUI()
    {
        foreach (var property in colorProperties)
            property.RefreshColorUI();
    }
    #endregion
}

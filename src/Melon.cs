using System.IO;
using BluePrinceModPreferencesManager.UI;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using UniverseLib.Input;

namespace BluePrinceModPreferencesManager;

public class Melon : MelonMod
{
    public const string GUID = "com.markekraus.BluePrinceModPreferencesManager";
    public const string Name = "BluePrinceModPreferencesManager";
    public const string Author = "MarkEKraus";
    public const string Version = "1.0.0";
    public const string Version4 = "1.0.0.0";
    internal static MelonPreferences_Category _category;
    internal static MelonPreferences_Entry<KeyCode> _mainMenuToggle;
    internal static MelonPreferences_Entry<float> _startupDelay;
    internal static MelonPreferences_Entry<bool> _disableEventSystemOverride;

    public override void OnInitializeMelon()
    {
        InitPreferences();

        UniverseLib.Universe.Init(_startupDelay.Value, LateInit, LogUniverseLib, new()
        {
            Disable_EventSystem_Override = _disableEventSystemOverride.Value,
            Force_Unlock_Mouse = true,
            Unhollowed_Modules_Folder =
                    Path.Combine(
                        Path.GetDirectoryName(MelonEnvironment.ModsDirectory),
                        Path.Combine("MelonLoader", "Managed"))
        });
        LogMsg($"Initialized! UI will initialize in {_startupDelay.Value} seconds.");
    }

    private void LateInit()
    {
        UIManager.Init();
    }
    public override void OnUpdate()
    {
        if (UIManager.Instance == null || !UIManager.Instance.UIRoot)
            return;

        if (InputManager.GetKeyDown(_mainMenuToggle.Value))
            UIManager.ShowMenu = !UIManager.ShowMenu;
    }

    private void InitPreferences()
    {
        _category = MelonPreferences.CreateCategory("BluePrinceModPreferencesManager", "Settings for Blue Prince Mod Preferences Manager");
        _mainMenuToggle = _category.CreateEntry<KeyCode>(
            "MainMenuToggle",
            KeyCode.F5,
            "Main Menu Toggle",
            "The key that toggles the main menu for the preferences manager.");
        _startupDelay = _category.CreateEntry<float>(
            "StartupDelay",
            1f,
            "Startup Delay",
            "Delay before activating the UI of this mod. Default: 1",
            false, false,
            new FloatValidator(1f, 1f, 100f));
        _disableEventSystemOverride = _category.CreateEntry<bool>(
            "DisableEventSystemOverride",
            false,
            "Disable EventSystem Override",
            "Disable EventSystem Override in UniverseLib. Default: false");
    }
}

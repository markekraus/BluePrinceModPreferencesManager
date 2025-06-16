using MelonLoader;
using UnityEngine;

namespace BluePrinceModPreferencesManager;

internal class PreferenceInfo
{
    public MelonPreferences_Entry Preference;
    public bool IsHidden => Preference.IsHidden;
    internal GameObject Content;
}
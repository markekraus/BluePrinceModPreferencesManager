using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using UnityEngine;
using UniverseLib.UI.Models;

namespace BluePrinceModPreferencesManager;

internal class CategoryInfo
{
    public MelonPreferences_Category Category;
    internal List<PreferenceInfo> Preferences = new();
    internal bool IsCompletelyHidden;
    internal ButtonRef ListButton;
    internal GameObject Content;
    internal IEnumerable<GameObject> HiddenEntries =>
        Preferences
            .Where(it => it.IsHidden)
            .Select(it => it.Content);
    public CategoryInfo() { }
    public CategoryInfo(MelonPreferences_Category Category) =>
        this.Category = Category;
}

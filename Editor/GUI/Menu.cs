using Dragonstone.GUI.Settings;
using UnityEditor;

namespace Dragonstone.GUI
{
    public class Menu
    {
        [MenuItem("Divine Dragon/Core/Settings", false, 1400)]
        public static void ShowDivineRipperSettings()
        {
            SettingsService.OpenProjectSettings(DivineDragonCoreSettingsProvider.DivineDragonCoreSettingsPath);
        }
    }
}
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dragonstone.GUI.Settings
{
    public class DivineDragonCoreSettingsProvider
    {
        /// <summary>
        /// Path to the DivineDragon Settings page
        /// </summary>
        public const string DivineDragonCoreSettingsPath = "Project/Divine Dragon";

        /// <summary>
        /// Path to Engage's settings.json file
        /// </summary>
        public const string DragonstoneEngageSettingsPathKey = "Dragonstone_EngageSettingsPath";
        
        [SettingsProvider]
        public static SettingsProvider CreateDivineRipperSettingsProvider()
        {
            var provider = new SettingsProvider(DivineDragonCoreSettingsPath, SettingsScope.Project)
            {
                label = "Divine Dragon",

                guiHandler = (searchContext) =>
                {
                    EditorGUI.BeginChangeCheck();
                    
                    EditorGUILayout.BeginVertical();
                    
                    EditorGUILayout.LabelField("Path to settings.json from Fire Emblem Engage:");

                    EditorGUILayout.BeginHorizontal();
                    
                    string settingsPath = EditorGUILayout.TextField(EditorPrefs.GetString(DragonstoneEngageSettingsPathKey, ""));

                    if (GUILayout.Button("Browse...", GUILayout.MaxWidth(80)))
                    {
                        string selectedPath = EditorUtility.OpenFilePanelWithFilters("Select settings.json from your game dump", settingsPath, new string[] { "Settings.json", "json" });
                        
                        if (!string.IsNullOrEmpty(selectedPath))
                        {
                            settingsPath = selectedPath;
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.EndVertical();
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        // If the user writes the path by hand, this will trigger a change after every character.
                        // So before setting the variables, make sure the path exists.
                        if (File.Exists(settingsPath) && Path.GetFileName(settingsPath) == "settings.json")
                        {
                            EditorPrefs.SetString(DragonstoneEngageSettingsPathKey, settingsPath);
                            EngageAddressableSettings.Initialize(settingsPath);
                        }
                    }
                },
            };

            return provider;
        }
    }
}
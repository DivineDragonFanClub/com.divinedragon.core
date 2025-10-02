using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dragonstone.GUI.Settings;
using UnityEditor;
using UnityEngine;

namespace Dragonstone
{
    public static class EngageAddressableSettings
    {
        private static string _gameCatalogLocation;
        private static string _gameRuntimePath;
        private static string _gameBuildTarget;
        
        [Serializable]
        private class AddressableSettingsRoot
        {
            public string m_buildTarget;
            public List<AddressableSettingsCatalogLocation> m_CatalogLocations;
        }
        
        [Serializable]
        private class AddressableSettingsCatalogLocation
        {
            public string m_InternalId;
        }

        /// <summary>
        /// Path that contains the catalog and settings files
        /// </summary>
        public static string GameRuntimePath { get; private set; }
        
        public static string GameCatalogLocation { get; private set; }
        
        /// <summary>
        /// Name of the target for the assets (Switch)
        /// </summary>
        public static string GameBuildTarget { get; private set; }
        
        /// <summary>
        /// Path to the directory containing the Unity assets
        /// </summary>
        public static string GameBuildPath => GameRuntimePath + "/" + GameBuildTarget;

        [InitializeOnLoadMethod]
        private static void InitializeOnLoad()
        {
            // We can somewhat safely assume that it exists because the settings check if it does
            string settingsPath =
                EditorPrefs.GetString(DivineDragonCoreSettingsProvider.DragonstoneEngageSettingsPathKey, "");
            
            if (!string.IsNullOrEmpty(settingsPath))
            {
                Initialize(settingsPath);
            }
        }
        
        public static bool Initialize(string settingsPath)
        {
            if (!string.IsNullOrEmpty(settingsPath))
            {
                if (File.Exists(settingsPath))
                {
                    string json = File.ReadAllText(settingsPath);
                    AddressableSettingsRoot settings = JsonUtility.FromJson<AddressableSettingsRoot>(json);
                
                    GameRuntimePath = Path.GetDirectoryName(settingsPath).Replace("\\", "/");
                    GameCatalogLocation = settings.m_CatalogLocations.First().m_InternalId.Replace("{UnityEngine.AddressableAssets.Addressables.RuntimePath}", GameRuntimePath);
                    GameBuildTarget = settings.m_buildTarget;

                    return true;
                }
                
                Debug.LogError("Engage settings.json not found.");
            }
            else
            {
                Debug.Log("Divine Dragon Core settings are not set.");
            }
            
            GameRuntimePath = null;
            GameCatalogLocation = null;
            GameBuildTarget = null;
            
            return false;
        }
    }
}
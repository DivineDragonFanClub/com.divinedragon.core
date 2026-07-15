using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Dragonstone
{
    [InitializeOnLoad]
    public static class ShaderInstaller
    {
        private const string DestParent = "Assets/Share/CustomRP";
        private const string ShaderDest = "Assets/Share/CustomRP/Shaders";
        private const string ShaderDestMeta = "Assets/Share/CustomRP/Shaders.meta";
        private const string InstalledVersionFile = "Assets/Share/CustomRP/.divine-shader-version";

        private const string PkgShaderTree = "Packages/com.divinedragon.core/Shaders~/Shaders";
        private const string PkgShaderMeta = "Packages/com.divinedragon.core/Shaders~/Shaders.meta";
        private const string PkgVersionFile = "Packages/com.divinedragon.core/Shaders~/payload-version.txt";
        private const string UrpMarker = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl";

        private const string AddressGroup = "fe";
        private const string AddressPrefix = "CustomRP/Shaders/";

        static ShaderInstaller()
        {
            EditorApplication.delayCall += () => Sync(force: false);
        }

        [MenuItem("Divine Dragon/Reinstall CustomRP Shaders", false, 1600)]
        private static void ForceReinstall() => Sync(force: true);

        private static void Sync(bool force)
        {
            string pkgTree = Path.GetFullPath(PkgShaderTree);
            if (!Directory.Exists(pkgTree))
                return;

            int payloadVersion = ReadVersion(Path.GetFullPath(PkgVersionFile), 1);
            bool installed = Directory.Exists(ShaderDest);
            int installedVersion = installed ? ReadVersion(InstalledVersionFile, 0) : -1;

            if (!force && installed && installedVersion >= payloadVersion)
                return;

            if (!File.Exists(Path.GetFullPath(UrpMarker)))
            {
                Debug.LogWarning("[Dragonstone] URP not ready, deferring CustomRP shader install.");
                return;
            }

            bool fresh = !installed;
            if (installed)
            {
                AssetDatabase.DeleteAsset(ShaderDest);
            }

            Directory.CreateDirectory(DestParent);
            CopyDir(pkgTree, ShaderDest);
            File.Copy(Path.GetFullPath(PkgShaderMeta), ShaderDestMeta, true);
            AssetDatabase.Refresh();

            RegisterShaderAddressables();
            WriteVersion(InstalledVersionFile, payloadVersion);

            Debug.Log(fresh
                ? $"[Dragonstone] Installed CustomRP shaders (v{payloadVersion})."
                : $"[Dragonstone] Updated CustomRP shaders to v{payloadVersion}.");
        }

        private static int ReadVersion(string path, int fallback)
        {
            try
            {
                if (File.Exists(path) && int.TryParse(File.ReadAllText(path).Trim(), out int v))
                    return v;
            }
            catch { }
            return fallback;
        }

        private static void WriteVersion(string path, int version)
        {
            try { File.WriteAllText(path, version.ToString()); }
            catch { }
        }

        private static void CopyDir(string src, string dst)
        {
            Directory.CreateDirectory(dst);
            foreach (string file in Directory.GetFiles(src))
                File.Copy(file, Path.Combine(dst, Path.GetFileName(file)), true);
            foreach (string dir in Directory.GetDirectories(src))
                CopyDir(dir, Path.Combine(dst, Path.GetFileName(dir)));
        }

        private static void RegisterShaderAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogWarning("[Dragonstone] No Addressable settings found, skipped shader registration.");
                return;
            }

            var group = settings.FindGroup(AddressGroup);
            if (group == null)
            {
                Debug.LogWarning($"[Dragonstone] Addressable group '{AddressGroup}' not found, skipped shader registration.");
                return;
            }

            foreach (string path in Directory.GetFiles(ShaderDest, "*.shader", SearchOption.AllDirectories))
            {
                string assetPath = path.Replace('\\', '/');
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (string.IsNullOrEmpty(guid))
                    continue;
                var entry = settings.CreateOrMoveEntry(guid, group, readOnly: false, postEvent: false);
                string rel = assetPath.Substring((ShaderDest + "/").Length);
                entry.address = AddressPrefix + rel;
            }
            AssetDatabase.SaveAssets();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Creates ItemDefinition, QuestDefinition and DialogueGraph assets from
    /// the Sdx17 menu without referencing any other Sdx17 package at compile
    /// time. The lookup order for each requested definition is:
    ///   1. A type with that name already in the project (found by reflection
    ///      across all loaded assemblies). If it derives from ScriptableObject,
    ///      a real asset of that type is created.
    ///   2. Otherwise a standalone definition script is written from the
    ///      template text file in Editor/Templates/, compiled by Unity on
    ///      import, and an asset of the new type is created.
    /// Either way the plugin itself never needs the Inventory, Quest or
    /// Dialogue package installed to compile.
    /// </summary>
    public static class CodeGenerators
    {
        [MenuItem("Sdx17/Generate/Item Definition")]
        public static void GenerateItemDefinitionMenu()
        {
            GenerateDefinition("ItemDefinition", "Sdx17.Inventory", "Editor/Templates/ItemDefinitionTemplate.txt");
        }

        [MenuItem("Sdx17/Generate/Quest Definition")]
        public static void GenerateQuestDefinitionMenu()
        {
            GenerateDefinition("QuestDefinition", "Sdx17.Quest", "Editor/Templates/QuestDefinitionTemplate.txt");
        }

        [MenuItem("Sdx17/Generate/Dialogue Graph")]
        public static void GenerateDialogueGraphMenu()
        {
            GenerateDefinition("DialogueGraph", "Sdx17.Dialogue", "Editor/Templates/DialogueGraphTemplate.txt");
        }

        /// <summary>
        /// Generates one definition asset. Returns the path of the created
        /// asset, or an empty string when generation could not complete.
        /// The asset is created next to the currently selected folder when
        /// one is selected, otherwise in Assets/.
        /// </summary>
        public static string GenerateDefinition(string typeName, string preferredNamespace, string templateRelativePath)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                Debug.LogError("[Sdx17] GenerateDefinition called with an empty type name.");
                return string.Empty;
            }

            string folder = GetSelectedFolderOrAssetsRoot();
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            string prefix = settings != null ? settings.AssetNamePrefix : "Sdx17_";
            string assetName = $"{prefix}{typeName}_New";
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, assetName + ".asset"));

            Type existingType = FindScriptableObjectType(typeName, preferredNamespace);
            if (existingType != null)
            {
                ScriptableObject instance = ScriptableObject.CreateInstance(existingType);
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = instance;
                EditorGUIUtility.PingObject(instance);
                Debug.Log($"[Sdx17] Created {existingType.FullName} asset at {assetPath} (type already existed in the project).");
                return assetPath;
            }

            // No matching type exists: write the standalone script from the
            // template, wait for Unity to compile it on import, then create
            // the asset from the freshly compiled type.
            string scriptPath = WriteScriptFromTemplate(typeName, preferredNamespace, templateRelativePath, folder);
            if (string.IsNullOrEmpty(scriptPath))
            {
                return string.Empty;
            }

            AssetDatabase.Refresh();
            Debug.Log($"[Sdx17] No '{typeName}' type was found, so a standalone script was created at {scriptPath}. " +
                      "Wait for Unity to finish compiling, then run the same Generate menu item again to create the asset.");
            return scriptPath;
        }

        /// <summary>
        /// Searches every loaded assembly for a ScriptableObject type whose
        /// short name matches, preferring the given namespace. Returns null
        /// when no such type exists.
        /// </summary>
        public static Type FindScriptableObjectType(string typeName, string preferredNamespace)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            List<Type> matches = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException loadException)
                {
                    // A partially loadable assembly still yields the types
                    // that did load; the null entries are skipped below.
                    types = loadException.Types;
                }

                if (types == null)
                {
                    continue;
                }

                foreach (Type type in types)
                {
                    if (type == null || type.Name != typeName)
                    {
                        continue;
                    }

                    if (!typeof(ScriptableObject).IsAssignableFrom(type) || type.IsAbstract)
                    {
                        continue;
                    }

                    matches.Add(type);
                }
            }

            if (matches.Count == 0)
            {
                return null;
            }

            foreach (Type match in matches)
            {
                if (!string.IsNullOrEmpty(preferredNamespace) &&
                    match.Namespace != null &&
                    match.Namespace.StartsWith(preferredNamespace, StringComparison.Ordinal))
                {
                    return match;
                }
            }

            return matches[0];
        }

        /// <summary>
        /// Writes a .cs script from a template text file, replacing the
        /// {TYPE_NAME} and {NAMESPACE} tokens. Returns the script path, or
        /// an empty string on failure.
        /// </summary>
        public static string WriteScriptFromTemplate(string typeName, string targetNamespace,
            string templateRelativePath, string folder)
        {
            if (string.IsNullOrEmpty(typeName) || string.IsNullOrEmpty(templateRelativePath))
            {
                return string.Empty;
            }

            // The template lives next to this package; resolve it relative
            // to the package root by walking up from the Editor folder.
            string packageRoot = GetPackageRoot();
            string templatePath = Path.Combine(packageRoot, templateRelativePath);
            if (!File.Exists(templatePath))
            {
                Debug.LogError($"[Sdx17] Generator template not found at {templatePath}. " +
                               "The Editor/Templates folder must be kept with the package.");
                return string.Empty;
            }

            string template = File.ReadAllText(templatePath);
            string scriptText = template
                .Replace("{TYPE_NAME}", typeName)
                .Replace("{NAMESPACE}", string.IsNullOrEmpty(targetNamespace) ? "Sdx17.Generated" : targetNamespace);

            string scriptPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folder, typeName + ".cs"));
            File.WriteAllText(scriptPath, scriptText);
            AssetDatabase.ImportAsset(scriptPath, ImportAssetOptions.ForceUpdate);
            Debug.Log($"[Sdx17] Created script {scriptPath} from template {templateRelativePath}.");
            return scriptPath;
        }

        /// <summary>
        /// Creates a ScriptableObject asset of a type found by reflection,
        /// without a compile-time reference to it. Used by the hub window
        /// and by tests. Returns the created instance, or null.
        /// </summary>
        public static ScriptableObject CreateAssetFromTypeName(string typeName, string preferredNamespace, string assetPath)
        {
            Type type = FindScriptableObjectType(typeName, preferredNamespace);
            if (type == null || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            ScriptableObject instance = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(instance, assetPath);
            AssetDatabase.SaveAssets();
            return instance;
        }

        /// <summary>Folder of the selected asset in the Project window, or Assets/ when nothing is selected.</summary>
        public static string GetSelectedFolderOrAssetsRoot()
        {
            UnityEngine.Object selected = Selection.activeObject;
            if (selected == null)
            {
                return "Assets";
            }

            string path = AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrEmpty(path))
            {
                return "Assets";
            }

            if (AssetDatabase.IsValidFolder(path))
            {
                return path;
            }

            string directory = Path.GetDirectoryName(path);
            return string.IsNullOrEmpty(directory) ? "Assets" : directory.Replace('\\', '/');
        }

        /// <summary>
        /// Locates the package root by finding this package's package.json
        /// on disk, starting from the known Editor folder location.
        /// Falls back to Assets/ when the package is installed as a folder
        /// and the file cannot be found.
        /// </summary>
        public static string GetPackageRoot()
        {
            // When installed as a UPM package this file sits in
            // Packages/com.sdx17.devplugin/Editor/; when copied into a
            // project it sits in Assets/.../Editor/. Either way the
            // package.json is two levels up from this source file's folder
            // at edit time, and at runtime we search the usual locations.
            string[] candidates =
            {
                "Packages/com.sdx17.devplugin",
                "Assets/Sdx17DevPlugin",
                "Assets/Sdx17/DevPlugin"
            };

            foreach (string candidate in candidates)
            {
                string manifest = Path.Combine(candidate, "package.json");
                if (File.Exists(manifest))
                {
                    return candidate;
                }
            }

            // Last resort: search the project for the manifest by name and
            // confirm it belongs to this package.
            string[] guids = AssetDatabase.FindAssets("package");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith("package.json", StringComparison.Ordinal) && File.Exists(path))
                {
                    string text = File.ReadAllText(path);
                    if (text.Contains("com.sdx17.devplugin", StringComparison.Ordinal))
                    {
                        return Path.GetDirectoryName(path).Replace('\\', '/');
                    }
                }
            }

            return "Assets";
        }
    }
}

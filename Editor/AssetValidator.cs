using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Checks the open scene and the currently selected assets for the
    /// problems that most often break a build or a Play Mode run: missing
    /// scripts, missing (null) references, empty required string fields and
    /// names that break the project naming rules. The validator never
    /// modifies anything; it only reports.
    /// </summary>
    public static class AssetValidator
    {
        [MenuItem("Sdx17/Validate/Open Scene")]
        public static void ValidateOpenSceneMenu()
        {
            ValidationReport report = ValidateOpenScene();
            LogReport(report, "Open scene validation");
        }

        [MenuItem("Sdx17/Validate/Selected Assets")]
        public static void ValidateSelectedAssetsMenu()
        {
            ValidationReport report = ValidateSelectedAssets();
            LogReport(report, "Selected assets validation");
        }

        [MenuItem("Sdx17/Validate/Whole Project (Scripts Only)")]
        public static void ValidateProjectScriptsMenu()
        {
            ValidationReport report = ValidateProjectScripts();
            LogReport(report, "Project script validation");
        }

        /// <summary>Validates every GameObject in the active scene.</summary>
        public static ValidationReport ValidateOpenScene()
        {
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            ValidationReport report = new ValidationReport();
            Scene scene = SceneManager.GetActiveScene();

            if (!scene.IsValid())
            {
                report.Add(ValidationSeverity.Error, "Scene", "No valid scene is open.");
                return report;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (root == null)
                {
                    continue;
                }

                ValidateGameObjectTree(root, settings, report);
            }

            report.Add(ValidationSeverity.Info, scene.name,
                $"Scene '{scene.name}' has {roots.Length} root GameObject(s).");
            return report;
        }

        /// <summary>Validates the assets currently selected in the Project window.</summary>
        public static ValidationReport ValidateSelectedAssets()
        {
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            ValidationReport report = new ValidationReport();
            UnityEngine.Object[] selection = Selection.objects;

            if (selection == null || selection.Length == 0)
            {
                report.Add(ValidationSeverity.Warning, "Selection",
                    "Nothing is selected. Select one or more assets in the Project window first.");
                return report;
            }

            foreach (UnityEngine.Object selected in selection)
            {
                if (selected == null)
                {
                    report.Add(ValidationSeverity.Error, "Selection", "A selected entry is null.");
                    continue;
                }

                CheckAssetName(selected.name, settings, report);

                if (selected is GameObject prefab)
                {
                    ValidateGameObjectTree(prefab, settings, report);
                }
                else if (selected is ScriptableObject scriptable)
                {
                    CheckSerializedFields(scriptable, settings, report);
                }
            }

            return report;
        }

        /// <summary>
        /// Scans every MonoScript in the project for scripts whose class is
        /// missing (for example after a rename without a matching file move).
        /// This is the cheapest whole-project check and the one most worth
        /// running before a build.
        /// </summary>
        public static ValidationReport ValidateProjectScripts()
        {
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            ValidationReport report = new ValidationReport();
            string[] guids = AssetDatabase.FindAssets("t:MonoScript");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                report.CheckedObjectCount++;

                if (script == null)
                {
                    report.Add(ValidationSeverity.Error, path, "MonoScript asset could not be loaded.");
                    continue;
                }

                if (script.GetClass() == null)
                {
                    report.Add(ValidationSeverity.Error, path,
                        "Script has no matching class (class name and file name may differ, or the file does not compile).");
                }

                CheckAssetName(script.name, settings, report);
            }

            if (guids.Length == 0)
            {
                report.Add(ValidationSeverity.Warning, "Project", "No MonoScript assets were found.");
            }

            return report;
        }

        /// <summary>Walks a GameObject and all of its children, checking every component.</summary>
        public static void ValidateGameObjectTree(GameObject root, DevPluginSettings settings, ValidationReport report)
        {
            if (root == null || settings == null || report == null)
            {
                return;
            }

            // GetComponentsInChildren(true) includes inactive objects, which
            // is what we want: a disabled object with a missing script still
            // breaks the build.
            Component[] components = root.GetComponentsInChildren<Component>(true);
            foreach (Component component in components)
            {
                report.CheckedObjectCount++;
                GameObject owner = component == null ? root : component.gameObject;
                string target = GetHierarchyPath(owner);

                if (component == null)
                {
                    if (settings.CheckMissingScripts)
                    {
                        report.Add(ValidationSeverity.Error, target,
                            "Missing script: a component slot on this GameObject has no script attached.");
                    }

                    continue;
                }

                CheckAssetName(owner.name, settings, report);

                if (settings.CheckMissingReferences || settings.CheckEmptyRequiredFields)
                {
                    CheckSerializedFields(component, settings, report);
                }
            }
        }

        /// <summary>
        /// Uses SerializedObject so any component or ScriptableObject can be
        /// inspected without knowing its concrete type. Reference fields
        /// that are null and string fields that are empty are reported.
        /// </summary>
        public static void CheckSerializedFields(UnityEngine.Object target, DevPluginSettings settings, ValidationReport report)
        {
            if (target == null || settings == null || report == null)
            {
                return;
            }

            SerializedObject serialized;
            try
            {
                serialized = new SerializedObject(target);
            }
            catch (Exception exception)
            {
                report.Add(ValidationSeverity.Warning, target.name,
                    $"Could not inspect serialized fields: {exception.Message}");
                return;
            }

            SerializedProperty property = serialized.GetIterator();
            if (property == null)
            {
                return;
            }

            // EnterChildren(true) walks the whole serialized tree, including
            // nested structs and arrays.
            bool hasNext = property.NextVisible(true);
            while (hasNext)
            {
                CheckSerializedProperty(property, target, settings, report);
                hasNext = property.NextVisible(false);
            }
        }

        private static void CheckSerializedProperty(SerializedProperty property, UnityEngine.Object target,
            DevPluginSettings settings, ValidationReport report)
        {
            if (property == null || property.name == "m_Script")
            {
                return;
            }

            string targetName = target == null ? "Unknown" : target.name;

            if (settings.CheckMissingReferences && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue == null)
                {
                    report.Add(ValidationSeverity.Warning, targetName,
                        $"Missing reference: field '{property.propertyPath}' is null.");
                }
            }

            if (settings.CheckEmptyRequiredFields && property.propertyType == SerializedPropertyType.String)
            {
                if (string.IsNullOrWhiteSpace(property.stringValue) && IsRequiredFieldName(property.name))
                {
                    report.Add(ValidationSeverity.Warning, targetName,
                        $"Empty field: '{property.propertyPath}' is blank but its name suggests it is required.");
                }
            }
        }

        /// <summary>
        /// Field names containing these words are treated as required by the
        /// validator. This is a heuristic, not a contract: a field named
        /// "id", "name", "key" or "title" that is blank is almost always a bug.
        /// </summary>
        private static bool IsRequiredFieldName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return false;
            }

            string lower = fieldName.ToLowerInvariant();
            return lower.Contains("id") || lower.Contains("name") || lower.Contains("key") ||
                   lower.Contains("title") || lower.Contains("display");
        }

        private static void CheckAssetName(string assetName, DevPluginSettings settings, ValidationReport report)
        {
            if (string.IsNullOrEmpty(assetName) || settings == null || report == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(settings.AssetNamePrefix) &&
                !assetName.StartsWith(settings.AssetNamePrefix, StringComparison.Ordinal))
            {
                // Generated Sdx17 assets should carry the prefix; other
                // project assets are only noted, not failed, so the rule
                // never blocks unrelated work.
                report.Add(ValidationSeverity.Info, assetName,
                    $"Name does not start with the configured prefix '{settings.AssetNamePrefix}'.");
            }

            if (settings.RequirePascalCaseNames && !IsPascalCase(assetName))
            {
                report.Add(ValidationSeverity.Warning, assetName,
                    "Name is not PascalCase (first letter upper-case, no spaces or underscores).");
            }
        }

        /// <summary>True when the name starts with an upper-case letter and has no spaces or underscores.</summary>
        public static bool IsPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value) || !char.IsUpper(value[0]))
            {
                return false;
            }

            foreach (char c in value)
            {
                if (c == ' ' || c == '_' || c == '-')
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>Full hierarchy path, e.g. "Player/Body/Weapon", for readable messages.</summary>
        public static string GetHierarchyPath(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return "Unknown GameObject";
            }

            List<string> parts = new List<string>();
            Transform current = gameObject.transform;
            while (current != null)
            {
                parts.Add(current.name);
                current = current.parent;
            }

            parts.Reverse();
            return string.Join("/", parts);
        }

        /// <summary>Writes the report to the console, one line per finding plus a summary.</summary>
        public static void LogReport(ValidationReport report, string title)
        {
            if (report == null)
            {
                return;
            }

            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            if (settings != null && !settings.LogToConsole)
            {
                return;
            }

            Debug.Log($"[Sdx17] {title}: {report.SummaryLine()}");
            foreach (ValidationResult result in report.Results)
            {
                switch (result.Severity)
                {
                    case ValidationSeverity.Error:
                        Debug.LogError($"[Sdx17] {result}");
                        break;
                    case ValidationSeverity.Warning:
                        Debug.LogWarning($"[Sdx17] {result}");
                        break;
                    default:
                        Debug.Log($"[Sdx17] {result}");
                        break;
                }
            }
        }
    }
}

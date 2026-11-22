using System;
using UnityEditor;
using UnityEngine;

namespace Sdx17.DevPlugin.Tests
{
    /// <summary>
    /// Editor checklist for the Developer Plugin. Run it from
    /// Sdx17 &gt; Tests &gt; Run Developer Plugin Checklist. Every check logs
    /// PASS or FAIL to the Console with a short reason, and a final line
    /// totals the result. Nothing here modifies project assets except the
    /// audit, which only writes its Markdown report file.
    /// </summary>
    public static class DevPluginChecklist
    {
        [MenuItem("Sdx17/Tests/Run Developer Plugin Checklist")]
        public static void RunChecklistMenu()
        {
            RunChecklist();
        }

        /// <summary>Runs all checks and returns the number of failures (0 = all passed).</summary>
        public static int RunChecklist()
        {
            int passed = 0;
            int failed = 0;

            Check("Settings load returns defaults when no asset exists", () =>
            {
                DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
                return settings != null &&
                       !string.IsNullOrEmpty(settings.AssetNamePrefix) &&
                       settings.LargeAssetWarningBytes > 0;
            }, ref passed, ref failed);

            Check("Settings ResetToDefaults restores known values", () =>
            {
                DevPluginSettings settings = ScriptableObject.CreateInstance<DevPluginSettings>();
                settings.ResetToDefaults();
                return settings.AssetNamePrefix == "Sdx17_" &&
                       settings.AuditReportPath == "Assets/Sdx17AuditReport.md";
            }, ref passed, ref failed);

            Check("Validator handles an empty selection without errors", () =>
            {
                Selection.objects = new UnityEngine.Object[0];
                ValidationReport report = AssetValidator.ValidateSelectedAssets();
                return report != null && report.WarningCount >= 0;
            }, ref passed, ref failed);

            Check("Validator PascalCase check accepts PascalCase and rejects lower-case", () =>
            {
                return AssetValidator.IsPascalCase("PlayerHealth") &&
                       !AssetValidator.IsPascalCase("playerHealth") &&
                       !AssetValidator.IsPascalCase("Player Health");
            }, ref passed, ref failed);

            Check("Generator reflection lookup returns null for a type that does not exist", () =>
            {
                Type found = CodeGenerators.FindScriptableObjectType(
                    "Sdx17DevPluginTypeThatCannotExist", "Sdx17.DevPlugin");
                return found == null;
            }, ref passed, ref failed);

            Check("Generator reflection lookup finds DevPluginSettings by name", () =>
            {
                Type found = CodeGenerators.FindScriptableObjectType("DevPluginSettings", "Sdx17.DevPlugin");
                return found != null && typeof(ScriptableObject).IsAssignableFrom(found);
            }, ref passed, ref failed);

            Check("Scene helper reports the starter setup consistently", () =>
            {
                bool exists = SceneHelper.StarterSetupExists();
                // Both outcomes are valid here: the check is that the query
                // runs and agrees with the individual object lookups.
                bool manual = GameObject.Find(SceneHelper.PlayerObjectName) != null &&
                              GameObject.Find(SceneHelper.CameraObjectName) != null &&
                              GameObject.Find(SceneHelper.LightObjectName) != null;
                return exists == manual;
            }, ref passed, ref failed);

            Check("Build checklist runs and returns at least one item", () =>
            {
                BuildChecklistReport report = BuildHelper.RunPreBuildChecklist();
                return report != null && report.Items.Count >= 4;
            }, ref passed, ref failed);

            Check("Audit runs and counts at least the package scripts", () =>
            {
                AuditReport report = ProjectAuditReport.RunAudit();
                return report != null && report.TotalAssets > 0 && report.TotalBytes >= 0;
            }, ref passed, ref failed);

            Check("Audit byte formatting is correct", () =>
            {
                return ProjectAuditReport.FormatBytes(500) == "500 B" &&
                       ProjectAuditReport.FormatBytes(2048) == "2.0 KB" &&
                       ProjectAuditReport.FormatBytes(1024 * 1024) == "1.0 MB";
            }, ref passed, ref failed);

            Check("Runtime info matches package.json", () =>
            {
                return DevPluginInfo.Version == "0.1.0" &&
                       DevPluginInfo.PackageName == "com.sdx17.devplugin";
            }, ref passed, ref failed);

            Debug.Log($"[Sdx17] Developer Plugin checklist: {passed} PASS, {failed} FAIL.");
            return failed;
        }

        private static void Check(string name, Func<bool> test, ref int passed, ref int failed)
        {
            bool result;
            try
            {
                result = test != null && test();
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Sdx17] FAIL - {name}: threw {exception.GetType().Name}: {exception.Message}");
                failed++;
                return;
            }

            if (result)
            {
                Debug.Log($"[Sdx17] PASS - {name}");
                passed++;
            }
            else
            {
                Debug.LogError($"[Sdx17] FAIL - {name}");
                failed++;
            }
        }
    }
}

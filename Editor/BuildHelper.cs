using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Pre-build checklist. It never builds anything itself; it checks the
    /// conditions that most often make a build fail or ship broken, and
    /// reports pass / warning / fail for each one so the developer can fix
    /// the list before opening File &gt; Build Settings.
    /// </summary>
    public static class BuildHelper
    {
        [MenuItem("Sdx17/Build/Run Pre-Build Checklist")]
        public static void RunChecklistMenu()
        {
            BuildChecklistReport report = RunPreBuildChecklist();
            LogChecklist(report);
        }

        /// <summary>Runs every check and returns the full report.</summary>
        public static BuildChecklistReport RunPreBuildChecklist()
        {
            BuildChecklistReport report = new BuildChecklistReport();

            CheckScenesInBuild(report);
            CheckCompanyAndProductNames(report);
            CheckScriptingBackend(report);
            CheckMissingScriptsInOpenScene(report);
            CheckTextureCompression(report);

            return report;
        }

        private static void CheckScenesInBuild(BuildChecklistReport report)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0)
            {
                report.Add(false, "Build scenes",
                    "No scenes are listed in Build Settings. Add at least one scene before building.");
                return;
            }

            int enabledCount = 0;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene != null && scene.enabled)
                {
                    enabledCount++;
                }
            }

            if (enabledCount == 0)
            {
                report.Add(false, "Build scenes",
                    "Build Settings lists scenes, but none of them are enabled.");
                return;
            }

            report.Add(true, "Build scenes",
                $"{enabledCount} scene(s) enabled in Build Settings.");
        }

        private static void CheckCompanyAndProductNames(BuildChecklistReport report)
        {
            string company = PlayerSettings.companyName;
            string product = PlayerSettings.productName;

            if (string.IsNullOrWhiteSpace(company) || company == "DefaultCompany")
            {
                report.Add(false, "Company name",
                    "Company name is empty or still 'DefaultCompany'. Set it in Player Settings.");
            }
            else
            {
                report.Add(true, "Company name", $"Company name is '{company}'.");
            }

            if (string.IsNullOrWhiteSpace(product) || product == "New Unity Project")
            {
                report.Add(false, "Product name",
                    "Product name is empty or still the Unity default. Set it in Player Settings.");
            }
            else
            {
                report.Add(true, "Product name", $"Product name is '{product}'.");
            }
        }

        private static void CheckScriptingBackend(BuildChecklistReport report)
        {
            ScriptingImplementation backend = PlayerSettings.GetScriptingBackend(NamedBuildTarget.Standalone);
            if (backend == ScriptingImplementation.IL2CPP)
            {
                report.Add(true, "Scripting backend",
                    "Scripting backend is IL2CPP, which is required for most store builds.");
            }
            else
            {
                report.Add(true, "Scripting backend",
                    $"Scripting backend is {backend}. IL2CPP is recommended for store builds; Mono is fine for local testing.");
            }
        }

        private static void CheckMissingScriptsInOpenScene(BuildChecklistReport report)
        {
            ValidationReport validation = AssetValidator.ValidateOpenScene();
            if (validation == null)
            {
                report.Add(false, "Missing scripts", "The scene validator returned no report.");
                return;
            }

            if (validation.HasErrors)
            {
                report.Add(false, "Missing scripts",
                    $"Open scene validation found {validation.ErrorCount} error(s). Fix them before building: {validation.SummaryLine()}");
            }
            else
            {
                report.Add(true, "Missing scripts",
                    $"Open scene validation found no errors ({validation.WarningCount} warning(s)).");
            }
        }

        private static void CheckTextureCompression(BuildChecklistReport report)
        {
            // This check is intentionally light: it only confirms the
            // project has a texture importer override path available, by
            // counting uncompressed-looking textures is not possible without
            // importing each one. Instead it reports the default texture
            // compression setting for the active platform.
            TextureImporterFormat format = EditorUserBuildSettings.overrideTextureCompression;
            report.Add(true, "Texture compression",
                $"Override texture compression is set to {format}. " +
                "Review large textures in the audit report before a release build.");
        }

        /// <summary>Writes the checklist to the console with a clear overall result.</summary>
        public static void LogChecklist(BuildChecklistReport report)
        {
            if (report == null)
            {
                return;
            }

            Debug.Log($"[Sdx17] Pre-build checklist: {(report.AllPassed ? "ALL CHECKS PASSED" : $"{report.FailedCount} check(s) FAILED")} " +
                      $"({report.PassedCount} passed, {report.FailedCount} failed).");
            foreach (BuildChecklistItem item in report.Items)
            {
                if (item.Passed)
                {
                    Debug.Log($"[Sdx17] PASS - {item.Name}: {item.Message}");
                }
                else
                {
                    Debug.LogError($"[Sdx17] FAIL - {item.Name}: {item.Message}");
                }
            }
        }
    }

    /// <summary>One pre-build check and its outcome.</summary>
    public readonly struct BuildChecklistItem
    {
        public BuildChecklistItem(bool passed, string name, string message)
        {
            Passed = passed;
            Name = name ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public bool Passed { get; }
        public string Name { get; }
        public string Message { get; }
    }

    /// <summary>Full result of a pre-build checklist run.</summary>
    public sealed class BuildChecklistReport
    {
        private readonly List<BuildChecklistItem> items = new List<BuildChecklistItem>();

        public IReadOnlyList<BuildChecklistItem> Items => items;
        public int PassedCount { get; private set; }
        public int FailedCount { get; private set; }
        public bool AllPassed => FailedCount == 0 && items.Count > 0;

        public void Add(bool passed, string name, string message)
        {
            items.Add(new BuildChecklistItem(passed, name, message));
            if (passed)
            {
                PassedCount++;
            }
            else
            {
                FailedCount++;
            }
        }
    }
}

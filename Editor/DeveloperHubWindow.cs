using UnityEditor;
using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// The single entry point for the plugin: Sdx17 &gt; Developer Hub.
    /// Every button calls the real tool behind it and shows the last result
    /// in the window, so the hub is a launcher, not a separate copy of the
    /// logic. All work is editor-only.
    /// </summary>
    public class DeveloperHubWindow : EditorWindow
    {
        [SerializeField] private string lastResult = "No tool has run yet in this session.";
        [SerializeField] private Vector2 scrollPosition;

        [MenuItem("Sdx17/Developer Hub")]
        public static void Open()
        {
            DeveloperHubWindow window = GetWindow<DeveloperHubWindow>("Sdx17 Developer Hub");
            window.minSize = new Vector2(360f, 420f);
            window.Show();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Sdx17 Developer Hub", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Editor tools for Sdx17 Unity projects. Version 0.1.0 — untested in Unity.",
                EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Validate", EditorStyles.boldLabel);
            if (GUILayout.Button("Validate Open Scene"))
            {
                ValidationReport report = AssetValidator.ValidateOpenScene();
                AssetValidator.LogReport(report, "Open scene validation");
                lastResult = report.SummaryLine();
            }

            if (GUILayout.Button("Validate Selected Assets"))
            {
                ValidationReport report = AssetValidator.ValidateSelectedAssets();
                AssetValidator.LogReport(report, "Selected assets validation");
                lastResult = report.SummaryLine();
            }

            if (GUILayout.Button("Validate Project Scripts"))
            {
                ValidationReport report = AssetValidator.ValidateProjectScripts();
                AssetValidator.LogReport(report, "Project script validation");
                lastResult = report.SummaryLine();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Generate", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Creates a ScriptableObject asset from an existing Sdx17 type when one is " +
                                       "installed, otherwise a standalone definition script from a template.",
                EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Generate Item Definition"))
            {
                string path = CodeGenerators.GenerateDefinition("ItemDefinition", "Sdx17.Inventory",
                    "Editor/Templates/ItemDefinitionTemplate.txt");
                lastResult = string.IsNullOrEmpty(path) ? "ItemDefinition generation failed. See the Console." : $"Created: {path}";
            }

            if (GUILayout.Button("Generate Quest Definition"))
            {
                string path = CodeGenerators.GenerateDefinition("QuestDefinition", "Sdx17.Quest",
                    "Editor/Templates/QuestDefinitionTemplate.txt");
                lastResult = string.IsNullOrEmpty(path) ? "QuestDefinition generation failed. See the Console." : $"Created: {path}";
            }

            if (GUILayout.Button("Generate Dialogue Graph"))
            {
                string path = CodeGenerators.GenerateDefinition("DialogueGraph", "Sdx17.Dialogue",
                    "Editor/Templates/DialogueGraphTemplate.txt");
                lastResult = string.IsNullOrEmpty(path) ? "DialogueGraph generation failed. See the Console." : $"Created: {path}";
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Player / Camera / Light Setup"))
            {
                SceneSetupResult result = SceneHelper.CreateStarterSetup();
                lastResult = result.Message;
                Debug.Log($"[Sdx17] {result.Message}");
            }

            EditorGUILayout.LabelField(SceneHelper.StarterSetupExists()
                ? "Starter setup: present in the open scene."
                : "Starter setup: not present in the open scene.");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Build & Audit", EditorStyles.boldLabel);
            if (GUILayout.Button("Run Pre-Build Checklist"))
            {
                BuildChecklistReport report = BuildHelper.RunPreBuildChecklist();
                BuildHelper.LogChecklist(report);
                lastResult = report.AllPassed
                    ? "Pre-build checklist: all checks passed."
                    : $"Pre-build checklist: {report.FailedCount} check(s) failed. See the Console.";
            }

            if (GUILayout.Button("Run Project Audit"))
            {
                AuditReport report = ProjectAuditReport.RunAudit();
                string path = ProjectAuditReport.WriteReport(report);
                lastResult = $"Audit written to {path}. {report.SummaryLine()}";
                Debug.Log($"[Sdx17] {lastResult}");
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (GUILayout.Button("Create / Select Settings Asset"))
            {
                CreateOrSelectSettings();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Last result", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(lastResult, EditorStyles.wordWrappedLabel);

            EditorGUILayout.EndScrollView();
        }

        private void CreateOrSelectSettings()
        {
            DevPluginSettings settings = DevPluginSettings.LoadOrCreate();
            if (settings == null)
            {
                lastResult = "Could not load or create settings.";
                return;
            }

            string path = AssetDatabase.GetAssetPath(settings);
            if (string.IsNullOrEmpty(path))
            {
                string newPath = AssetDatabase.GenerateUniqueAssetPath("Assets/DevPluginSettings.asset");
                AssetDatabase.CreateAsset(settings, newPath);
                AssetDatabase.SaveAssets();
                path = newPath;
                lastResult = $"Created settings asset at {path}.";
            }
            else
            {
                lastResult = $"Settings asset already exists at {path}.";
            }

            Selection.activeObject = settings;
            EditorGUIUtility.PingObject(settings);
            Debug.Log($"[Sdx17] {lastResult}");
        }
    }
}

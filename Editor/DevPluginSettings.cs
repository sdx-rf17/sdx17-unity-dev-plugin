using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Project-wide options for the Developer Plugin. Create one from the
    /// menu (Sdx17 &gt; Settings &gt; Create Settings Asset) and keep it in the
    /// project; every tool reads it through <see cref="DevPluginSettings.LoadOrCreate"/>
    /// and falls back to the built-in defaults below when no asset exists,
    /// so the plugin works with no setup at all.
    /// </summary>
    [CreateAssetMenu(fileName = "DevPluginSettings", menuName = "Sdx17/Dev Plugin Settings")]
    public class DevPluginSettings : ScriptableObject
    {
        [SerializeField] private string assetNamePrefix = "Sdx17_";
        [SerializeField] private bool requirePascalCaseNames = true;
        [SerializeField] private bool checkMissingScripts = true;
        [SerializeField] private bool checkMissingReferences = true;
        [SerializeField] private bool checkEmptyRequiredFields = true;
        [SerializeField] private string auditReportPath = "Assets/Sdx17AuditReport.md";
        [SerializeField] private int largeAssetWarningBytes = 10 * 1024 * 1024;
        [SerializeField] private bool logToConsole = true;

        /// <summary>Prefix new generated assets must start with (empty = no rule).</summary>
        public string AssetNamePrefix => assetNamePrefix;

        /// <summary>When true, the validator warns on asset names that are not PascalCase.</summary>
        public bool RequirePascalCaseNames => requirePascalCaseNames;

        public bool CheckMissingScripts => checkMissingScripts;
        public bool CheckMissingReferences => checkMissingReferences;
        public bool CheckEmptyRequiredFields => checkEmptyRequiredFields;

        /// <summary>Project-relative path the audit report is written to.</summary>
        public string AuditReportPath => auditReportPath;

        /// <summary>Assets at or above this size in bytes are listed as large in the audit.</summary>
        public int LargeAssetWarningBytes => largeAssetWarningBytes;

        public bool LogToConsole => logToConsole;

        /// <summary>
        /// Returns the settings asset if one exists anywhere in the project,
        /// otherwise a new in-memory instance carrying the field defaults.
        /// The in-memory instance is never saved, so editing it at runtime
        /// has no effect; create a real asset to persist changes.
        /// </summary>
        public static DevPluginSettings LoadOrCreate()
        {
#if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:DevPluginSettings");
            if (guids != null && guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                DevPluginSettings found = UnityEditor.AssetDatabase.LoadAssetAtPath<DevPluginSettings>(path);
                if (found != null)
                {
                    return found;
                }
            }
#endif
            return CreateInstance<DevPluginSettings>();
        }

        /// <summary>Restores the field defaults shown above.</summary>
        public void ResetToDefaults()
        {
            assetNamePrefix = "Sdx17_";
            requirePascalCaseNames = true;
            checkMissingScripts = true;
            checkMissingReferences = true;
            checkEmptyRequiredFields = true;
            auditReportPath = "Assets/Sdx17AuditReport.md";
            largeAssetWarningBytes = 10 * 1024 * 1024;
            logToConsole = true;
        }
    }
}

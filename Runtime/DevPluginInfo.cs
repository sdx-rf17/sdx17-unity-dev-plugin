using UnityEngine;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Small runtime helper shipped with the plugin. The plugin itself is
    /// editor-only; this class exists so runtime code and samples can read
    /// the package version and log with the shared "[Sdx17]" prefix without
    /// depending on any editor assembly.
    /// </summary>
    public static class DevPluginInfo
    {
        /// <summary>Package version, matching package.json.</summary>
        public const string Version = "0.1.0";

        /// <summary>Package name, matching package.json.</summary>
        public const string PackageName = "com.sdx17.devplugin";

        /// <summary>True when this build was made from the untested 0.1.0 code.</summary>
        public const bool UntestedInUnity = true;
    }

    /// <summary>Shared log prefix for Sdx17 runtime messages.</summary>
    public static class DevPluginLogger
    {
        public static void Log(object message)
        {
            Debug.Log($"[Sdx17] {message}");
        }

        public static void LogWarning(object message)
        {
            Debug.LogWarning($"[Sdx17] {message}");
        }

        public static void LogError(object message)
        {
            Debug.LogError($"[Sdx17] {message}");
        }
    }
}

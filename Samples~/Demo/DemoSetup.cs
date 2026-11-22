using UnityEngine;

namespace Sdx17.DevPlugin.Samples
{
    /// <summary>
    /// Minimal demo for the Developer Plugin sample. Attach it to an empty
    /// GameObject in a demo scene and press Play: it logs the package
    /// version, checks whether the starter scene setup exists, and reports
    /// the result. It performs no editor work at runtime — the editor tools
    /// themselves are exercised from the Sdx17 menu, not from this script.
    /// </summary>
    public class DemoSetup : MonoBehaviour
    {
        [SerializeField] private bool logOnStart = true;
        [SerializeField] private string demoSceneName = "DevPluginDemo";

        public bool LogOnStart => logOnStart;
        public string DemoSceneName => demoSceneName;

        private void Start()
        {
            if (!logOnStart)
            {
                return;
            }

            DevPluginLogger.Log($"Developer Plugin demo started. Package {DevPluginInfo.PackageName} v{DevPluginInfo.Version}.");
            DevPluginLogger.Log($"Demo scene: {demoSceneName}. Active scene: {gameObject.scene.name}.");

            GameObject player = GameObject.Find("Sdx17_Player");
            if (player == null)
            {
                DevPluginLogger.LogWarning("No Sdx17_Player found. Create the starter setup first: Sdx17 > Scene > Create Player / Camera / Light Setup.");
            }
            else
            {
                DevPluginLogger.Log($"Found player object '{player.name}' at {player.transform.position}.");
            }

            GameObject mainCamera = GameObject.Find("Sdx17_MainCamera");
            GameObject lightObject = GameObject.Find("Sdx17_DirectionalLight");
            DevPluginLogger.Log($"Camera present: {mainCamera != null}. Light present: {lightObject != null}.");
        }
    }
}

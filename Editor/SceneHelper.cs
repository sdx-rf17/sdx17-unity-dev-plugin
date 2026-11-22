using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sdx17.DevPlugin
{
    /// <summary>
    /// Builds a minimal, working starting setup in the open scene: a player
    /// capsule with a CharacterController, a camera that follows it, and a
    /// directional light. Running it twice never duplicates the setup; the
    /// second run reports what already exists.
    /// </summary>
    public static class SceneHelper
    {
        public const string PlayerObjectName = "Sdx17_Player";
        public const string CameraObjectName = "Sdx17_MainCamera";
        public const string LightObjectName = "Sdx17_DirectionalLight";

        [MenuItem("Sdx17/Scene/Create Player / Camera / Light Setup")]
        public static void CreateStarterSetupMenu()
        {
            SceneSetupResult result = CreateStarterSetup();
            Debug.Log($"[Sdx17] Scene setup: {result}");
        }

        [MenuItem("Sdx17/Scene/Select Player")]
        public static void SelectPlayerMenu()
        {
            GameObject player = GameObject.Find(PlayerObjectName);
            if (player == null)
            {
                Debug.LogWarning("[Sdx17] No player setup exists yet. Run Sdx17 > Scene > Create Player / Camera / Light Setup first.");
                return;
            }

            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);
        }

        /// <summary>Creates the setup, or reports the existing objects when it already exists.</summary>
        public static SceneSetupResult CreateStarterSetup()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                return new SceneSetupResult(false, 0, 0, 0, "No valid scene is open.");
            }

            GameObject player = GameObject.Find(PlayerObjectName);
            GameObject cameraObject = GameObject.Find(CameraObjectName);
            GameObject lightObject = GameObject.Find(LightObjectName);

            int created = 0;
            int existing = 0;

            if (player == null)
            {
                player = CreatePlayer();
                created++;
            }
            else
            {
                existing++;
            }

            if (cameraObject == null)
            {
                cameraObject = CreateFollowCamera(player);
                created++;
            }
            else
            {
                existing++;
            }

            if (lightObject == null)
            {
                lightObject = CreateDirectionalLight();
                created++;
            }
            else
            {
                existing++;
            }

            if (created == 0)
            {
                return new SceneSetupResult(true, 0, existing, 3,
                    "Starter setup already exists; nothing was duplicated.");
            }

            // Mark the scene dirty so Unity asks to save the new objects.
            EditorSceneManager.MarkSceneDirty(scene);
            return new SceneSetupResult(true, created, existing, 3,
                $"Created {created} object(s), {existing} already existed.");
        }

        /// <summary>True when all three setup objects already exist in the open scene.</summary>
        public static bool StarterSetupExists()
        {
            return GameObject.Find(PlayerObjectName) != null &&
                   GameObject.Find(CameraObjectName) != null &&
                   GameObject.Find(LightObjectName) != null;
        }

        private static GameObject CreatePlayer()
        {
            // A capsule primitive gives a visible body and a collider in one
            // call; the CharacterController is the movement component most
            // Sdx17 starter projects expect.
            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = PlayerObjectName;
            player.transform.position = new Vector3(0f, 1f, 0f);

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0f, 0f);
            controller.height = 2f;
            controller.radius = 0.5f;

            // The primitive's own CapsuleCollider would fight the
            // CharacterController, so it is removed on purpose.
            CapsuleCollider primitiveCollider = player.GetComponent<CapsuleCollider>();
            if (primitiveCollider != null)
            {
                Object.DestroyImmediate(primitiveCollider);
            }

            Undo.RegisterCreatedObjectUndo(player, "Create Sdx17 Player");
            return player;
        }

        private static GameObject CreateFollowCamera(GameObject player)
        {
            GameObject cameraObject = new GameObject(CameraObjectName);
            cameraObject.transform.position = new Vector3(0f, 4f, -6f);
            cameraObject.transform.LookAt(player != null ? player.transform.position : Vector3.zero);

            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 1000f;

            // Keep the camera usable as the scene's main camera.
            cameraObject.tag = "MainCamera";

            AudioListener listener = cameraObject.AddComponent<AudioListener>();
            if (listener == null)
            {
                Debug.LogWarning("[Sdx17] AudioListener could not be added to the follow camera.");
            }

            Undo.RegisterCreatedObjectUndo(cameraObject, "Create Sdx17 Follow Camera");
            return cameraObject;
        }

        private static GameObject CreateDirectionalLight()
        {
            GameObject lightObject = new GameObject(LightObjectName);
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;

            Undo.RegisterCreatedObjectUndo(lightObject, "Create Sdx17 Directional Light");
            return lightObject;
        }
    }

    /// <summary>Outcome of a scene-helper run.</summary>
    public readonly struct SceneSetupResult
    {
        public SceneSetupResult(bool success, int createdCount, int existingCount, int expectedCount, string message)
        {
            Success = success;
            CreatedCount = createdCount;
            ExistingCount = existingCount;
            ExpectedCount = expectedCount;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }
        public int CreatedCount { get; }
        public int ExistingCount { get; }
        public int ExpectedCount { get; }
        public string Message { get; }

        public override string ToString()
        {
            return Message;
        }
    }
}

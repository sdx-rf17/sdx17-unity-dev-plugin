# Sdx17 Developer Plugin — Guide

> **Version 0.1.0 — UNTESTED IN UNITY**
>
> Nothing in this package has been compiled by Unity, opened in the editor,
> or run in Play Mode yet. No Unity editor was available in the environment
> where v0.1.0 was written. Do not treat anything in this guide as tested,
> working, or production-ready until the test checklist in `README.md` has
> been completed in Unity 2022.3 LTS and every step passes. The runtime
> constant `DevPluginInfo.UntestedInUnity` is `true` for this version.
>
> Package: `com.sdx17.devplugin` · Version `0.1.0` · MIT licence, Copyright 2026 Sdx17.
> Requirements: Unity **2022.3 LTS**. URP and Built-in compatible (the package
> uses no render-pipeline-specific code). No paid dependencies. No hard
> references to any other Sdx17 package.

---

## Contents

1. [Quickstart (5 minutes)](#1-quickstart-5-minutes)
2. [Concepts](#2-concepts)
3. [The Developer Hub](#3-the-developer-hub)
4. [API reference](#4-api-reference)
5. [Config reference (`DevPluginSettings`)](#5-config-reference-devpluginsettings)
6. [Integration with the other Sdx17 systems](#6-integration-with-the-other-sdx17-systems)
7. [Demo sample](#7-demo-sample)
8. [Troubleshooting / FAQ](#8-troubleshooting--faq)
9. [Performance notes](#9-performance-notes)
10. [Limitations](#10-limitations)

---

## 1. Quickstart (5 minutes)

### Step 1 — Install the package (1 minute)

**Option A — Package Manager (recommended):**

1. Open your Unity 2022.3 LTS project.
2. `Window > Package Manager > + > Add package from disk…`
3. Select this folder's `package.json`.

**Option B — `manifest.json`:**

Add this line to your project's `Packages/manifest.json` (adjust the
relative path to where you keep the package):

```json
"com.sdx17.devplugin": "file:../sdx17-unity-dev-plugin"
```

**Option C — Copy the folder:**

Copy the whole folder into your project under `Packages/` or `Assets/`.
Keep the `Editor/Templates/` folder with the package — the generators read
their template text files from it, and generation fails without them.

### Step 2 — Check it loaded (30 seconds)

Open the Console (`Window > General > Console`). There should be **no
compile errors**. The `Sdx17` menu should appear in the menu bar. If the
menu is missing, check the Console for compile errors first — see
[FAQ Q1](#q1-the-sdx17-menu-is-missing-what-do-i-do).

### Step 3 — Open the hub (30 seconds)

`Sdx17 > Developer Hub`. The window has five sections: **Validate**,
**Generate**, **Scene**, **Build & Audit**, and **Settings**. Every button
calls the real tool behind it and shows the last result at the bottom of
the window.

### Step 4 — Validate your scene (1 minute)

With any scene open, click **Validate Open Scene** in the hub (or
`Sdx17 > Validate > Open Scene`). A clean empty scene should report no
errors. Findings are also written to the Console when `Log To Console` is
on (it is, by default).

### Step 5 — Create a starter setup (1 minute)

Click **Create Player / Camera / Light Setup** in the hub (or
`Sdx17 > Scene > Create Player / Camera / Light Setup`). This adds three
GameObjects to the open scene:

- `Sdx17_Player` — capsule at `(0, 1, 0)` with a `CharacterController`
  (height 2, radius 0.5; the capsule primitive's own `CapsuleCollider` is
  removed so it does not fight the `CharacterController`).
- `Sdx17_MainCamera` — camera at `(0, 4, -6)`, field of view 60, tagged
  `MainCamera`, with an `AudioListener`, looking at the player.
- `Sdx17_DirectionalLight` — white directional light, intensity 1, soft
  shadows, rotated `(50, -30, 0)`.

Running it a second time creates nothing new — it reports that the setup
already exists.

### Step 6 — (Optional) Create a settings asset (1 minute)

Click **Create / Select Settings Asset** in the hub. This creates a
`DevPluginSettings` asset at `Assets/DevPluginSettings.asset` (or selects
the existing one). Without a settings asset, every tool uses the built-in
defaults and everything still works. See
[Section 5](#5-config-reference-devpluginsettings) for every option.

That is the whole quickstart. The remaining sections explain each tool in
depth.

---

## 2. Concepts

### Editor-only, by design

Almost everything in this package lives in `Editor/` and is compiled into
the editor-only assembly `Sdx17.DevPlugin.Editor`. Editor code is stripped
from builds, so the validator, generators, scene helper, build helper and
audit add **zero bytes and zero runtime cost** to your game.

The only runtime code is `Runtime/DevPluginInfo.cs` — two tiny static
classes, `DevPluginInfo` and `DevPluginLogger` — compiled into
`Sdx17.DevPlugin.Runtime`. They exist so runtime code and the demo sample
can read the package version and log with the shared `[Sdx17]` prefix
without depending on any editor assembly.

### The validator never modifies anything

`AssetValidator` only reports. It never deletes, moves, renames, or fixes
an asset. Every finding is a `ValidationResult` with a `ValidationSeverity`
(`Info`, `Warning`, or `Error`), a `Target` (what was checked), and a
`Message` (what is wrong). A run is summarised in a `ValidationReport`
with `ErrorCount`, `WarningCount`, `InfoCount`, `CheckedObjectCount`, and
`HasErrors`.

### Generators work by reflection, not by reference

`CodeGenerators` never references another Sdx17 package at compile time.
When you ask for an `ItemDefinition`, it searches every loaded assembly
for a `ScriptableObject` type with that short name, preferring the
`Sdx17.Inventory` namespace. If the real type exists (because the
Inventory package is installed), a real asset of that type is created. If
it does not exist, a standalone definition script is written from the
template in `Editor/Templates/`; after Unity compiles it, running the same
menu item again creates the asset from the new type. The two-step flow is
intentional — see [FAQ Q4](#q4-i-ran-generate--item-definition-and-got-a-script-not-an-asset-why).

### Settings are optional, with safe defaults

`DevPluginSettings` is a `ScriptableObject`. Every tool reads it through
`DevPluginSettings.LoadOrCreate()`: if a settings asset exists anywhere in
the project, it is used; otherwise a new in-memory instance carrying the
built-in defaults is returned (and never saved). Editing that in-memory
instance has no effect — create a real asset to persist changes.

### Idempotency

The scene helper is idempotent: running `CreateStarterSetup()` twice
never duplicates the player, camera, or light. It returns a
`SceneSetupResult` telling you how many objects were created
(`CreatedCount`), how many already existed (`ExistingCount`), and out of
how many expected (`ExpectedCount`, always 3).

---

## 3. The Developer Hub

`Sdx17 > Developer Hub` opens `DeveloperHubWindow` (class
`DeveloperHubWindow : EditorWindow`, namespace `Sdx17.DevPlugin`). The
window is a launcher, not a second copy of the logic: every button calls
the same public method the matching menu item calls, and the last result
is shown at the bottom of the window in the **Last result** field.

| Section | Button | What it calls |
| --- | --- | --- |
| Validate | Validate Open Scene | `AssetValidator.ValidateOpenScene()` |
| Validate | Validate Selected Assets | `AssetValidator.ValidateSelectedAssets()` |
| Validate | Validate Project Scripts | `AssetValidator.ValidateProjectScripts()` |
| Generate | Generate Item Definition | `CodeGenerators.GenerateDefinition("ItemDefinition", "Sdx17.Inventory", "Editor/Templates/ItemDefinitionTemplate.txt")` |
| Generate | Generate Quest Definition | `CodeGenerators.GenerateDefinition("QuestDefinition", "Sdx17.Quest", "Editor/Templates/QuestDefinitionTemplate.txt")` |
| Generate | Generate Dialogue Graph | `CodeGenerators.GenerateDefinition("DialogueGraph", "Sdx17.Dialogue", "Editor/Templates/DialogueGraphTemplate.txt")` |
| Scene | Create Player / Camera / Light Setup | `SceneHelper.CreateStarterSetup()` |
| Scene | (status line) | `SceneHelper.StarterSetupExists()` — "present" / "not present" |
| Build & Audit | Run Pre-Build Checklist | `BuildHelper.RunPreBuildChecklist()` |
| Build & Audit | Run Project Audit | `ProjectAuditReport.RunAudit()` + `ProjectAuditReport.WriteReport(report)` |
| Settings | Create / Select Settings Asset | Creates `Assets/DevPluginSettings.asset` if none exists, otherwise selects the existing asset |

The hub window's minimum size is 360 × 420. Its `lastResult` string and
scroll position are `[SerializeField]` fields, so they survive recompiles
within the session.

### Menu reference (complete)

| Menu item | Method |
| --- | --- |
| `Sdx17 > Developer Hub` | `DeveloperHubWindow.Open()` |
| `Sdx17 > Validate > Open Scene` | `AssetValidator.ValidateOpenSceneMenu()` |
| `Sdx17 > Validate > Selected Assets` | `AssetValidator.ValidateSelectedAssetsMenu()` |
| `Sdx17 > Validate > Whole Project (Scripts Only)` | `AssetValidator.ValidateProjectScriptsMenu()` |
| `Sdx17 > Generate > Item Definition` | `CodeGenerators.GenerateItemDefinitionMenu()` |
| `Sdx17 > Generate > Quest Definition` | `CodeGenerators.GenerateQuestDefinitionMenu()` |
| `Sdx17 > Generate > Dialogue Graph` | `CodeGenerators.GenerateDialogueGraphMenu()` |
| `Sdx17 > Scene > Create Player / Camera / Light Setup` | `SceneHelper.CreateStarterSetupMenu()` |
| `Sdx17 > Scene > Select Player` | `SceneHelper.SelectPlayerMenu()` |
| `Sdx17 > Build > Run Pre-Build Checklist` | `BuildHelper.RunChecklistMenu()` |
| `Sdx17 > Audit > Run Project Audit` | `ProjectAuditReport.RunAuditMenu()` |
| `Sdx17 > Tests > Run Developer Plugin Checklist` | `DevPluginChecklist` (see [Section 4.8](#48-test-checklist-testsdevpluginchecklistcs)) |
| `Assets > Create > Sdx17 > Dev Plugin Settings` | `DevPluginSettings` (`[CreateAssetMenu]`) |

---

## 4. API reference

All public types live in namespace `Sdx17.DevPlugin`, except the demo
sample (`Sdx17.DevPlugin.Samples`). All editor types are in the
`Sdx17.DevPlugin.Editor` assembly; the two runtime types are in
`Sdx17.DevPlugin.Runtime`. Every class and method listed below was read
from the actual source files in `Editor/` and `Runtime/` — nothing here is
invented. Private helpers (for example
`AssetValidator.CheckSerializedProperty`, `BuildHelper.CheckScenesInBuild`,
`SceneHelper.CreatePlayer`) are implementation details and are not part of
the public API.

### 4.1 Validation — `AssetValidator` and `ValidationResult.cs`

File: `Editor/AssetValidator.cs` (static class `AssetValidator`).
File: `Editor/ValidationResult.cs` (`ValidationSeverity`,
`ValidationResult`, `ValidationReport`).

#### `ValidationSeverity` (enum)

```csharp
public enum ValidationSeverity { Info, Warning, Error }
```

#### `ValidationResult` (readonly struct)

One finding from the validator.

```csharp
public ValidationResult(ValidationSeverity severity, string target, string message)
public ValidationSeverity Severity { get; }
public string Target { get; }
public string Message { get; }
public override string ToString() // "[Error] Player: Missing script on component."
```

`Target` and `Message` are never null — the constructor replaces null
with `string.Empty`.

#### `ValidationReport` (sealed class)

Summary of a validation run.

```csharp
public IReadOnlyList<ValidationResult> Results { get; }
public int ErrorCount { get; }
public int WarningCount { get; }
public int InfoCount { get; }
public int CheckedObjectCount { get; set; }
public bool HasErrors { get; }            // ErrorCount > 0
public void Add(ValidationResult result)  // increments the matching counter
public void Add(ValidationSeverity severity, string target, string message)
public string SummaryLine()               // "Checked 12 object(s): 1 error(s), 2 warning(s), 0 info note(s)."
```

#### `AssetValidator` (public static class)

```csharp
// Menu entry points (also callable from your own editor code):
public static void ValidateOpenSceneMenu()        // Sdx17/Validate/Open Scene
public static void ValidateSelectedAssetsMenu()   // Sdx17/Validate/Selected Assets
public static void ValidateProjectScriptsMenu()   // Sdx17/Validate/Whole Project (Scripts Only)

// Core checks — each returns a ValidationReport and modifies nothing:
public static ValidationReport ValidateOpenScene()
public static ValidationReport ValidateSelectedAssets()
public static ValidationReport ValidateProjectScripts()

// Lower-level checks, reusable from your own editor tools:
public static void ValidateGameObjectTree(GameObject root, DevPluginSettings settings, ValidationReport report)
public static void CheckSerializedFields(UnityEngine.Object target, DevPluginSettings settings, ValidationReport report)

// Helpers:
public static bool IsPascalCase(string value)
public static string GetHierarchyPath(GameObject gameObject)
public static void LogReport(ValidationReport report, string title)
```

What each check does:

- **`ValidateOpenScene()`** — validates every root `GameObject` of the
  active scene (via `ValidateGameObjectTree`), and adds an `Info` result
  naming the scene and its root count. If no valid scene is open, it
  returns a report with one `Error` ("No valid scene is open.").
- **`ValidateSelectedAssets()`** — validates whatever is selected in the
  Project window. A selected `GameObject` (prefab) is walked as a tree; a
  selected `ScriptableObject` has its serialized fields checked; every
  selected asset has its name checked against the naming rules. With
  nothing selected, it returns a report with one `Warning` telling you to
  select something first.
- **`ValidateProjectScripts()`** — scans every `MonoScript` in the project
  (`AssetDatabase.FindAssets("t:MonoScript")`). A script whose
  `MonoScript.GetClass()` returns null is an `Error` ("Script has no
  matching class (class name and file name may differ, or the file does
  not compile)."). This is the cheapest whole-project check and the one
  most worth running before a build. If no `MonoScript` assets exist at
  all, it reports a `Warning`.
- **`ValidateGameObjectTree(root, settings, report)`** — walks a
  `GameObject` and all its children, checking every component for missing
  scripts and, via `CheckSerializedFields`, for null references and empty
  required fields. Does nothing if `root`, `settings`, or `report` is null.
- **`CheckSerializedFields(target, settings, report)`** — inspects a
  `UnityEngine.Object` through `SerializedObject`: null Object-reference
  fields are `Warning`s when `CheckMissingReferences` is on; blank string
  fields named like `id`, `name`, `key`, `title`, or `display` are
  `Warning`s when `CheckEmptyRequiredFields` is on.
- **`IsPascalCase(value)`** — returns `true` when the string starts with
  an uppercase letter and contains no spaces or underscores (the exact
  rule used by the naming check). Example from the README:
  `AssetValidator.IsPascalCase("PlayerHealth")` returns `true`.
- **`GetHierarchyPath(gameObject)`** — returns the slash-separated scene
  path of a `GameObject` (for example `Player/Weapon/Sword`), used in
  finding messages so you can locate the object.
- **`LogReport(report, title)`** — writes the report to the Console:
  errors via `Debug.LogError`, warnings via `Debug.LogWarning`, and the
  `SummaryLine()` as a final line. Only called when the settings say so —
  see `LogToConsole` in [Section 5](#5-config-reference-devpluginsettings).

Example:

```csharp
using Sdx17.DevPlugin;

ValidationReport report = AssetValidator.ValidateOpenScene();
AssetValidator.LogReport(report, "Open scene validation");
if (report.HasErrors)
    Debug.LogError($"Scene has {report.ErrorCount} error(s).");
Debug.Log(report.SummaryLine());

ValidationReport scripts = AssetValidator.ValidateProjectScripts();
bool pascal = AssetValidator.IsPascalCase("PlayerHealth"); // true
```

### 4.2 Code generators — `CodeGenerators`

File: `Editor/CodeGenerators.cs` (public static class `CodeGenerators`).

```csharp
// Menu entry points:
public static void GenerateItemDefinitionMenu()     // Sdx17/Generate/Item Definition
public static void GenerateQuestDefinitionMenu()    // Sdx17/Generate/Quest Definition
public static void GenerateDialogueGraphMenu()      // Sdx17/Generate/Dialogue Graph

// Core API:
public static string GenerateDefinition(string typeName, string preferredNamespace, string templateRelativePath)
public static Type FindScriptableObjectType(string typeName, string preferredNamespace)
public static string WriteScriptFromTemplate(string typeName, string targetNamespace,
    string templateRelativePath, string folder)
public static ScriptableObject CreateAssetFromTypeName(string typeName, string preferredNamespace, string assetPath)
public static string GetSelectedFolderOrAssetsRoot()
public static string GetPackageRoot()
```

- **`GenerateDefinition(typeName, preferredNamespace, templateRelativePath)`**
  — the single entry point used by all three menu items and the hub. It
  builds an asset name from the settings prefix plus the type name
  (`Sdx17_ItemDefinition_New` by default), picks a unique path in the
  currently selected folder (or `Assets/`), and then:
  1. If `FindScriptableObjectType` finds the type, creates a real
     `ScriptableObject` asset of that type, selects it, pings it, logs
     `[Sdx17] Created …`, and returns the **asset path**.
  2. If not, writes a standalone script from the template and returns the
     **script path**, logging that you should wait for Unity to compile and
     run the same menu item again.
  Returns `string.Empty` when generation could not complete (for example
  an empty type name, or a missing template file).
- **`FindScriptableObjectType(typeName, preferredNamespace)`** — searches
  every loaded assembly (`AppDomain.CurrentDomain.GetAssemblies()`) for a
  non-abstract `ScriptableObject` type whose short name matches. If several
  match, it prefers one whose namespace starts with `preferredNamespace`;
  otherwise it returns the first match. Returns `null` when nothing
  matches. Assemblies that only partially load are handled via
  `ReflectionTypeLoadException.Types`. Also useful in your own tools.
- **`WriteScriptFromTemplate(typeName, targetNamespace, templateRelativePath, folder)`**
  — reads the template text file under the package root, replaces the
  `{TYPE_NAME}` and `{NAMESPACE}` tokens (empty namespace falls back to
  `Sdx17.Generated`), writes `<typeName>.cs` into `folder`, imports it, and
  returns the script path — or `string.Empty` on failure (missing template
  file logs an error telling you the `Editor/Templates` folder must be
  kept with the package).
- **`CreateAssetFromTypeName(typeName, preferredNamespace, assetPath)`** —
  creates a `ScriptableObject` asset of a type found by reflection, without
  a compile-time reference to it. Used by the hub window and by tests.
  Returns the created instance, or `null` when the type is not found or
  `assetPath` is empty.
- **`GetSelectedFolderOrAssetsRoot()`** — folder of the selected asset in
  the Project window, or `Assets/` when nothing (or a non-folder asset's
  parent folder) is selected.
- **`GetPackageRoot()`** — locates the package root by finding this
  package's `package.json`: it checks `Packages/com.sdx17.devplugin`,
  `Assets/Sdx17DevPlugin`, and `Assets/Sdx17/DevPlugin`, then falls back to
  searching the project for a `package.json` containing
  `com.sdx17.devplugin`, and finally to `Assets/`.

The three built-in generator calls, exactly as wired in the code:

```csharp
CodeGenerators.GenerateDefinition("ItemDefinition", "Sdx17.Inventory", "Editor/Templates/ItemDefinitionTemplate.txt");
CodeGenerators.GenerateDefinition("QuestDefinition", "Sdx17.Quest", "Editor/Templates/QuestDefinitionTemplate.txt");
CodeGenerators.GenerateDefinition("DialogueGraph", "Sdx17.Dialogue", "Editor/Templates/DialogueGraphTemplate.txt");
```

Templates: `Editor/Templates/ItemDefinitionTemplate.txt`,
`Editor/Templates/QuestDefinitionTemplate.txt`,
`Editor/Templates/DialogueGraphTemplate.txt`. Each is a standalone
definition script with `{TYPE_NAME}` and `{NAMESPACE}` template tokens.

### 4.3 Scene helper — `SceneHelper` and `SceneSetupResult`

File: `Editor/SceneHelper.cs`.

```csharp
// Object names created in the scene (public constants — use them, don't hard-code strings):
public const string PlayerObjectName = "Sdx17_Player";
public const string CameraObjectName = "Sdx17_MainCamera";
public const string LightObjectName = "Sdx17_DirectionalLight";

// Menu entry points:
public static void CreateStarterSetupMenu()  // Sdx17/Scene/Create Player / Camera / Light Setup
public static void SelectPlayerMenu()        // Sdx17/Scene/Select Player

// Core API:
public static SceneSetupResult CreateStarterSetup()
public static bool StarterSetupExists()
```

- **`CreateStarterSetup()`** — creates whichever of the three objects is
  missing in the open scene (player capsule with `CharacterController`,
  follow camera, directional light — see [Quickstart step 5](#step-5--create-a-starter-setup-1-minute)
  for exact values), marks the scene dirty with
  `EditorSceneManager.MarkSceneDirty`, and returns a `SceneSetupResult`.
  If no valid scene is open, returns a failed result ("No valid scene is
  open."). If everything already exists, returns success with
  `CreatedCount == 0` and the message "Starter setup already exists;
  nothing was duplicated." Every created object is registered with
  `Undo.RegisterCreatedObjectUndo`, so creation is undoable.
- **`StarterSetupExists()`** — `true` only when all three objects exist in
  the open scene (checked with `GameObject.Find` on each constant name).
- **`SelectPlayerMenu()`** — selects and pings `Sdx17_Player` in the
  Hierarchy; if there is no player yet, it logs a warning telling you to
  run the setup first.

#### `SceneSetupResult` (readonly struct)

```csharp
public SceneSetupResult(bool success, int createdCount, int existingCount, int expectedCount, string message)
public bool Success { get; }
public int CreatedCount { get; }
public int ExistingCount { get; }
public int ExpectedCount { get; }   // always 3
public string Message { get; }
public override string ToString()   // returns Message
```

Example:

```csharp
using Sdx17.DevPlugin;

SceneSetupResult setup = SceneHelper.CreateStarterSetup(); // idempotent: never duplicates
Debug.Log(setup.Message); // "Created 3 object(s), 0 already existed."
bool exists = SceneHelper.StarterSetupExists();
```

### 4.4 Build helper — `BuildHelper`, `BuildChecklistItem`, `BuildChecklistReport`

File: `Editor/BuildHelper.cs`. It never builds anything itself — it checks
the conditions that most often make a build fail or ship broken, so you
can fix the list before opening `File > Build Settings`.

```csharp
// Menu entry point:
public static void RunChecklistMenu()  // Sdx17/Build/Run Pre-Build Checklist

// Core API:
public static BuildChecklistReport RunPreBuildChecklist()
public static void LogChecklist(BuildChecklistReport report)
```

`RunPreBuildChecklist()` runs five checks, in this order:

1. **Build scenes** — fails if no scenes are listed in Build Settings, or
   if none of the listed scenes is enabled.
2. **Company name** — fails if `PlayerSettings.companyName` is empty or
   still `DefaultCompany`.
3. **Product name** — fails if `PlayerSettings.productName` is empty or
   still `New Unity Project`.
4. **Scripting backend** — always passes, but reports the backend for the
   Standalone target: IL2CPP is noted as required for most store builds;
   any other backend (for example Mono) is noted as fine for local testing.
5. **Missing scripts** — runs `AssetValidator.ValidateOpenScene()`; fails
   if the scene validation has errors.
6. **Texture compression** — always passes; reports the current
   `EditorUserBuildSettings.overrideTextureCompression` value and reminds
   you to review large textures in the audit report. (The code comments
   say this check is intentionally light: it does not inspect each texture.)

`LogChecklist(report)` writes one summary line (`ALL CHECKS PASSED` or
`N check(s) FAILED`, with passed/failed counts) and then one `PASS` /
`FAIL` line per item. It does nothing when `report` is null.

#### `BuildChecklistItem` (readonly struct)

```csharp
public BuildChecklistItem(bool passed, string name, string message)
public bool Passed { get; }
public string Name { get; }
public string Message { get; }
```

#### `BuildChecklistReport` (sealed class)

```csharp
public IReadOnlyList<BuildChecklistItem> Items { get; }
public int PassedCount { get; }
public int FailedCount { get; }
public bool AllPassed { get; }  // FailedCount == 0 && Items.Count > 0
public void Add(bool passed, string name, string message)
```

Example:

```csharp
using Sdx17.DevPlugin;

BuildChecklistReport checklist = BuildHelper.RunPreBuildChecklist();
BuildHelper.LogChecklist(checklist);
if (!checklist.AllPassed)
    Debug.LogWarning($"{checklist.FailedCount} pre-build check(s) failed.");
```

### 4.5 Project audit — `ProjectAuditReport`, `AuditReport`, `AssetSizeEntry`

File: `Editor/ProjectAuditReport.cs`. The audit is data only — it never
deletes, moves, or renames anything.

```csharp
// Menu entry point:
public static void RunAuditMenu()  // Sdx17/Audit/Run Project Audit

// Core API:
public static AuditReport RunAudit()
public static string WriteReport(AuditReport report)  // returns the path written to
public static long GetFileSizeBytes(string projectPath)
public static string FormatBytes(long bytes)          // e.g. FormatBytes(2048) == "2.0 KB"
```

- **`RunAudit()`** — scans the whole project
  (`AssetDatabase.FindAssets(string.Empty)`). For every asset it counts
  the file extension (case-insensitive) in `CountByExtension`, counts
  folders, sums file sizes into `TotalBytes`, records assets at or above
  `DevPluginSettings.LargeAssetWarningBytes` in `LargeAssets`, and tracks
  duplicate file names (same name without extension, case-insensitive) in
  `DuplicateNames`. The 10 largest assets, largest first, go into
  `LargestAssets`.
- **`WriteReport(report)`** — writes the audit as Markdown to the path in
  settings (`AuditReportPath`, default `Assets/Sdx17AuditReport.md`) and
  returns that path; returns `string.Empty` when `report` is null. The
  report contains a `Generated:` timestamp, total assets, folders, total
  size, large-asset threshold, assets by extension, the largest assets,
  and duplicate names.
- **`GetFileSizeBytes(projectPath)`** — file size in bytes for a
  project-relative path (0 when the file cannot be read).
- **`FormatBytes(bytes)`** — human-readable size, for example `2048` →
  `"2.0 KB"`.

#### `AssetSizeEntry` (readonly struct)

```csharp
public AssetSizeEntry(string path, long sizeBytes)
public string Path { get; }
public long SizeBytes { get; }
```

#### `AuditReport` (sealed class)

```csharp
public AuditReport()  // initialises the collections
public int TotalAssets { get; set; }
public int FolderCount { get; set; }
public long TotalBytes { get; set; }
public long LargeAssetThresholdBytes { get; set; }
public Dictionary<string, int> CountByExtension { get; }
public List<AssetSizeEntry> LargestAssets { get; }
public List<AssetSizeEntry> LargeAssets { get; }
public List<string> DuplicateNames { get; }
public string SummaryLine()
```

Example:

```csharp
using Sdx17.DevPlugin;

AuditReport audit = ProjectAuditReport.RunAudit();
string reportPath = ProjectAuditReport.WriteReport(audit); // Markdown
string size = ProjectAuditReport.FormatBytes(2048);        // "2.0 KB"
Debug.Log(audit.SummaryLine());
```

### 4.6 Settings — `DevPluginSettings`

File: `Editor/DevPluginSettings.cs`. Full field reference is in
[Section 5](#5-config-reference-devpluginsettings); the API surface is:

```csharp
[CreateAssetMenu(fileName = "DevPluginSettings", menuName = "Sdx17/Dev Plugin Settings")]
public class DevPluginSettings : ScriptableObject
{
    public string AssetNamePrefix { get; }        // read-only property
    public bool RequirePascalCaseNames { get; }
    public bool CheckMissingScripts { get; }
    public bool CheckMissingReferences { get; }
    public bool CheckEmptyRequiredFields { get; }
    public string AuditReportPath { get; }
    public int LargeAssetWarningBytes { get; }
    public bool LogToConsole { get; }

    public static DevPluginSettings LoadOrCreate()
    public void ResetToDefaults()
}
```

All eight backing fields are `private` with `[SerializeField]` — edit
them in the Inspector on the asset, not from code. `LoadOrCreate()`
returns the project asset when one exists (found with
`AssetDatabase.FindAssets("t:DevPluginSettings")`), otherwise an
in-memory instance carrying the defaults. `ResetToDefaults()` restores
the built-in defaults on the instance it is called on.

### 4.7 Developer Hub window — `DeveloperHubWindow`

File: `Editor/DeveloperHubWindow.cs`.

```csharp
public class DeveloperHubWindow : EditorWindow
{
    public static void Open()  // Sdx17/Developer Hub; window title "Sdx17 Developer Hub", min size 360 x 420
}
```

`OnGUI()` (private) draws the five sections listed in
[Section 3](#3-the-developer-hub) and keeps the last result in the
`[SerializeField] private string lastResult` field
("No tool has run yet in this session." initially). The private
`CreateOrSelectSettings()` helper creates `Assets/DevPluginSettings.asset`
when no settings asset exists and selects it, or selects the existing one.

### 4.8 Runtime — `DevPluginInfo` and `DevPluginLogger`

File: `Runtime/DevPluginInfo.cs` (namespace `Sdx17.DevPlugin`, assembly
`Sdx17.DevPlugin.Runtime`). The only runtime code in the package.

```csharp
public static class DevPluginInfo
{
    public const string Version = "0.1.0";              // matches package.json
    public const string PackageName = "com.sdx17.devplugin";
    public const bool UntestedInUnity = true;           // true for the untested 0.1.0 code
}

public static class DevPluginLogger
{
    public static void Log(object message)        // Debug.Log("[Sdx17] ...")
    public static void LogWarning(object message) // Debug.LogWarning("[Sdx17] ...")
    public static void LogError(object message)   // Debug.LogError("[Sdx17] ...")
}
```

Example:

```csharp
using Sdx17.DevPlugin;

Debug.Log(DevPluginInfo.Version);      // "0.1.0"
Debug.Log(DevPluginInfo.PackageName);  // "com.sdx17.devplugin"
DevPluginLogger.Log("Hello from runtime."); // "[Sdx17] Hello from runtime."
```

### 4.9 Test checklist — `Tests~/DevPluginChecklist.cs`

Not shipped in the package assembly. File `Tests~/DevPluginChecklist.cs`
(assembly `Sdx17.DevPlugin.Tests.Editor`, Editor platform only) adds the
menu item `Sdx17 > Tests > Run Developer Plugin Checklist`. It logs
`PASS` / `FAIL` for the core paths — settings load, generator reflection
lookup, validator on an empty scene, audit counting, and the scene-helper
guard — and finishes with a line reporting the number of failures
(expect `0 FAIL`). Run it after import as a smoke test; it is not a
substitute for the full test checklist in `README.md`.

### 4.10 Demo sample — `DemoSetup`

File `Samples~/Demo/DemoSetup.cs`, namespace `Sdx17.DevPlugin.Samples`,
class `DemoSetup : MonoBehaviour`. Attach it to an empty GameObject and
press Play: it logs the package version, checks for the starter scene
setup, and reports what it finds. It performs no editor work at runtime.

```csharp
public bool LogOnStart { get; }       // [SerializeField] private bool logOnStart = true
public string DemoSceneName { get; }  // [SerializeField] private string demoSceneName = "DevPluginDemo"
// private void Start() — logs via DevPluginLogger; see Samples~/Demo/README.md
//   for the expected Console lines.
```

---

## 5. Config reference (`DevPluginSettings`)

Create the asset from the hub (`Create / Select Settings Asset`) or from
`Assets > Create > Sdx17 > Dev Plugin Settings`. All fields are private
with `[SerializeField]` and public read-only properties — edit them in the
Inspector. `ResetToDefaults()` restores the defaults below.

| Field (Inspector label) | Property | Default | Effect |
| --- | --- | --- | --- |
| Asset Name Prefix | `AssetNamePrefix` | `Sdx17_` | Prefix new generated assets must start with; the validator notes (does not fail) names without it. Empty = no rule. Used by `CodeGenerators.GenerateDefinition` when naming new assets (`Sdx17_ItemDefinition_New`). |
| Require PascalCase Names | `RequirePascalCaseNames` | `true` | Validator warns on asset names that are not PascalCase (checked by `AssetValidator.IsPascalCase`). |
| Check Missing Scripts | `CheckMissingScripts` | `true` | Validator reports missing-script components as errors. |
| Check Missing References | `CheckMissingReferences` | `true` | Validator warns on null Object-reference fields. |
| Check Empty Required Fields | `CheckEmptyRequiredFields` | `true` | Validator warns on blank `id` / `name` / `key` / `title` / `display` string fields. |
| Audit Report Path | `AuditReportPath` | `Assets/Sdx17AuditReport.md` | Where `ProjectAuditReport.WriteReport` writes the Markdown audit report. |
| Large Asset Warning Bytes | `LargeAssetWarningBytes` | `10485760` (10 MB) | Assets at or above this size are listed as large in the audit (`AuditReport.LargeAssets`). |
| Log To Console | `LogToConsole` | `true` | Validator writes its findings to the Console (via `AssetValidator.LogReport`). |

Notes:

- **No settings asset = defaults.** `LoadOrCreate()` returns an
  in-memory instance with these defaults. The plugin works with no setup
  at all.
- **Only one asset is used.** If several `DevPluginSettings` assets exist,
  `LoadOrCreate()` returns the first one found by
  `AssetDatabase.FindAssets("t:DevPluginSettings")`. Keep exactly one.
- **Changing settings is per-project.** The asset lives in your project,
  not in the package, so it is not overwritten when you update the package.
- **The in-memory fallback is not saved.** Editing the instance returned
  by `LoadOrCreate()` when no asset exists has no effect after a domain
  reload — create a real asset to persist changes.

---

## 6. Integration with the other Sdx17 systems

The plugin is designed to sit alongside the other Sdx17 packages and to
work with **none of them installed**. It compiles standalone and never
hard-references another package. There are three different kinds of
integration — do not confuse them:

### 6.1 Validation covers all 11 systems (generic, no type knowledge)

The validator inspects components and `ScriptableObject`s through
`SerializedObject`, so it works on assets from any Sdx17 package without
knowing their types. Run it after importing a new Sdx17 package to catch
missing scripts and broken references. This applies to all 11 systems:

| # | System | What the validator catches on its assets |
| --- | --- | --- |
| 1 | **Inventory** | Missing scripts on item prefabs; null item/icon references; empty `id` / `name` fields on `ItemDefinition` assets. |
| 2 | **Quest** | Missing scripts on quest objects; null objective/reward references; empty `id` / `title` fields on `QuestDefinition` assets. |
| 3 | **Dialogue** | Missing scripts on dialogue objects; null node/line references; empty `id` / `title` fields on `DialogueGraph` assets. |
| 4 | **Combat** | Missing scripts on weapon/ability objects; null damage/effect references; empty `id` / `name` fields. |
| 5 | **NPC** | Missing scripts on NPC prefabs; null dialogue/quest references; empty `id` / `name` fields. |
| 6 | **Secure Save** | Missing scripts on save-manager objects; null save-slot references; empty `key` fields. |
| 7 | **Matchmaking** | Missing scripts on lobby/match objects; null session references; empty `id` / `name` fields. |
| 8 | **Multiplayer** | Missing scripts on networked prefabs; null sync references; empty `id` / `name` fields. |
| 9 | **UI** | Missing scripts on UI prefabs; null button/panel references; empty `id` / `name` fields. |
| 10 | **Admin** | Missing scripts on admin-panel objects; null command/config references; empty `key` / `name` fields. |
| 11 | **Terrain Tools** | Missing scripts on terrain objects; null brush/layer references; empty `id` / `name` fields. |

The validator checks *structure* (scripts present, references filled,
required strings non-empty, names follow the rules). It does **not**
understand game logic: it cannot tell you whether a quest chain is
completable, whether an inventory item's stats are balanced, or whether a
dialogue graph has a dead end. Those checks belong to the systems
themselves.

### 6.2 Generation creates ScriptableObjects — for 3 systems only

`Sdx17 > Generate` creates definition assets for exactly three systems,
by reflection (see [Section 4.2](#42-code-generators--codegenerators)):

| Menu item | Type created | Preferred namespace | Needs the real package? |
| --- | --- | --- | --- |
| `Generate > Item Definition` | `ItemDefinition` | `Sdx17.Inventory` | No — standalone script from `ItemDefinitionTemplate.txt` if the type is absent. |
| `Generate > Quest Definition` | `QuestDefinition` | `Sdx17.Quest` | No — standalone script from `QuestDefinitionTemplate.txt` if the type is absent. |
| `Generate > Dialogue Graph` | `DialogueGraph` | `Sdx17.Dialogue` | No — standalone script from `DialogueGraphTemplate.txt` if the type is absent. |

**There is no generator for Combat, NPC, Secure Save, Matchmaking,
Multiplayer, UI, Admin, or Terrain Tools.** Those eight systems have no
`Generate` menu item in v0.1.0, and this guide does not claim otherwise.
If you need definition assets for them, create them from their own
packages (`Assets > Create` in that package), then use this plugin's
validator to check them. Adding generators for the other systems is a
possible future version, not a v0.1.0 feature.

When the real package *is* installed, the generator finds its type by
reflection and creates the asset from that type — check the asset's type
in the Inspector to confirm. When it is *not* installed, the standalone
script written from the template is a minimal definition in the preferred
namespace (or `Sdx17.Generated` when no namespace is given); it is a
starting point, not the full system.

### 6.3 Scene helper, build checklist, and audit are system-agnostic

- **Scene helper.** The player capsule, follow camera, and directional
  light are a neutral starting point for any Sdx17 game. Add Inventory,
  Quest, Dialogue, Combat, or NPC components to the player afterwards from
  their own packages. The helper knows nothing about those components and
  adds none of them.
- **Build checklist.** Checks the whole project: scenes in Build Settings,
  company/product names, scripting backend, missing scripts in the open
  scene, and the texture-compression override. It applies equally whether
  you have zero or eleven Sdx17 packages installed.
- **Project audit.** Counts and checks every asset and scene, including
  all Sdx17 packages, and lists the largest assets (often textures or
  audio from any package) so you know what to shrink before a build.

### 6.4 No version coupling

The plugin does not check other packages' versions and does not require
them to be at any particular version. `FindScriptableObjectType` matches
on type *name* (preferring a namespace), not on version. If two packages
both define a type called `ItemDefinition`, the one in the preferred
namespace wins; otherwise the first match found is used.

### 6.5 Suggested workflow across the Sdx17 suite

1. Install the Developer Plugin first, in a clean Unity 2022.3 project.
2. Run `Sdx17 > Scene > Create Player / Camera / Light Setup` for a
   neutral starting scene.
3. Install the Sdx17 system packages you need (Inventory, Quest, …).
4. Run `Sdx17 > Validate > Open Scene` and `Validate > Whole Project
   (Scripts Only)` after each import — catch missing scripts early.
5. Use `Sdx17 > Generate` to create `ItemDefinition`, `QuestDefinition`,
   and `DialogueGraph` assets; fill in their fields in the Inspector.
6. Before every build, run `Sdx17 > Build > Run Pre-Build Checklist`,
   then `Sdx17 > Audit > Run Project Audit` and shrink anything listed as
   large.
7. Keep one `DevPluginSettings` asset in the project so the naming and
   audit rules are the same for everyone on the team.

---

## 7. Demo sample

The package ships a demo sample in `Samples~/Demo/` (import it from the
Package Manager: select the package, expand **Samples**, import **Demo**).
**No `.unity` scene file is included** — you build the demo scene yourself
in about two minutes; the full steps are in `Samples~/Demo/README.md`.

1. Create a new scene (`File > New Scene > Basic (Built-in)`, or the URP
   equivalent) and save it as `DevPluginDemo`.
2. Create an empty GameObject named `Demo` and add the `DemoSetup`
   component (`Add Component > Demo Setup`, namespace
   `Sdx17.DevPlugin.Samples`).
3. Run `Sdx17 > Scene > Create Player / Camera / Light Setup`.
4. Press Play. Expected Console output:

```text
[Sdx17] Developer Plugin demo started. Package com.sdx17.devplugin v0.1.0.
[Sdx17] Found player object 'Sdx17_Player' at (0.0, 1.0, 0.0).
[Sdx17] Camera present: True. Light present: True.
```

If `Sdx17_Player` is missing, the demo logs a warning telling you to run
the scene setup first. The demo exercises the *runtime* helper only; the
editor tools are exercised from the `Sdx17` menu, not from the demo
script. After the demo, try: `Sdx17 > Validate > Open Scene` (the demo
scene should validate with no errors), `Sdx17 > Generate > Item
Definition`, and `Sdx17 > Audit > Run Project Audit`.

> Status: the demo steps have **not** been run in a real Unity editor yet
> (v0.1.0 is untested). Follow the test checklist in `README.md` and
> report any step that does not match.

---

## 8. Troubleshooting / FAQ

### Q1. The `Sdx17` menu is missing. What do I do?

Check the Console first (`Window > General > Console`). The menu is
created by `[MenuItem]` attributes in the editor scripts, so if any file
in `Editor/` fails to compile, the whole editor assembly fails and no
`Sdx17` menu appears. Fix the first compile error shown, wait for Unity
to recompile, and check again. Common causes: the package folder was
copied without its `Editor/` folder, or the project is not on Unity
2022.3 LTS. If there are no errors and the menu is still missing, close
and reopen the project, then reimport the package.

### Q2. I get compile errors after import. Is my Unity version wrong?

Most likely, yes. The package targets Unity **2022.3 LTS**
(`"unity": "2022.3"` in `package.json`). It uses only standard
`UnityEditor` / `UnityEngine` APIs, but it has never been compiled on any
other version — v0.1.0 is untested in Unity at all. Use Unity 2022.3 LTS
exactly, in a fresh project, and report the exact Unity version and the
first Console error if it still fails.

### Q3. The validator reports a missing-script error. What does that mean, and do I fix it in this plugin?

A missing-script error means a component in the scene or prefab points
to a script file Unity can no longer find — usually because the script
was renamed without renaming its file, moved, or deleted. The validator
only *reports* it; it never fixes or removes anything. Fix it in your
project: select the GameObject named in the finding (the message includes
its hierarchy path from `GetHierarchyPath`), remove the broken component
or reassign the correct script, then run the validator again. Do not
ignore missing-script errors before a build — they are the most common
cause of a broken player.

### Q4. I ran `Generate > Item Definition` and got a script, not an asset. Why?

That is the intended two-step flow when the Inventory package is **not**
installed. The generator first looks for an existing `ItemDefinition`
type by reflection. Finding none, it writes a standalone
`ItemDefinition.cs` from `Editor/Templates/ItemDefinitionTemplate.txt`
and returns the script path. Wait for Unity to finish compiling the new
script, then run `Sdx17 > Generate > Item Definition` again — this time
the type exists, and a real `.asset` file is created (named
`Sdx17_ItemDefinition_New` by default, in the selected folder or
`Assets/`). If the Inventory package *is* installed and you still get a
script, check that its `ItemDefinition` type is a non-abstract
`ScriptableObject` — `FindScriptableObjectType` skips abstract types and
non-`ScriptableObject` types.

### Q5. The generator says the template was not found. What happened?

The error means `CodeGenerators.WriteScriptFromTemplate` could not find
the template file under the package root (resolved by
`GetPackageRoot()`). The usual cause is installing the package by copying
only the `.cs` files and leaving `Editor/Templates/` behind. Reinstall
the full package folder, including `Editor/Templates/`, and try again.
Do not move the templates elsewhere — the generators resolve them relative
to the package root.

### Q6. My generated asset landed in the wrong folder. How do I control where assets go?

`GenerateDefinition` creates the asset next to the currently selected
folder in the Project window (`GetSelectedFolderOrAssetsRoot()`); when
nothing is selected, it uses `Assets/`. Select the target folder in the
Project window *before* running the `Generate` menu item. The same rule
applies to the standalone script written on the first run. There is no
setting for a default output folder in v0.1.0.

### Q7. The audit report is empty / was written to an unexpected path. Where is it?

The report path comes from `DevPluginSettings.AuditReportPath`
(default `Assets/Sdx17AuditReport.md`). If you changed that field, the
report is written to your custom path — check the Console line from
`RunAuditMenu` / the hub, which prints the exact path. If the report file
is empty, check that the audit actually ran: `RunAudit()` scans
`AssetDatabase.FindAssets(string.Empty)`, so in an empty project the
counts are legitimately zero. If `WriteReport` returns `string.Empty`,
the report argument was null — that should not happen from the menu or
hub; report it as a bug with your Unity version.

### Q8. The pre-build checklist says "Build scenes: No scenes are listed in Build Settings", but I have a scene open. Is that a bug?

No. An open scene is not automatically a build scene. Open
`File > Build Settings`, click **Add Open Scenes** (or drag scenes into
the list), and make sure at least one scene is enabled (ticked). Then run
the checklist again. The same applies to the company/product name checks:
they read `PlayerSettings`, not the scene — set them in
`Edit > Project Settings > Player`.

### Q9. Can I use this plugin in a Built-in Render Pipeline project, or only URP?

Both. The package uses no render-pipeline-specific code, so it is
compatible with URP and Built-in. It creates only a capsule, a camera,
and a directional light — all pipeline-agnostic. (This is stated in the
README; like everything else in v0.1.0, it has not been verified in a real
Unity editor yet.)

### Q10. Does the plugin work at runtime / in a build?

No — and that is by design. Everything except `DevPluginInfo` /
`DevPluginLogger` is editor-only and is stripped from builds. The only
runtime API is `DevPluginInfo.Version`, `DevPluginInfo.PackageName`,
`DevPluginInfo.UntestedInUnity`, and the three `DevPluginLogger` methods
(`Log`, `LogWarning`, `LogError`), which just prefix messages with
`[Sdx17]`. If you call any `Editor/` class from runtime code, your build
will fail to compile — keep editor calls in `#if UNITY_EDITOR` blocks or
in editor scripts.

### Q11. I changed a setting, but the validator still uses the old value. Why?

You probably edited the in-memory fallback instead of a real asset, or
you have more than one `DevPluginSettings` asset and the tools are
reading the other one. `LoadOrCreate()` returns the *first* asset found
by `FindAssets("t:DevPluginSettings")`; if no asset exists, it returns an
unsaved in-memory instance and edits to it are lost on domain reload.
Fix: use the hub's **Create / Select Settings Asset** button, which
creates exactly one asset at `Assets/DevPluginSettings.asset` (or selects
the existing one), edit *that* asset in the Inspector, and delete any
duplicate settings assets.

### Q12. The validator warns about my asset name not being PascalCase / not starting with `Sdx17_`. Is that an error?

No — both are warnings/notes, not errors. `RequirePascalCaseNames`
warns on non-PascalCase names, and the prefix rule is noted, not failed
(the validator does not fail a build over a name). If your project uses
different naming conventions, turn `RequirePascalCaseNames` off or clear
`AssetNamePrefix` (empty = no rule) in your `DevPluginSettings` asset.

---

## 9. Performance notes

- **Editor-only cost, zero runtime cost.** All tools except the two
  runtime helper classes run in the editor. Nothing in `Editor/` ships in
  a build.
- **Validator cost is proportional to what you check.**
  `ValidateOpenScene()` walks every `GameObject` in the active scene and
  inspects every component's serialized fields — fast on a starter scene,
  slower on a scene with thousands of objects. `ValidateProjectScripts()`
  is the cheapest whole-project check (one `MonoScript.GetClass()` call
  per script) and is the one to run most often. `ValidateSelectedAssets()`
  only checks what you selected, so use it for quick spot checks.
- **Audit cost is proportional to project size.** `RunAudit()` calls
  `AssetDatabase.FindAssets(string.Empty)` and stats every file on disk
  (`GetFileSizeBytes`). On a large project this takes a few seconds; it is
  a manual action, not something that runs automatically, so it never
  slows down editing or Play Mode. The audit sorts assets by size and
  keeps only the 10 largest in `LargestAssets`, so the report stays small
  no matter how big the project is.
- **Generators and the scene helper are one-shot actions.** Reflection
  (`FindScriptableObjectType` scans all loaded assemblies) happens once
  per generation, not per frame. The scene helper does three
  `GameObject.Find` calls per run.
- **No background work, no polling, no file watchers.** No tool in this
  package runs on a timer, subscribes to editor update loops, or watches
  the file system. Everything runs only when you click a menu item, a hub
  button, or call the API yourself.
- **Settings lookup is cheap but repeated.** Every tool call runs
  `DevPluginSettings.LoadOrCreate()`, which does an
  `AssetDatabase.FindAssets("t:DevPluginSettings")` query. That is fine
  for manual tool runs; do not call the validator in a tight loop from
  your own editor code — cache the `DevPluginSettings` instance and the
  `ValidationReport` instead.
- **Console logging.** With `LogToConsole` on (the default), every
  validation finding is a Console entry. On a very dirty scene that can
  mean hundreds of lines; turn `LogToConsole` off and read the
  `ValidationReport` (or the hub's **Last result** summary line) instead.

---

## 10. Limitations

Honest limits of v0.1.0. Read this before buying, installing, or relying
on the package.

1. **UNTESTED IN UNITY.** Nothing has been compiled by Unity, opened in
   the editor, or run in Play Mode. The 13-step test checklist in
   `README.md` must be completed in Unity 2022.3 LTS before any claim of
   "works", "tested", or "production-ready" is made. Until then, treat
   every feature in this guide as *designed and written, not verified*.
2. **Editor-only.** The plugin does not add gameplay. It contains no
   inventory, quest, dialogue, combat, NPC, save, matchmaking,
   multiplayer, UI, admin, or terrain logic — those live in their own
   Sdx17 packages. The only runtime code is `DevPluginInfo` /
   `DevPluginLogger`.
3. **Generators cover 3 systems, not 11.** Only `ItemDefinition`
   (Inventory), `QuestDefinition` (Quest), and `DialogueGraph` (Dialogue)
   have generators. There is no generator for Combat, NPC, Secure Save,
   Matchmaking, Multiplayer, UI, Admin, or Terrain Tools in v0.1.0.
4. **The validator checks structure, not game logic.** It finds missing
   scripts, null references, empty required strings, and naming breaks.
   It cannot validate quest flow, dialogue reachability, combat balance,
   save-data migration, or network correctness.
5. **The build checklist is a checklist, not a build.** It never builds
   anything. It checks five things (build scenes, company/product names,
   scripting backend, missing scripts, texture-compression override) and
   the texture check is intentionally light — it reports the override
   setting and does not inspect individual textures. A passing checklist
   does not guarantee a successful build.
6. **The audit never fixes anything.** It counts, lists, and writes a
   Markdown report. It never deletes, moves, or renames assets, and it
   does not compress textures or remove duplicates for you.
7. **Naming checks are heuristics.** `IsPascalCase` is a simple rule
   (uppercase first letter, no spaces or underscores). The "required
   field" check keys off field names (`id`, `name`, `key`, `title`,
   `display`) — a field with a different name that your game treats as
   required will not be checked, and a field with one of those names that
   is legitimately empty will warn anyway.
8. **One settings asset wins.** If several `DevPluginSettings` assets
   exist, tools use the first one found. Keep exactly one.
9. **Reflection matching is by name.** `FindScriptableObjectType` matches
   on type short name, preferring a namespace prefix. If two packages
   define the same type name, the preferred-namespace match wins;
   otherwise the first match found is used, which may not be the one you
   wanted.
10. **Scene helper creates a minimal setup, not a game.** A capsule, a
    follow camera, and a light. No movement script, no input handling, no
    ground, no UI. You add gameplay from the other Sdx17 packages (or
    your own code) afterwards.
11. **No automated tests ship in the package.** The editor checklist in
    `Tests~/` logs PASS/FAIL for the core paths, but it is a manual
    smoke test run from a menu item, not a test runner suite, and it has
    not been run in Unity either.
12. **Unity 2022.3 LTS only (as written).** The package targets 2022.3.
    Other Unity versions have not been tried, and compatibility with them
    is not claimed.

---

*Sdx17 Developer Plugin v0.1.0 — Guide. MIT licence, Copyright 2026 Sdx17.
See `README.md` for the package overview and the Unity test checklist,
`CHANGELOG.md` for version history, and `LISTING.md` for the Asset Store
listing draft. No Asset Store listing exists yet, and no sales or reviews
are claimed.*

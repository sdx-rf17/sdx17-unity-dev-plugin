# Sdx17 Developer Plugin

> **Version 0.1.0 — UNTESTED IN UNITY**
>
> No Unity editor was available in the environment where this package was
> written. Nothing here has been compiled by Unity, opened in the editor,
> or run in Play Mode yet. Do not treat it as tested, working or
> production-ready until the test checklist at the end of this file has
> been completed in Unity 2022.3 LTS and every step passes.

## What it is

An **editor-only** Unity package (UPM, `com.sdx17.devplugin`) that gives
Sdx17 projects one place for the repetitive development work:

- **Developer Hub** — one window (`Sdx17 > Developer Hub`) launching every
  tool below.
- **Asset validator** — missing scripts, missing references, empty required
  fields and naming-rule breaks in the open scene, selected assets or all
  project scripts.
- **Code generators** — create `ItemDefinition`, `QuestDefinition` and
  `DialogueGraph` ScriptableObject assets from the menu.
- **Scene helper** — one-click player / camera / light starter setup.
- **Build helper** — pre-build checklist (scenes, names, scripting backend,
  missing scripts) before you open Build Settings.
- **Project audit** — asset counts, largest assets, duplicate names, written
  as a Markdown report.

## What it is NOT

- Not a game system. It contains no inventory, quest, dialogue, combat or
  NPC logic — those live in their own Sdx17 packages.
- Not a runtime framework. The only runtime code is the small
  `DevPluginInfo` / `DevPluginLogger` helper; everything else is editor-only
  and is stripped from builds.
- Not a replacement for the other Sdx17 packages. It **validates** them and
  **generates their ScriptableObjects**, but it does not implement them.
- Not tested. See the header above and the checklist below.

## Setup (under 5 minutes)

1. **Requirements:** Unity **2022.3 LTS**, URP-compatible (the package
   uses no render-pipeline-specific code and has no paid dependencies).
2. **Install as a UPM package** (recommended):
   - `Window > Package Manager > + > Add package from disk…`
   - Select this folder's `package.json`.
   - Or add `"com.sdx17.devplugin": "file:../sdx17-unity-dev-plugin"` to
     your project's `Packages/manifest.json` (adjust the relative path).
3. **Or copy the folder** into your project under `Packages/` or `Assets/`.
   Keep the `Editor/Templates/` folder with the package — the generators
   read their template text files from it.
4. **Open the hub:** `Sdx17 > Developer Hub`. If the menu is missing, check
   the Console for compile errors first (see the checklist).
5. **Optional settings:** `Sdx17 > Developer Hub > Create / Select Settings
   Asset` creates a `DevPluginSettings` asset. Without one, the built-in
   defaults are used and everything still works.

## File table

| Path | What it is |
| --- | --- |
| `package.json` | UPM manifest (`com.sdx17.devplugin`, 0.1.0, Unity 2022.3) |
| `README.md` | This file |
| `CHANGELOG.md` | Version history |
| `LICENSE` | MIT, Copyright 2026 Sdx17 |
| `.gitignore` | Unity standard ignores; `.meta` files are kept |
| `Editor/Sdx17.DevPlugin.Editor.asmdef` | Editor assembly (Editor platform only) |
| `Editor/DeveloperHubWindow.cs` | Hub window: `Sdx17 > Developer Hub` |
| `Editor/AssetValidator.cs` | Scene / selection / project-script validation |
| `Editor/ValidationResult.cs` | `ValidationResult`, `ValidationReport` types |
| `Editor/CodeGenerators.cs` | `Sdx17 > Generate` — Item / Quest / Dialogue generators |
| `Editor/SceneHelper.cs` | Player / camera / light starter setup |
| `Editor/BuildHelper.cs` | Pre-build checklist |
| `Editor/ProjectAuditReport.cs` | Asset counts, largest assets, duplicate names, Markdown report |
| `Editor/DevPluginSettings.cs` | `DevPluginSettings` ScriptableObject (options) |
| `Editor/Templates/ItemDefinitionTemplate.txt` | Standalone ItemDefinition script template |
| `Editor/Templates/QuestDefinitionTemplate.txt` | Standalone QuestDefinition script template |
| `Editor/Templates/DialogueGraphTemplate.txt` | Standalone DialogueGraph script template |
| `Runtime/Sdx17.DevPlugin.Runtime.asmdef` | Runtime assembly |
| `Runtime/DevPluginInfo.cs` | `DevPluginInfo` (version) + `DevPluginLogger` (`[Sdx17]` prefix) |
| `Samples~/Demo/DemoSetup.cs` | Demo MonoBehaviour (logs version, checks starter setup) |
| `Samples~/Demo/README.md` | Demo scene setup instructions (no `.unity` file included) |
| `Tests~/DevPluginChecklist.cs` | Editor checklist logging PASS / FAIL per core path |
| `Tests~/Sdx17.DevPlugin.Tests.Editor.asmdef` | Test assembly (Editor platform only) |

## Config options (`DevPluginSettings`)

Create the asset from the hub window, or from
`Assets > Create > Sdx17 > Dev Plugin Settings`. All fields are private
with `[SerializeField]` and public read-only properties.

| Field | Default | Effect |
| --- | --- | --- |
| `Asset Name Prefix` | `Sdx17_` | Prefix new generated assets must start with; the validator notes (does not fail) names without it |
| `Require PascalCase Names` | `true` | Validator warns on asset names that are not PascalCase |
| `Check Missing Scripts` | `true` | Validator reports missing-script components as errors |
| `Check Missing References` | `true` | Validator warns on null Object-reference fields |
| `Check Empty Required Fields` | `true` | Validator warns on blank `id` / `name` / `key` / `title` / `display` string fields |
| `Audit Report Path` | `Assets/Sdx17AuditReport.md` | Where the audit report is written |
| `Large Asset Warning Bytes` | `10485760` (10 MB) | Assets at or above this size are listed as large |
| `Log To Console` | `true` | Validator writes its findings to the Console |

`DevPluginSettings.LoadOrCreate()` returns the project asset when one
exists, otherwise an in-memory instance carrying these defaults.

## Public API

```csharp
using Sdx17.DevPlugin;

// --- Validation ---
ValidationReport report = AssetValidator.ValidateOpenScene();
AssetValidator.LogReport(report, "Open scene validation");
// report.HasErrors, report.ErrorCount, report.WarningCount, report.SummaryLine()

ValidationReport selected = AssetValidator.ValidateSelectedAssets();
ValidationReport scripts = AssetValidator.ValidateProjectScripts();
bool pascal = AssetValidator.IsPascalCase("PlayerHealth"); // true

// --- Generators (no compile-time reference to other Sdx17 packages) ---
// Creates a real asset when a type with that name already exists in the
// project; otherwise writes a standalone script from Editor/Templates/
// and asks you to run the menu item again after Unity compiles it.
string path = CodeGenerators.GenerateDefinition(
    "ItemDefinition", "Sdx17.Inventory", "Editor/Templates/ItemDefinitionTemplate.txt");

// Reflection lookup used by the generators (also useful in your own tools):
System.Type type = CodeGenerators.FindScriptableObjectType("QuestDefinition", "Sdx17.Quest");

// --- Scene helper ---
SceneSetupResult setup = SceneHelper.CreateStarterSetup(); // idempotent: never duplicates
bool exists = SceneHelper.StarterSetupExists();

// --- Build helper ---
BuildChecklistReport checklist = BuildHelper.RunPreBuildChecklist();
BuildHelper.LogChecklist(checklist); // checklist.AllPassed

// --- Project audit ---
AuditReport audit = ProjectAuditReport.RunAudit();
string reportPath = ProjectAuditReport.WriteReport(audit); // Markdown
string size = ProjectAuditReport.FormatBytes(2048); // "2.0 KB"

// --- Runtime helper (the only runtime code) ---
DevPluginInfo.Version;      // "0.1.0"
DevPluginInfo.PackageName;  // "com.sdx17.devplugin"
DevPluginLogger.Log("Hello from runtime.");
```

Menu reference: `Sdx17 > Developer Hub`, `Sdx17 > Validate > …`,
`Sdx17 > Generate > …`, `Sdx17 > Scene > …`, `Sdx17 > Build > …`,
`Sdx17 > Audit > …`, `Sdx17 > Tests > Run Developer Plugin Checklist`.

## Integration notes — other Sdx17 systems

The plugin is designed to sit alongside the other Sdx17 packages and to
work with **none of them installed**:

- **Validation covers all Sdx17 packages.** The validator inspects
  components and ScriptableObjects through `SerializedObject`, so it works
  on Inventory items, Quest definitions, Dialogue graphs, NPC data and any
  other Sdx17 asset without knowing their types. Run it after importing a
  new Sdx17 package to catch missing scripts and broken references.
- **Generators create their ScriptableObjects.** `Sdx17 > Generate > Item
  Definition / Quest Definition / Dialogue Graph` first searches all loaded
  assemblies by reflection for a type with that name (preferring the
  `Sdx17.Inventory`, `Sdx17.Quest` or `Sdx17.Dialogue` namespace). If the
  real package is installed, the asset is created from its type. If it is
  not installed, a standalone definition script is written from the
  template in `Editor/Templates/`; run the same menu item again after Unity
  compiles it and the asset is created from the new type. The plugin
  therefore compiles standalone and never hard-references another package.
- **Scene helper is system-agnostic.** The player capsule, follow camera
  and directional light it creates are a neutral starting point for any
  Sdx17 game; add Inventory / Quest / Dialogue components to the player
  afterwards from their own packages.
- **Audit and build checklist are project-wide.** They count and check
  every asset and scene, including all Sdx17 packages, and the audit
  report lists the largest assets (often textures or audio from any
  package) so you know what to shrink before a build.
- **No version coupling.** The plugin does not check other packages'
  versions and does not require them to be at any particular version.

## TEST checklist for Unity (must be completed before any tested claim)

Run these in **Unity 2022.3 LTS**, in a fresh URP project, with the package
installed from disk as described above. Record the result of each step.

1. **Import:** no compile errors in the Console after import; the `Sdx17`
   menu appears in the menu bar.
2. **Hub:** `Sdx17 > Developer Hub` opens; the window shows all sections
   (Validate, Generate, Scene, Build & Audit, Settings).
3. **Validator — clean scene:** create an empty scene, run
   `Sdx17 > Validate > Open Scene`; expect no errors.
4. **Validator — missing script:** add a component, then delete its script
   file (or use a known broken prefab); run the validator and confirm it
   reports a missing-script error naming the GameObject.
5. **Validator — missing reference:** add a MonoBehaviour with a public
   `GameObject` field left null; confirm a missing-reference warning.
6. **Generator — standalone:** with no other Sdx17 package installed, run
   `Sdx17 > Generate > Item Definition`; confirm a script is created from
   the template, Unity compiles it, and running the menu item again
   creates a `.asset` file.
7. **Generator — with a real type:** if the Inventory package is installed,
   run the same menu item and confirm the asset is created from the
   Inventory `ItemDefinition` type (check the asset's type in the Inspector).
8. **Scene helper:** run `Sdx17 > Scene > Create Player / Camera / Light
   Setup` twice; confirm three objects exist after the first run and no
   duplicates after the second.
9. **Build checklist:** run `Sdx17 > Build > Run Pre-Build Checklist`;
   confirm every check logs PASS or FAIL with a readable reason.
10. **Audit:** run `Sdx17 > Audit > Run Project Audit`; confirm
    `Assets/Sdx17AuditReport.md` is created and its counts match the
    Project window.
11. **Checklist script:** run
    `Sdx17 > Tests > Run Developer Plugin Checklist`; confirm the final
    line reports `0 FAIL`.
12. **Demo sample:** follow `Samples~/Demo/README.md`; press Play and
    confirm the expected Console lines appear.
13. **Build:** run the pre-build checklist, then build the demo scene for
    Standalone; confirm the build succeeds and the player starts.

If any step fails, note the exact Unity version, the step number and the
Console error, and do not claim the package works until it is fixed and
the checklist is re-run.

## License

MIT — see `LICENSE`. Copyright 2026 Sdx17.

# Changelog

All notable changes to the Sdx17 Developer Plugin are documented here.
The format follows Keep a Changelog, and versioning follows Semantic Versioning.

## [0.1.0] - 2026-11-21

### Added

- Developer Hub editor window under `Sdx17 > Developer Hub`, with one-click
  access to every tool in the package.
- Asset validator: checks the open scene and selected assets for missing
  scripts, missing component references, empty required fields and naming
  rule breaks; results are logged and shown in the hub window.
- Code generators (menu: `Sdx17 > Generate`): `ItemDefinition`,
  `QuestDefinition` and `DialogueGraph`. Each generator first looks for an
  existing type with that name by reflection and creates a real
  ScriptableObject asset from it; when no such type exists in the project it
  writes a standalone definition script from the template text files in
  `Editor/Templates/`, so the plugin compiles and runs with no other Sdx17
  package installed.
- Scene helper: `Sdx17 > Scene > Create Player / Camera / Light Setup`
  builds a named player capsule, a follow camera and a directional light in
  the open scene, and refuses to duplicate a setup that already exists.
- Build helper: pre-build checklist (scene list, company/product names,
  scripting backend, compression, missing-script scan) with a clear
  pass / warning report before `File > Build Settings`.
- Project audit report: counts scripts, scenes, prefabs, materials, textures
  and audio clips, lists the largest assets and duplicate asset names, and
  writes a Markdown report to `Assets/Sdx17AuditReport.md`.
- `DevPluginSettings` ScriptableObject (`Sdx17 > Settings > Create Settings
  Asset`) for naming rules, validation toggles and the audit output path.
- Small runtime helper (`Sdx17.DevPlugin.Runtime`): `DevPluginInfo` exposes
  the package version at runtime and `DevPluginLogger` prefixes log lines
  with `[Sdx17]`.
- Demo sample (`Samples~/Demo/`): `DemoSetup` MonoBehaviour that wires the
  hub, validator and audit tools into a scene at runtime for a quick smoke
  check, plus setup instructions.
- Editor checklist (`Tests~/DevPluginChecklist.cs`): logs PASS / FAIL for the
  core paths (settings load, generator reflection lookup, validator on an
  empty scene, audit counting, scene-helper guard).

### Status

- v0.1.0 — **UNTESTED IN UNITY**. No Unity editor was available in the
  environment where this version was written, so nothing here has been
  compiled by Unity, opened in the editor, or run in Play Mode yet. See
  `README.md` for the test checklist that must be completed before any
  claim of working, tested or production-ready is made.

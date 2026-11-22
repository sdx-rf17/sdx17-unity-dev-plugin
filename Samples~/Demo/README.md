# Demo Sample — Sdx17 Developer Plugin

Minimal smoke-test sample for the Developer Plugin. **No `.unity` scene
file is included** — build the demo scene yourself in about two minutes
using the steps below.

## What the sample contains

- `DemoSetup.cs` — a MonoBehaviour that logs the package version on Play,
  checks for the starter scene setup, and reports what it finds. It does
  no editor work at runtime.

## Demo scene setup (do this in Unity)

1. Create a new scene: `File > New Scene > Basic (Built-in)` (or the URP
   equivalent if your project uses URP) and save it as `DevPluginDemo`.
2. Create an empty GameObject named `Demo`:
   `GameObject > Create Empty`, rename it to `Demo`.
3. Add the demo script to it: with `Demo` selected, `Add Component >
   Demo Setup` (namespace `Sdx17.DevPlugin.Samples`).
4. Create the starter setup from the plugin menu:
   `Sdx17 > Scene > Create Player / Camera / Light Setup`.
   This adds three GameObjects to the scene:
   - `Sdx17_Player` — capsule with a `CharacterController`
   - `Sdx17_MainCamera` — camera with an `AudioListener`, tagged `MainCamera`
   - `Sdx17_DirectionalLight` — soft-shadow directional light
5. Press Play. Expected Console output:
   - `[Sdx17] Developer Plugin demo started. Package com.sdx17.devplugin v0.1.0.`
   - `[Sdx17] Found player object 'Sdx17_Player' at (0.0, 1.0, 0.0).`
   - `[Sdx17] Camera present: True. Light present: True.`

## What to try next (editor tools)

- `Sdx17 > Developer Hub` — run the validator, the audit and the
  pre-build checklist from one window.
- `Sdx17 > Validate > Open Scene` — the demo scene should validate with
  no errors.
- `Sdx17 > Generate > Item Definition` — creates an item asset (or a
  standalone script first, then the asset on the second run).
- `Sdx17 > Audit > Run Project Audit` — writes `Assets/Sdx17AuditReport.md`.

## Status

v0.1.0 — **UNTESTED IN UNITY**. These steps have not been run in a real
Unity editor yet; follow the test checklist in the package `README.md`
and report any step that does not match.

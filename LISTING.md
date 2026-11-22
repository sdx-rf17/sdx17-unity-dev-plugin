# Asset Store Listing — DRAFT (not submitted)

> **Status: DRAFT ONLY. No Asset Store listing exists yet. This file is a
> listing draft for the Sdx17 Developer Plugin v0.1.0. Do not submit it
> until every item in `SELLING_REQUIREMENTS.md` has passed — v0.1.0 is
> UNTESTED IN UNITY, and the listing must not go live before the package
> has been verified in a clean Unity 2022.3 LTS project.**
>
> Publisher: Sdx17 · GitHub: `sdx-rf17` (https://github.com/sdx-rf17) ·
> Package: `com.sdx17.devplugin` · Version 0.1.0 · MIT licence.

---

## Title

**Sdx17 Developer Plugin**

## Short description (under 200 characters)

> One hub for Sdx17 Unity projects: validate assets, generate ScriptableObjects, set up scenes, check builds and audit your project — editor-only, no dependencies.

*(Character count: 161 — verify again in the publisher portal before submitting, as the portal counts characters its own way.)*

## Long description

**One hub for the repetitive development work in your Sdx17 Unity projects.**

The Sdx17 Developer Plugin is an editor-only toolkit that gives every Sdx17 project a single place for the jobs you repeat on every project: checking assets, creating definition files, setting up a starter scene, checking the project before a build, and seeing what is actually in it.

**What you get**

- **Developer Hub** — one window (`Sdx17 > Developer Hub`) that launches every tool below. No hunting through menus.
- **Asset validator** — finds missing scripts, missing (null) references, empty required fields (`id`, `name`, `key`, `title`, `display`), and naming-rule breaks in the open scene, the selected assets, or all project scripts. It only reports — it never changes your assets.
- **Code generators** — create `ItemDefinition`, `QuestDefinition`, and `DialogueGraph` ScriptableObject assets from `Sdx17 > Generate`. If the matching Sdx17 package is installed, the asset is created from its real type (found by reflection). If it is not installed, a standalone definition script is written from a template first; run the menu item again after Unity compiles it and the asset is created. No hard references to other packages.
- **Scene helper** — one click creates a player capsule with a `CharacterController`, a follow camera, and a directional light. Running it twice never duplicates anything.
- **Build helper** — a pre-build checklist (scenes in Build Settings, company/product names, scripting backend, missing scripts in the open scene, texture-compression override) to run before you open Build Settings.
- **Project audit** — counts your assets by type, lists the 10 largest assets and duplicate names, and writes a Markdown report (`Assets/Sdx17AuditReport.md`).
- **Settings** — a `DevPluginSettings` ScriptableObject for the naming prefix, PascalCase rule, validation toggles, audit path, large-asset threshold, and console logging. Everything works with the built-in defaults if you never create one.
- **Tiny runtime helper** — `DevPluginInfo` (package version) and `DevPluginLogger` (`[Sdx17]` log prefix) are the only runtime code. Everything else is editor-only and is stripped from builds.

**Built for the Sdx17 suite — and for projects without it**

The plugin validates assets from all Sdx17 systems (Inventory, Quest, Dialogue, Combat, NPC, Secure Save, Matchmaking, Multiplayer, UI, Admin, Terrain Tools) through Unity's serialization system, without knowing their types — so it works whether you have one Sdx17 package installed or all eleven. Generators currently cover three systems (Inventory, Quest, Dialogue); the other systems are validated, not generated. The plugin compiles and runs standalone, with none of the other packages installed.

**Honest status**

Version 0.1.0 is the first release. It was written and reviewed outside Unity and is being tested in Unity 2022.3 LTS now; the guide (`Documentation~/GUIDE.md`) and README describe exactly what is and is not verified. If you find a step that does not match the guide, report it — the listing will be updated with the fix.

## Keywords (10)

1. `developer tools`
2. `editor tools`
3. `validation`
4. `scriptableobject`
5. `code generator`
6. `project audit`
7. `build checklist`
8. `scene setup`
9. `workflow`
10. `sdx17`

## Category

**Tools / Utilities** (suggested — re-check the category list in the publisher portal at listing time; Unity changes categories, and the closest current match is what matters, not this draft's guess).

## Suggested price

**$25**

### Why $25 (and not $95)

- **New publisher, zero reviews.** Nobody has bought from Sdx17 on the Asset Store yet, and there are no reviews, no ratings, and no sales history to justify a premium price. A first-time buyer takes a chance on an unknown publisher; the price has to reflect that.
- **The $15–$35 first-reviews band.** For a new publisher, pricing in the $15–$35 range is the realistic way to get the first buyers and the first reviews. $25 sits in the middle of that band: low enough to be an easy first purchase, high enough not to signal "throwaway".
- **What the package is.** This is a focused editor-utility package (validator, generators, scene helper, checklist, audit), not a full game framework. Comparable small editor tools on the Asset Store cluster in the $10–$40 range; large frameworks with gameplay systems charge far more, and this package deliberately is not one of those — it contains no inventory, quest, dialogue, combat, or NPC gameplay logic.
- **$95 on day one would be a mistake.** At $95 with zero reviews, the package would sit unsold while cheaper, reviewed alternatives get the clicks. Price is easy to raise later, after reviews and a tested version exist; it is hard to recover from a launch that nobody buys.
- **Revenue math (before tax and refunds).** Unity's standard publisher share is 70% of the sale price, so at $25 the publisher receives **$17.50 per sale** before tax and refunds. That is the honest number — do not quote $25 as what Sdx17 keeps.

**This price is a suggestion, not a promise.** It does not guarantee sales, reviews, or any income. Re-check comparable Asset Store listings and the current publisher terms before submitting, and re-check the price again at listing time. If the package gains verified testing, more generators, and real reviews, a higher price can be considered for a later version — never retroactively.

## What's included

- Full package source code (C#): 8 editor scripts, 1 runtime script, editor and runtime assembly definitions.
  - Editor: `DeveloperHubWindow.cs`, `AssetValidator.cs`, `ValidationResult.cs`, `CodeGenerators.cs`, `SceneHelper.cs`, `BuildHelper.cs`, `ProjectAuditReport.cs`, `DevPluginSettings.cs`, plus 3 generator templates in `Editor/Templates/`.
  - Runtime: `DevPluginInfo.cs` (`DevPluginInfo`, `DevPluginLogger`).
- `package.json` — UPM manifest (`com.sdx17.devplugin`, version 0.1.0, Unity 2022.3).
- `README.md` — overview, setup, config table, public API, integration notes, and the 13-step Unity test checklist.
- `Documentation~/GUIDE.md` — full guide: 5-minute quickstart, concepts, complete API reference, config reference, integration with all 11 Sdx17 systems, demo instructions, 12-question FAQ, performance notes, and limitations.
- `CHANGELOG.md` — version history (Keep a Changelog format).
- `LICENSE` — MIT licence, Copyright 2026 Sdx17.
- Demo sample (`Samples~/Demo/`): `DemoSetup.cs` and setup instructions. Note: no `.unity` scene file is included; the demo scene is built in about two minutes following the sample README.
- Editor checklist (`Tests~/DevPluginChecklist.cs`): logs PASS / FAIL for the core paths from `Sdx17 > Tests > Run Developer Plugin Checklist`.
- `ThirdPartyNotices.txt` — no third-party code, fonts, audio, or assets are included; all code is original by Sdx17, MIT licensed.

## Requirements / Dependencies

- **Unity 2022.3 LTS** (the version the package targets; other versions have not been tried and compatibility with them is not claimed).
- **Render pipeline:** URP and Built-in compatible — the package uses no render-pipeline-specific code.
- **No paid dependencies.** No other packages are required.
- **No hard references to other Sdx17 packages.** The plugin compiles and runs standalone. It validates assets from the other Sdx17 systems generically and generates `ItemDefinition` / `QuestDefinition` / `DialogueGraph` assets by reflection when those packages are present.
- **Platforms:** editor tools run in the Unity Editor on any platform the Editor supports. The build checklist reports the Standalone scripting backend; other build targets have not been tested.

## Demo scene description

The package ships a demo sample (`Samples~/Demo/`, importable from the Package Manager). There is **no pre-built `.unity` scene file** — the demo scene is built in about two minutes:

1. Create a new scene and save it as `DevPluginDemo`.
2. Add an empty GameObject named `Demo` with the `DemoSetup` component.
3. Run `Sdx17 > Scene > Create Player / Camera / Light Setup` — this adds `Sdx17_Player` (capsule with `CharacterController`), `Sdx17_MainCamera` (follow camera, tagged `MainCamera`), and `Sdx17_DirectionalLight`.
4. Press Play. Expected Console output: the plugin version line, the player-found line, and the camera/light presence line (exact lines are in `Samples~/Demo/README.md` and `Documentation~/GUIDE.md`).

The demo shows the runtime helper and the starter scene setup. The editor tools (validator, generators, checklist, audit) are exercised from the `Sdx17` menu, not from the demo scene.

> The demo steps have not yet been run in a real Unity editor (v0.1.0 is untested). The listing screenshots and video must be real Unity captures of this demo once it has been verified — see `SELLING_REQUIREMENTS.md`.

## Support contact

- **Publisher:** Sdx17
- **GitHub:** https://github.com/sdx-rf17
- **Issues / questions:** open an issue on the Sdx17 GitHub profile's relevant repository, or contact the publisher through the Asset Store publisher page.

No email address is listed here, because no public support email exists for Sdx17 — do not invent one. Add a real support email to this draft only when Sdx17 has created one and is willing to publish it. Support is best-effort for v0.1.0; response times are not guaranteed.

## Version history

### 0.1.0 — 2026-11-21 (first release)

- Developer Hub editor window (`Sdx17 > Developer Hub`) with one-click access to every tool.
- Asset validator: open scene, selected assets, whole-project scripts; missing scripts, missing references, empty required fields, naming rules.
- Code generators: `ItemDefinition`, `QuestDefinition`, `DialogueGraph` — reflection-first, template fallback, no hard dependencies.
- Scene helper: player / camera / light starter setup, idempotent.
- Build helper: pre-build checklist (scenes, company/product names, scripting backend, missing scripts, texture compression).
- Project audit: asset counts, largest assets, duplicate names, Markdown report.
- `DevPluginSettings` ScriptableObject for naming rules, validation toggles, and audit output.
- Runtime helper: `DevPluginInfo` and `DevPluginLogger` (`[Sdx17]` prefix).
- Demo sample and editor checklist.
- **Status: UNTESTED IN UNITY.** Nothing in v0.1.0 has been compiled by Unity, opened in the editor, or run in Play Mode yet. See `README.md` for the test checklist.

*(No earlier versions exist. Do not add version entries that did not happen.)*

---

*Draft only — not submitted. No Asset Store listing exists yet, and no sales or reviews are claimed. Price suggestion ($25) does not guarantee sales. Re-check price, comparables, category, and publisher terms at listing time.*

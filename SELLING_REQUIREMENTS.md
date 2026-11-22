# Selling Requirements — Sdx17 Developer Plugin v0.1.0

> **Do NOT submit this package to the Unity Asset Store until every
> checkbox below is ticked with evidence (a date, a Unity version, and
> the file or screenshot that proves it).** v0.1.0 is currently
> **UNTESTED IN UNITY** — nothing has been compiled by Unity, opened in
> the editor, or run in Play Mode yet. Submitting an untested package
> risks rejection, bad first reviews, and a damaged publisher reputation
> that a new publisher cannot afford.
>
> Publisher: Sdx17 · GitHub: `sdx-rf17` · Package: `com.sdx17.devplugin`
> · Suggested price: $25 (see `LISTING.md` — re-check before listing).

---

## 1. Unity verification (the hard gate — all must pass)

- [ ] **Clean-project import:** the package imports into a brand-new, empty Unity **2022.3 LTS** project (URP template) with **zero errors and zero warnings** in the Console. Record the exact Unity version (for example `2022.3.62f1`).
- [ ] **README test checklist:** all 13 steps in `README.md` ("TEST checklist for Unity") pass, in order, in that clean project. Record the result of each step; if any step fails, fix it, bump the version if the code changed, and re-run the whole checklist.
- [ ] **Demo scene plays:** the `Samples~/Demo/` demo scene is built following `Samples~/Demo/README.md`, and pressing Play produces the expected Console lines (version line, player-found line, camera/light line) with no errors.
- [ ] **Editor checklist:** `Sdx17 > Tests > Run Developer Plugin Checklist` finishes with **0 FAIL**.
- [ ] **Standalone build:** the pre-build checklist passes, then a Standalone build of the demo scene succeeds and the built player starts with no errors.
- [ ] **Built-in pipeline spot-check (recommended):** repeat the import and demo steps in a fresh Built-in Render Pipeline project, since the listing claims URP/Built-in compatibility. If it fails, remove the Built-in claim from `LISTING.md` before submitting.

## 2. Documentation proofread

- [ ] **README.md proofread:** every menu path, class name, method name, file path, and default value matches the actual code. (The API examples in the README were written from the source; verify them by running them.)
- [ ] **Guide proofread:** `Documentation~/GUIDE.md` proofread end to end — quickstart steps actually work in order, the API reference matches the code (class/method names, signatures, return values), the config table matches `DevPluginSettings`, and the integration section does not over-claim (generators cover Inventory/Quest/Dialogue only; the other 8 systems are validated, not generated).
- [ ] **LISTING.md proofread:** title, short description (under 200 characters — recount in the portal), long description, keywords, "What's included", requirements, and demo description all match the real package. No feature is claimed that v0.1.0 does not have; no sale, review, or listing is claimed to exist.
- [ ] **CHANGELOG.md:** the 0.1.0 entry is accurate and complete. If code changed during testing, the changelog and version number are updated to match.
- [ ] **No stand-in text** left anywhere in the package, README, guide, or listing (no unfinished sections).

## 3. Screenshots (at least 5, real Unity screenshots only)

- [ ] **At least 5 screenshots**, all **real captures from Unity** (Editor or Play Mode) at **1920×1080**. No AI-generated images presented as screenshots, no mockups, no concept art in place of real captures.
- [ ] Suggested shot list (adapt to what the verified package actually shows):
  1. Developer Hub window open in the Editor.
  2. Validator results in the Console / hub (a real validation run).
  3. A generated `ItemDefinition` asset selected in the Inspector.
  4. The starter scene setup in the Scene view (`Sdx17_Player`, camera, light).
  5. The project audit report (`Assets/Sdx17AuditReport.md`) open in the Editor.
  6. (Optional) Pre-build checklist output in the Console.
- [ ] Each screenshot shows the real package doing the real thing — no staged fake results, no edited Console output, no numbers that do not match the demo project.
- [ ] Screenshots are checked at full size for readability (text legible, no UI cut off, no personal data visible).

## 4. Video (30–60 seconds, real Unity recording only)

- [ ] **One video, 30–60 seconds**, a **real screen recording of Unity** (Editor and/or Play Mode) — for example: open the hub → validate the scene → generate an Item Definition → create the starter setup → press Play on the demo scene.
- [ ] No AI-generated video, no slideshow of still images passed off as a recording, no voiceover claiming features the package does not have.
- [ ] Video is captioned or narrated plainly (or silent with on-screen text) — no invented testimonials, no "best seller" claims.
- [ ] Video file meets the Asset Store's current format/size requirements (check the publisher portal at upload time).

## 5. Icon

- [ ] **Icon 512×512** (PNG), supplied as the package icon in the publisher portal. It must be original Sdx17 artwork (for example the Sdx17 monogram / brand mark) — not a Unity default icon, not a screenshot, not an AI image presented as a screenshot.
- [ ] Icon is legible at small sizes (it will be shown as a thumbnail) and does not contain text that is unreadable when scaled down.

## 6. AI-content disclosure

- [ ] **No AI images presented as screenshots.** (See sections 3–4: screenshots and video must be real Unity captures.)
- [ ] **AI-content disclosure:** if any AI-generated material is used anywhere in the listing — for example concept art in a screenshot, the icon, or a cover image — disclose it honestly in the listing exactly as Unity's current AI-disclosure rules require, and label it as concept art, never as a screenshot or as the product itself. If no AI material is used, no disclosure is needed — but do not add AI material at the last minute without disclosing it.
- [ ] The code itself is original Sdx17 work (see `ThirdPartyNotices.txt`); if any part of the submitted package was AI-assisted in a way Unity requires disclosing, disclose that too.

## 7. Publisher account, tax, and payout

- [ ] **Publisher account set up** on the Unity Asset Store (publisher profile created, display name "Sdx17", profile complete).
- [ ] **Two-factor authentication (2FA) enabled** on the publisher/Unity account.
- [ ] **Tax information submitted** and accepted in the publisher portal (do not submit until this is done — payouts cannot happen without it).
- [ ] **PayPal set up** as the payout method and verified in the publisher portal.
- [ ] Publisher terms, revenue share (70% standard — $17.50 net per $25 sale before tax/refunds), and refund policy read and understood. The $25 price does not guarantee any sales.

## 8. Price and comparables (re-check at listing time)

- [ ] **Re-check comparable Asset Store listings** on the day of submission: search the Tools / Utilities category for small editor-utility packages (validator, generator, audit, workflow tools) and note their prices, review counts, and feature scope.
- [ ] **Re-check the $25 suggested price** against those comparables. $25 is the draft suggestion for a new publisher with zero reviews (the $15–$35 first-reviews band; $95 on day one is not recommended). If the comparables have moved, adjust the price and update `LISTING.md` — and record the comparables you checked.
- [ ] **Re-check the category** in the publisher portal: Tools / Utilities is the suggested category, but confirm it is still the closest match at listing time.

## 9. Submission rules and expectations

- [ ] **Review takes at least 5 business days.** Plan for it — do not promise buyers, or yourself, a launch date sooner than that, and do not submit the day before you need it live.
- [ ] **5 submissions/month cap in the first 6 months.** New publishers are limited to 5 submissions per month for the first 6 months (Unity's current limit — verify it in the publisher portal at submission time). Do not waste a submission on an untested package: a rejection costs one of those 5 slots and the review wait.
- [ ] **One clean submission, not a rushed one.** If anything in sections 1–8 is not done, fix it first and re-verify — do not submit "to see what happens".
- [ ] **After approval:** check the live listing page yourself (title, description, screenshots, video, price, requirements) before announcing it anywhere. Keep this file and `LISTING.md` updated with the live listing URL and the approval date once it exists.

---

## Sign-off

| Item | Evidence (file / screenshot / date) | Done |
| --- | --- | --- |
| Clean 2022.3 import, 0 errors/warnings | Unity version: ________, date: ________ | ☐ |
| README 13-step checklist, all pass | Notes file: ________ | ☐ |
| Demo scene plays | Screenshot/video: ________ | ☐ |
| Editor checklist 0 FAIL | Console capture: ________ | ☐ |
| Standalone build succeeds | Build log: ________ | ☐ |
| Docs proofread (README + Guide + Listing) | Reviewer/date: ________ | ☐ |
| ≥ 5 real screenshots, 1920×1080 | Folder: ________ | ☐ |
| 30–60 s real Unity video | File: ________ | ☐ |
| Icon 512×512 | File: ________ | ☐ |
| AI disclosure (if any AI material used) | Note: ________ | ☐ |
| Publisher + 2FA + tax + PayPal | Portal check date: ________ | ☐ |
| Price/comparables re-checked | Comparables noted: ________ | ☐ |
| Submitted (date) / approved (date) | Listing URL: ________ | ☐ |

*Nothing above has been done yet for v0.1.0. Every box is unchecked until the work is actually performed and the evidence is recorded.*

# Statistics+ (Statistle) — Microsoft Store submission

Everything Partner Center asks for that is not listing copy. Listing text is in
`STORE-LISTING.md`; the privacy policy is `PRIVACY-POLICY.md`; licence text is
`TERMS-OF-USE.md`; the reserved identity is in `STORE-IDENTITY.md`; upload images are in
`images/store/`.

Modelled on SMADA's `resources/STORE-SUBMISSION.md` — the two apps are the same kind of
offline desktop calculator on the same Partner Center account, so most answers are identical.

## Where each item stands — 2.1.0, the first release as Statistics+ (plan R.1–R.6)

1.0.0 is live (Store ID 9MV608S7M9X6, https://apps.microsoft.com/detail/9MV608S7M9X6, public). **2.0.0 was never
submitted**: Partner Center's submission 2 (a draft from 2026-11-12) holds `Statistle_2.0.0.0_x64_bundle.msixupload`. 2.1.0
replaces it in that same draft: its package version 2.1.0.0 is higher than both, so nothing clashes. 2.1.0 is an **update of
the same product**: same identity, Product name Statistics+.

| Item | Status |
|---|---|
| Name Statistics+ on this product | ✅ Moved by Ron 2026-11-12 (Manage app names) |
| Manifest | ✅ `DisplayName` and the tile's `ShortName` are Statistics+; identity unchanged; Version 2.1.0.0 |
| Version | ✅ **2.1.0** (`StatistleVersion`), higher than the live 1.0.0 and the draft's 2.0.0 |
| Privacy policy | ✅ `https://softwareplus.ai/statistle/privacy/`, with "Feedback you send" (live 2026-11-23, plan F.3) |
| `runFullTrust` justification and certification notes | ✅ Below, updated for 2.1.0 (Send feedback, the start screen's sections) |
| Store assets | ✅ The "+" icon, tiles and listing images from 2026-11-12 (unchanged) |
| Hand-test rounds 2026-11-23, 23b, 23c | ✅ Ron, 2026-11-23: *"Excellent work, tests well."* |
| Release build and tests | ✅ 2026-11-23: Release build, 752 tests green, the content validator clean |
| `.msixupload` | ✅ Built 2026-11-23: `artifacts\Statistle_2.1.0.0_x64_bundle.msixupload` (78.1 MB; 696 course files inside, Part III's Modules 19–24 among them); family name derived from the built manifest = `DeanEaglin.statistle_xs303fgqvwdg8`; the staged program starts as "Statistics+ — Untitled" (2.1.0.0) and closes |
| Listing copy | ✅ `STORE-LISTING.md`, rewritten for 2.1.0 (three parts, My notes, Glossary, Course Links, the new window, What's new) |
| Screenshots | ✅ 10 at 1600 × 1000 in `images/store/screenshots-2.1/` (the order is in `STORE-LISTING.md`) |
| WACK | ☐ **Ron** runs `packaging/run-wack.ps1` (needs elevation) on the 2.1.0 bundle |
| Submit | ☐ **Ron**, after WACK (order below) |

## The Partner Center account

| | |
|---|---|
| Sign-in | ron.eaglin@gmail.com |
| Publisher display name | **Dean Eaglin** |
| Publisher ID | `CN=E88392BA-A722-4B3A-8372-04403A55AA63` — per account, the same as SMADA's |
| Other apps | https://apps.microsoft.com/search/publisher?name=Dean+Eaglin |

## Building the submission package

```powershell
./packaging/make-msixupload.ps1 `
    -IdentityName 'DeanEaglin.statistle' `
    -Publisher    'CN=E88392BA-A722-4B3A-8372-04403A55AA63'
```

Note the **lower-case `s`** in `DeanEaglin.statistle` — that is what was reserved, and the
identity cannot be changed after the product is created.

The package is deliberately **unsigned**: the Store re-signs every submission with the
certificate it issues and rejects one signed with a local certificate.

Output, in `artifacts/`:

| File | Upload |
|---|---|
| `Statistle_<version>_x64_bundle.msixupload` | **This is the file Partner Center wants** |
| `Statistle_<version>_x64.msixbundle` | Inside the upload; kept for WACK and sideload testing |
| `Statistle_<version>_x64.appxsym` | Inside the upload; symbols for crash reports |

Version comes from `StatistleVersion` in `Directory.Build.props`; the Store reserves the fourth
part, so it is forced to 0. **Every submission needs a higher version than the last.**

Verify a build by deriving its package family name from the built manifest — SHA-256 of the
UTF-16 publisher string, first 8 bytes, base32 over `0123456789abcdefghjkmnpqrstvwxyz` — and
checking it equals `DeanEaglin.statistle_xs303fgqvwdg8`. A typo or case error in either half of
the identity breaks that match, which is a stronger check than reading the values back.

## `runFullTrust` justification

Paste into *Submission → Product declarations → notes*, and into the certification notes:

> Statistics+ (formerly Statistle) is a Win32 desktop application (WPF on .NET 10) distributed as an MSIX package. The
> `runFullTrust` restricted capability is required for any desktop application packaged this way. Statistics+ uses it
> only to run as an ordinary desktop process: it reads its own lessons from the package, keeps each learner's progress
> in `C:\ProgramData\Statistle\Learners` (shared by every account on the computer, since several students use one lab
> computer, each with a PIN-locked file of their own), and reads and writes the user's own files through standard
> Windows file dialogs — `.statistle` projects, PDF progress reports, and exported PDF, HTML, text, CSV, Python and R
> files, by default under `Documents\Statistle`. It does not access other applications' data, does not require
> administrator rights, installs no drivers or services, and collects no user data. It goes online only when the user
> asks: Check for a new version asks the Microsoft Store (`Windows.Services.Store`), and Help on the web, Send feedback
> and the About links open a page in the browser.

## Age rating (IARC questionnaire)

Statistics+ is a statistics course and calculator. The expected answers, all negative, give **3+ / Everyone**:

| Question area | Answer |
|---|---|
| Violence, fear, sexual content, profanity, drugs, gambling | None |
| In-app purchases | None |
| User-to-user interaction, chat, user-generated content sharing | None |
| Collects or shares personal information | No |
| Shares the user's location | No |
| Displays advertising | No |
| Unrestricted internet access | No — it goes online only when the user asks (Check for a new version asks the Microsoft Store; web help and About links open the browser) |

## Product declarations

| Declaration | Answer |
|---|---|
| Allows purchases outside the Microsoft commerce engine | No |
| Tested to meet accessibility guidelines | **No** — leave unchecked; Statistics+ has not been through a formal accessibility test |
| Depends on non-Microsoft drivers or NT services | No |
| Can run on Windows in S mode | No — a `runFullTrust` desktop app cannot |
| Contains cryptography | **Yes** (new in 2.0): .NET's own AES locks each learner's progress file with their PIN, and ECDSA signs progress reports so an instructor can check them. Standard mass-market encryption protecting the user's own data, which the export rules exempt; confirm it is limited to authentication, data integrity and the protection of the user's own files (as PLC+) |

**Notes for certification** (Partner Center's *Additional Testing Information* page):

> Statistics+ (formerly Statistle) requires no account, sign-in with us, purchase or network connection.
> (Check for a new version asks the Microsoft Store, and only when chosen.)
>
> **How to try it.** The first screen is in sections. Open **Who is learning?** (choose its name), press **New
> learner**, type any name and a 4-digit PIN, and keep or skip the recovery code. The Course opens: PlusBook Tools and
> the book's three parts on the left, help on the right. Open **Part I** and **Module 2**, and choose an item. Its tabs:
> Learn (a lesson), Examples (change the values and the figure redraws), Exercise (type an answer and press Check my
> answers; the program checks it against the value it calculates), Reference, and My notes. **Part III** holds
> applications in six fields (for example Module 24 · Sports). The Workbench (the strip at the top, or "Just the
> Workbench" in **Course Tools** on the first screen, no sign-in) is the statistics calculator: open a sample project from
> File > Open, or add a Data table and an analysis from the left rail; every analysis has an Instructions tab and a Code
> tab with the Python and R.
>
> **Files.** Learners' progress files go to `C:\ProgramData\Statistle\Learners`, shared by every account on a lab
> computer; user projects and exports to `Documents\Statistle`; recovery autosave to `%LOCALAPPDATA%\Statistle`.
> Nothing is written beside the executable.
>
> **Launching other programs.** Statistics+ calls ShellExecute only to open a page in the user's default browser
> (a generated report or instructions page as HTML, from a file it has just written to the temporary folder, or
> Help on the web, Help > Send feedback, which opens the feedback form on softwareplus.ai, and the About links), to open
> the user's own e-mail program with a message when the learner
> chooses Send to my instructor (the learner presses Send; the program sends nothing), and to show a saved report in
> File Explorer. It launches no other process, installs no service or driver, and does not run the Python or R it
> generates — that code is text for the user to copy.

## Pricing and availability

| Field | Value |
|---|---|
| Base price | **Free** |
| Free trial | None |
| Markets | All |
| Visibility | Public — available and discoverable |
| Device families | Windows 10/11 desktop, x64 |
| Publish date | As soon as it passes certification |

## System requirements

| | |
|---|---|
| OS | Windows 10 version 2004 (build 19041) or later — the manifest's `MinVersion`. Windows 11 supported |
| Architecture | x64 |
| Disk | ~250 MB (self-contained; it carries the .NET runtime, so no prerequisite install) |
| Memory | 4 GB recommended |
| Graphics | No special requirement; plots render through ScottPlot on the CPU |

## WACK

```powershell
./packaging/run-wack.ps1
```

It picks the newest `.msixbundle` in `artifacts/`, prompts for elevation, and prints the
pass/fail summary; the full report lands in `artifacts/wack-report.xml`.

**Do not call `appcert.exe` directly.** It is manifested `requireAdministrator`, so from a normal
prompt Windows either refuses it or spawns an elevated console that closes the instant it exits —
and an elevated console starts in `C:\Windows\System32`, where a relative package path does not
resolve, so appcert fails immediately and the window disappears before the error can be read.

**What SMADA's run found, and what was done about it here in advance:**

| SMADA's finding | Statistle |
|---|---|
| `App resources` — logo images over 204800 bytes | Pre-empted. `make-store-assets.ps1` writes every package asset under the cap, quantising 256 → 128 → 64 → 32 only as far as it must. All 68 assets are clear |
| `DPIAwarenessValidation` — no DPI declaration | Pre-empted. `src/Statistle.App/app.manifest` declares `PerMonitorV2` and `asInvoker` |
| `Blocked executables` — process-launch APIs | Expect the same optional FAIL. Almost all of it is inside Microsoft's own runtime, which a self-contained deployment must carry; Statistle's own contribution is the browser launch described in the certification notes above. It did not stop SMADA passing overall |

### Result — 2026-09-09, on 1.0.0

**OVERALL PASS, 23 of 24.** Both pre-emptions held: `App resources` and
`DPIAwarenessValidation` passed first time, where SMADA had to be fixed and rebuilt.

The one FAIL is `Blocked executables`, optional, 49 messages — the same finding SMADA
passed overall with. Three name Statistle's own binaries:

| Message | What it is |
|---|---|
| `Statistle.App.exe` … `ShellExecuteW`, `Statistle.App.dll` … `Process.Start` | Opening a generated report or instructions page in the browser. Legitimate for a `runFullTrust` desktop app; this test targets sandboxed UWP |
| `Statistle.Core.dll contains a blocked executable reference to "reg"` | **A false positive.** The scan matches blocked executable names as substrings. `Statistle.Core.dll` contains no `reg.exe` string and no process-launching API at all — checked — and the only words containing "reg" are `regression`, `region` and `Regular` |

The rest of the run makes the substring behaviour plain: it also reports `CsI`, `dNX`, `CDb` and `rEg` inside `System.Private.CoreLib.dll`, `System.Text.RegularExpressions.dll` and `PresentationFramework.dll`. The certification notes above already describe the browser launch, so a reviewer meeting this has the explanation in front of them.

WACK is not a gate on submission — Store certification runs its own equivalent — but a local
pass avoids a failed round trip.

## Submission order — 2.1.0

1. ~~Name, privacy page (with Feedback you send), version 2.1.0, manifest, listing copy, screenshots~~ — done (plan R).
2. ~~Ron's hand-test rounds 2026-11-23, 23b and 23c~~ — passed (Ron, 2026-11-23).
3. ~~The `.msixupload`~~ — built 2026-11-23 (above).
4. **Ron:** WACK, elevated: `./packaging/run-wack.ps1` (it picks the newest `.msixbundle`, the 2.1.0 one).
5. **Ron, in Partner Center, on the existing product, in the draft submission 2:**
   - Packages: remove `Statistle_2.0.0.0_x64_bundle.msixupload` and upload `artifacts\Statistle_2.1.0.0_x64_bundle.msixupload`.
   - Store listings → English: Product name **Statistics+**; the short description, description, features, What's new and
     search terms from `STORE-LISTING.md`; the 10 screenshots from `images/store/screenshots-2.1/` in its order; the new logo
     and listing images (`images/store/README.md`).
   - Properties → privacy URL `https://softwareplus.ai/statistle/privacy/`; Product declarations → cryptography **Yes**.
   - Additional Testing Information → the certification notes above. Submit.
6. The coordinator deploys `softwareplus.ai/statistle/` (the 2.1 page) at submission and updates `RELEASES.md`.
7. Once live: tag `v2.1.0` (plan R.6) and record it here.

The 1.0.0 history (WACK result, first submission) is kept above.

# Statistle — Microsoft Store submission

Everything Partner Center asks for that is not listing copy. Listing text is in
`STORE-LISTING.md`; the privacy policy is `PRIVACY-POLICY.md`; licence text is
`TERMS-OF-USE.md`; the reserved identity is in `STORE-IDENTITY.md`; upload images are in
`images/store/`.

Modelled on SMADA's `resources/STORE-SUBMISSION.md` — the two apps are the same kind of
offline desktop calculator on the same Partner Center account, so most answers are identical.

## Where each item stands

| Item | Status |
|---|---|
| Name reserved | ✅ `DeanEaglin.statistle` |
| Privacy policy page | ✅ Live at `https://punchmonkeyserver.com/privacy/statistle` |
| `runFullTrust` justification | ✅ Written — below |
| Version | ✅ **1.0.0**, the first release version |
| Store assets | ✅ Generated from the logo by `packaging/make-store-assets.ps1` |
| `.msixupload` | ✅ Built and identity-verified — below |
| Listing copy, categories, age rating | ✅ `STORE-LISTING.md` |
| WACK | ✅ **PASS**, 23/24, on 1.0.0 (2026-09-09 12:09) — see below |
| Screenshots | **Outstanding** — shot list at the end of `STORE-LISTING.md` |
| Submit | Blocked only on the screenshots |

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

> Statistle is a Win32 desktop application (WPF on .NET 10) distributed as an MSIX package. The
> `runFullTrust` restricted capability is required for any desktop application packaged this
> way. Statistle uses it only to run as an ordinary desktop process and to read and write the
> user's own files through standard Windows file dialogs — `.statistle` project files, and
> exported PDF, HTML, text, CSV, Python and R files, saved by default under the user's
> `Documents\Statistle` folder. It does not access other applications' data, does not require
> administrator rights, installs no drivers or services, makes no network connection of any
> kind, and collects no user data.

## Age rating (IARC questionnaire)

Statistle is a statistics calculator. The expected answers, all negative, give **3+ / Everyone**:

| Question area | Answer |
|---|---|
| Violence, fear, sexual content, profanity, drugs, gambling | None |
| In-app purchases | None |
| User-to-user interaction, chat, user-generated content sharing | None |
| Collects or shares personal information | No |
| Shares the user's location | No |
| Displays advertising | No |
| Unrestricted internet access | No — the app makes no network connection at all |

## Product declarations

| Declaration | Answer |
|---|---|
| Allows purchases outside the Microsoft commerce engine | No |
| Tested to meet accessibility guidelines | **No** — leave unchecked; Statistle has not been through an accessibility pass |
| Depends on non-Microsoft drivers or NT services | No |
| Can run on Windows in S mode | No — a `runFullTrust` desktop app cannot |
| Contains cryptography | No |

**Notes for certification** (free text, tells the tester how to exercise the app):

> Statistle requires no account, sign-in, or network connection, and has no trial or paid tier.
> To exercise it: open a sample project from `File → Open` (samples ship with the package), or
> add a Data table from the left rail, type a few numbers into a column, and then add a
> Descriptive statistics analysis. Every analysis has an Instructions tab describing what it
> does and a Code tab showing the Python and R that reproduce it. User files are written to the
> signed-in user's `Documents\Statistle` folder; recovery autosave goes to
> `%LOCALAPPDATA%\Statistle`. Nothing is written beside the executable.
>
> Statistle calls ShellExecute in one place only: to open a generated report or instructions
> page as HTML in the user's default browser, from a file it has just written to the temporary
> folder. It launches no other process, installs no service or driver, and makes no network
> connection. In particular it does not run the Python or R it generates — that code is text for
> the user to copy.

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
| Disk | ~200 MB (self-contained; it carries the .NET runtime, so no prerequisite install) |
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

## Submission order

1. ~~Reserve the name~~ — done: `DeanEaglin.statistle`.
2. ~~Publish the privacy page~~ — done and live.
3. ~~Set the release version~~ — done: 1.0.0.
4. ~~Generate the Store assets~~ — done, from the logo.
5. ~~Build the `.msixupload`~~ — done and PFN-verified.
6. ~~Run WACK~~ — done: PASS, 23/24.
7. Capture the screenshots (list in `STORE-LISTING.md`) — **at least 1366 px wide**.
8. Partner Center: packages → listing copy → images → screenshots → properties → age rating →
   pricing → submit.
9. Once live: tag the commit, and record the result here.

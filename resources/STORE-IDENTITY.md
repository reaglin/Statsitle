# Statistle — Microsoft Store identity

Reserved in Partner Center 2026-09-09. Recorded here because the package identity **cannot be
changed after the product is created**, and because two of the three values are account-wide and
easy to get wrong by hand.

| Value | Statistle |
|---|---|
| `Package/Identity/Name` | `DeanEaglin.statistle` — **lower-case `s`**, which is what was reserved |
| `Package/Identity/Publisher` | `CN=E88392BA-A722-4B3A-8372-04403A55AA63` |
| `PublisherDisplayName` | `Dean Eaglin` — set in `packaging/AppxManifest.xml` |
| Package family name | `DeanEaglin.statistle_xs303fgqvwdg8` |

The publisher ID and the `_xs303fgqvwdg8` hash are **per account**: SMADA (`DeanEaglin.SMADA`)
carries exactly the same pair. Only `Name` differs per app.

Partner Center account: sign-in `ron.eaglin@gmail.com`, publishing as **Dean Eaglin**
(https://apps.microsoft.com/search/publisher?name=Dean+Eaglin).

Privacy policy: **https://softwareplus.ai/statistle/privacy/** from 1.1.0 (source
`site/statistle/privacy/index.html` in the `SoftwarePlus` repo; 1.0 linked
`https://punchmonkeyserver.com/privacy/statistle`); the text is mirrored here as `PRIVACY-POLICY.md`.

The rest of the submission — listing copy, age rating, declarations, WACK, the order to do it in
— is in `STORE-SUBMISSION.md`.

## Packaging

Ported from SMADA 2026-09-09, so these four scripts match its Store pipeline:

| Script | What it does |
|---|---|
| `packaging/make-store-assets.ps1` | Every tile at five scales plus the taskbar target sizes, the Partner Center listing images, and `src/Statistle.App/Statistle.ico` — all from `resources/images/Statistle_logo2048x2048.jpg` |
| `packaging/pack.ps1` | `-IdentityName` / `-PublisherDisplayName`, and a `makepri` step so the scale-qualified assets actually resolve |
| `packaging/make-msixupload.ps1` | pack → `makeappx bundle` → zip with the `.appxsym`, unsigned as the Store requires |
| `packaging/run-wack.ps1` | Self-elevating certification-kit run against the newest bundle, with a pass/fail summary |

Build the submission package with:

```powershell
./packaging/make-msixupload.ps1 `
    -IdentityName 'DeanEaglin.statistle' `
    -Publisher    'CN=E88392BA-A722-4B3A-8372-04403A55AA63'
```

**Built and verified 2026-09-09 at 1.0.0**: `artifacts/Statistle_1.0.0.0_x64_bundle.msixupload`
(71 MB) — 68 assets and a 41 KB `resources.pri` in the package, `PerMonitorV2` in the exe, and
its derived package family name matched `DeanEaglin.statistle_xs303fgqvwdg8`.

## The artwork

Ron's logo — a `statistle` wordmark over an annotated bell curve — arrived 2026-09-09 and is the
source for everything. Two layers by size, the same rule SMADA uses:

- **The logo** carries the 310 × 310 tile, the splash, and the box, poster and hero images. Its
  white rounded-rectangle surround is flood-filled to its own navy (`#142E4F`) so tiles are
  full-bleed instead of showing a white notch in each corner.
- **The mark** — that bell curve redrawn without the wordmark or the annotations, in the logo's
  palette — carries everything small, where "statistle" and "68.2%" are illegible noise.

Every package asset is written under WACK's 204800-byte cap by stepping the palette down only as
far as needed (256 → 128 → 64 → 32); on flat artwork that is invisible. `packaging/Assets` is
1.3 MB, none of it over the cap. `AppxManifest.xml`'s splash `BackgroundColor` is `#142E4F` to
match.

## Still needed before submitting

- ~~WACK~~ — done 2026-09-09: **PASS, 23/24**. Both pre-emptions held; the one FAIL is the
  optional "Blocked executables", as expected. Details in `STORE-SUBMISSION.md`.
- **Screenshots** — .png, at least **1366 × 768**; shot list at the end of `STORE-LISTING.md`.
  Capture wider than 1366: SMADA's first set came out 1345 px and had to be padded.

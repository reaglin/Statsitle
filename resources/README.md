# resources/

Source material and everything the Microsoft Store submission needs. Nothing here is compiled
into the app.

## Store submission

| File | What it is |
|---|---|
| `STORE-SUBMISSION.md` | **Start here.** Package identity, the `runFullTrust` justification, age-rating answers, product declarations, system requirements, WACK, and the order to do it in |
| `STORE-IDENTITY.md` | The reserved Partner Center identity and what the packaging scripts do |
| `STORE-LISTING.md` | Every piece of listing copy, paste-ready, with Partner Center's character limits — plus the screenshot shot list |
| `PRIVACY-POLICY.md` | The privacy policy, mirrored from the live page at `softwareplus.ai/statistle/privacy/` (from 1.1.0; 1.0 linked `punchmonkeyserver.com/privacy/statistle`) |
| `TERMS-OF-USE.md` | Additional licence terms |
| `images/store/` | The Partner Center upload images, and a README saying which file fills which slot |

Regenerate the artwork with `packaging/make-store-assets.ps1`; build the submission package with
`packaging/make-msixupload.ps1`; run the certification kit with `packaging/run-wack.ps1`.

## Source images

| File | What it is |
|---|---|
| `images/Statistle_logo2048x2048.jpg` | The logo. Every icon, tile and listing image is derived from it |

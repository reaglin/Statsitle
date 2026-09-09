# Statistle — Privacy Policy

**Application:** Statistle
**Distribution:** Microsoft Store
**Publisher:** Dean Eaglin (Microsoft Store account name; the author publishes as Ron Eaglin)
**Contact:** ron.eaglin@gmail.com
**Effective:** 2026-09-09

**Published at `https://punchmonkeyserver.com/privacy/statistle`** — the page source is
`Components/Pages/Privacy/PrivacyStatistle.razor` in the `PunchMonkeyServer` repo. Keep the two
in step; the Store listing links to the published URL, and Partner Center requires a live one.

## Summary

Statistle does not collect, transmit, or share any personal information. It is a standalone
desktop application that runs entirely on your computer. It contains no analytics, no
telemetry, no advertising, no accounts, and no network communication of any kind.

Statistle is free software for students: enter a data table, choose an analysis, and it
computes the result, explains every input and every number it reports, and writes the Python
and R that reproduce the same analysis. It follows the syllabus of EGN3443 *Probability and
Statistics for Engineers*.

## What information Statistle collects

**None.** Statistle collects no personal information, no usage data, no diagnostic data, no
device identifiers, and no location data. It does not ask you to create an account or to
sign in, and it has no in-app purchases.

## Information you create, and where it stays

Everything you enter — data tables, analysis settings, project names — is your own work. It is
written only to your own computer, and only to these places:

| What | Where |
|---|---|
| Project files (`.statistle`), exported reports, and generated Python or R scripts | `Documents\Statistle`, or wherever you choose in the Save dialog |
| Recovery autosave, so an interrupted session can be restored | `%LOCALAPPDATA%\Statistle` |
| The HTML of a report or instructions page at the moment you ask to open it in your browser | Your Windows temporary folder, cleared by Windows in the normal way |

None of it is uploaded, synchronised, or sent anywhere. Deleting these files, or uninstalling
Statistle, removes them from your computer. The author has no copy of them and no way to
obtain one.

## The Code tab

Statistle's Code tab **writes** Python and R source for your data and your chosen analysis, so
you can read it, learn from it, and run it yourself elsewhere. Statistle does not execute that
code, does not install Python or R, and does not send your data or the generated script
anywhere. It is text on your screen until you copy or save it.

## Network use

Statistle makes no network connections. It contains no code that opens a socket, calls a web
service, checks for updates, or reports errors. It works with no internet connection at all.

## Permissions

The Microsoft Store package declares one capability, `runFullTrust`. This is the standard
capability for a full desktop (Win32/WPF) application and is what allows Statistle to read and
write your project files through ordinary Windows file dialogs. It is not used to access any
other application's data, and Statistle never needs administrator rights.

## Children's privacy

Statistle is a study tool for college statistics students. Because it collects no information
from anyone, it collects no information from children.

## Third parties

Statistle includes open-source components (MathNet.Numerics for the numerical routines,
ScottPlot for charts, PDFsharp for PDF export). These run locally as part of the application;
none of them transmit data.

Microsoft operates the Store through which Statistle is distributed, and Microsoft's own
privacy statement governs the download and installation process. That is separate from the
application itself, which remains offline.

## Changes to this policy

If this policy changes, the revised version will be published at the URL above with a new
effective date. Because Statistle collects nothing, any change is expected to be clarifying
rather than substantive.

## Contact

Questions about this policy: **ron.eaglin@gmail.com**

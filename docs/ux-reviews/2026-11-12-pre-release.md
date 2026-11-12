# UX review — Statistics+ 2.0.0 full sweep before the Store (plan 15.1), 2026-11-12

Cognitive walkthrough + Nielsen heuristics, read from source against `..\USABILITY-CHECKLIST.md`, by two `ux-reviewer`
passes in full-sweep mode (the Course side; the Workbench and the shell), then an inline re-check of the fixes.

**1 Critical, 7 Major, 31 Minor, 3 notes. All fixed** (the About wording by Ron's choice, below).

Tasks walked: a new student's first start to Module 0; an exercise with the Workbench beside it; a module check failed on
one question; forgetting the PIN, signing out and the idle lock; a second student on the same computer; an instructor
with no account checking a report; open the sample, run a one-way ANOVA, copy the R; import a CSV with a bad row; close
with unsaved changes; Help ▸ About and Check for a new version.

## The Course (commit 985e7b8)

| # | Severity | Finding | Fix |
|---|---|---|---|
| 1 | Major | "Who is learning?" covered only the content: the menu bar and Course/Workbench strip stayed live behind it; F1 opened help on the hidden page | Menu and strip disabled while it shows; F1 points to the screen's own help; its own status line |
| 2 | Major | Access keys clashed between the start screen and the menu (Alt+F, Alt+P) and on item pages (Alt+L: the Learn tab against the Learner menu) | "Open the learners' fol_der"; the item tabs have no access keys (Ctrl+1–4) |
| 3 | Major | The idle lock threw away typed, unchecked answers while its banner said the work was saved | The same learner returns to the item with the answers kept; a different learner starts empty; the banner says what is kept and for how long |
| 4 | Major | Enter in one answer box graded the whole exercise and recorded a failed attempt | Enter goes to the next empty box and checks only when all are filled; Check with a blank counts nothing |
| 5 | Minor | A module check with blank questions was graded and recorded | Names the blank questions, marks each card, counts nothing |
| 6 | Minor | The exercise did not say how to reach its Workbench analysis | The lead says how (or that the task says how) |
| 7 | Minor | Copy buttons gave no feedback | "Copied ✓" for 2 s |
| 8 | Minor | Show a hint did nothing after the last hint | Disabled, with "All n hints are shown above." |
| 9 | Minor | The start screen's Instructors paragraph did not say where to check a report | It names the button |
| 10 | Minor | No way to change a PIN except with the recovery code | Learner ▸ Change my PIN… |
| 11 | Minor | Tab tooltips did not give Ctrl+1–4; Help for this screen described only the Workbench | Both reworded |
| 12 | Minor | The tree legend lacked ★ ↺ ▤; "modules written so far" and "coming later" were draft words | Legend complete; draft words gone ("coming later" only if a module is) |
| 13 | Minor | The recovery-code page ignored Esc; its tick box had no access key or tooltip | Esc asks the closing question; key and tooltip |
| 14 | Minor | Settings' example path used the code name; nothing had focus with no learners | D:\Statistics\Learners; focus on New learner… |

## The Workbench and the shell (commit 79fd3ca)

| # | Severity | Finding | Fix |
|---|---|---|---|
| C1 | **Critical** | Importing a table skipped non-number cells column by column, shifting values up, so a regression could pair the wrong rows (silently, when two columns each lost a cell) | Import leaves out the whole row and says which rows and why; paired analyses refuse columns of different length and say so |
| M1 | Major | The data-table guide advised blank cells for missing values, which shifts paired data | Guide says to delete the whole row in paired data, and how import treats text |
| M2 | Major | The Project tree's highlight drifted from what was open; clicking the selected table again did nothing | Tree follows the open item both ways |
| M3 | Major | Undo stepped back only over adds, deletes and imports, so Ctrl+Z dropped everything typed since, with no warning | Every committed edit is an undo step (quick edits to one field merge); the status line names what was undone and what is next |
| m1 | Minor | An out-of-range value showed only a red border | A sentence under the box |
| m2 | Minor | A typed column name that did not match gave no list | The message lists the table's columns |
| m3 | Minor | Clicking the "Data tables" heading brought back the first-run banner mid-project | Keeps the open item; a "click a table or an analysis" banner when nothing is open |
| m4 | Minor | Copy, open-in-browser and Use result failures went only to the status line, one with a type name | Error boxes in plain words |
| m5 | Minor | "Undone" alone | Says what was undone |
| m6 | Minor | Some save dialogs had no title or start folder; Saved did not say where | Titles, the same folder, "Saved to {path}." |
| m7 | Minor | Narrator names missing on the code box, report and instructions viewers, an import box | Named |
| m8 | Minor | "Edit…" beside a table reference went to the table, no window | "Go to table" |
| m9 | Minor | "value(s)" | Singular and plural |
| — | Minor | The exe's Product name was Statistle | Product and AssemblyTitle are Statistics+; default file names "statistics" |

## Re-check of the fixes (inline, commit after c1daeec)

12 Minor and 2 notes, all fixed: the import preview's tense ("will be left out"); a warning when import leaves columns of
different length; the Enter tooltip made exact; "No answer yet" on each blank card; "Not copied" reverts; the recovery
code's Copy guarded; two messages that named a Cancel button and a menu item that do not exist; one undo step's name;
"Copied ✓" on the Workbench's Copy code and the result line too; every status message ends with a full stop; the exercise
lead names the Data from / Data table entry; Undo keeps its step if the restore fails; Change my PIN clears a wrong PIN.
Found while taking the Store screenshots: the exercise lead repeated the steps an exercise already gives (now skipped), and
the status line under "Who is learning?" spoke about the page behind it.

## Decided by Ron (2026-11-12): the suggested wording

- Help ▸ About said "nothing you enter leaves this computer". Send to my instructor and Check for a new version go online
  when the person asks. Now: "Free software; nothing leaves this computer unless you send it." (plan 5.7)

## To look at by eye (hand-test part W)

The start screen at 1366 × 768 and 150 %; the tree at its narrowest; the answer form beside the tree at the smallest
window; the red ✗ and the problem banner in High Contrast; "Copied ✓" keeping the button's width; the two light-blue
banners in the Workbench; focus visible on the Course/Workbench strip.

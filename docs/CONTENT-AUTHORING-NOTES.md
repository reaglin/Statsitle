# Statistics+ — notes for content authors (what the first five modules taught)

Read with `docs/CONTENT-FORMAT.md`. Module 2 is Ron's approved model (plan 11.1 ✅); modules 0, 1, 3 and 4 were
written to it and then fixed after a UX review (`docs/ux-reviews/2026-11-11-modules-0-4.md`). Do these from the start:

1. **Every number is checked.** Asked answers come from the engine, but every number in prose, hints, quiz answers and
   explanations is computed (a scratch C# harness with `StatsTaskRunner.Build`, or the Python standard library; there
   is no numpy here). R is at `C:\Program Files\R\R-4.4.3` if you want to run the generated R.
2. **Say how precise to be.** An exercise intro says how many decimals; a probability is asked with
   `"tolerance": 0.001` and "three decimals or more". The answer box's tooltip says the same from the tolerance.
3. **Recognized mistakes** (`mistakes`) wherever the Learn page names a classic error and the analysis has the
   mistaken number (another result, or a fixed `value` you compute). The validator rejects one inside the tolerance.
4. **`wrong`** on an asked result whose likely error is not arithmetic (a number read in the Workbench: "Check the
   Workbench settings …").
5. **Every module-check question has `lookAgain`** naming the item that teaches it (a test fails otherwise).
6. **Practice questions never repeat a case the Learn page works in full**, and one question never gives away
   another's answer.
7. **Distractors** are the same length and shape as the right answer, and plainly wrong to someone who knows; none is
   literally true.
8. **No asked results within each other's tolerance** (the validator warns; aim for no warnings).
9. **Define every symbol and term before use** (⌈ ⌉, log₂, df, t, …); point forward ("Module 7 teaches …") for previews.
10. **The Workbench is reached by the Workbench button (Ctrl+B) and choosing the analysis in the list on the left**;
    there is no "add". Give settings by the labels the Workbench shows, and say Ctrl+L comes back.
11. **`null` leaves a number setting blank** (no alternative mean, so no power row; no x₂).
12. **Matching questions**: long left items wrap, but keep them short where you can.
13. **Name where course material is** ("in Module N on the course website (reaglin.github.io/EGN3443)"), after checking.
14. **Don't hand over a graded assignment's answers**: use the course's datasets for Examples where Ron allowed it,
    but not an assignment's exact numbers in an Exercise.
15. Reuse reference pages where they fit; ids `st.mN`, `st.mN.oK`, `st.mN.<slug>`, tasks `st.mN.<slug>.eg/.ex`,
    files `mN-<slug>-eg.json`; module outcomes in the course's own MO wording.

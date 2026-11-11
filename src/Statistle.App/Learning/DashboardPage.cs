using System.Text;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Progress;
using CoreDashboard = Learning.Core.Progress.Dashboard;

namespace Statistle.App.Learning;

/// <summary>
/// The My progress page (plan 9.4; ported from PLC+'s <c>ShellViewModel.DashboardMarkdown</c>): so far, review suggested,
/// the module checks, the path map, and the outcomes, as Markdown for <see cref="MarkdownView"/>. The numbers come from the
/// Learning Core's <see cref="CoreDashboard"/>.
/// </summary>
public static class DashboardPage
{
    /// <summary>The page for <paramref name="progress"/> in <paramref name="package"/>, at <paramref name="now"/> (default: now).</summary>
    public static string Markdown(ContentPackage package, LearningProgress progress, DateTimeOffset? now = null)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(progress);
        var app = LearningApp.Name;
        var d = CoreDashboard.Build(package, progress, now ?? DateTimeOffset.Now);
        var t = d.Totals;
        var md = new StringBuilder();
        md.AppendLine("## So far").AppendLine();
        md.AppendLine($"- **{t.ItemsComplete} of {t.Items} items** complete, in the modules written so far");
        md.AppendLine($"- **{t.ModulesComplete} of {t.Modules} modules** complete");
        md.AppendLine($"- **{t.OutcomesMastered} of {t.Outcomes} outcomes** mastered");
        md.AppendLine($"- **Time in {app}:** " + (t.Time.TotalMinutes < 1 ? "none yet" :
            $"{CoreDashboard.Duration(t.Time)} in all, {CoreDashboard.Duration(t.Today)} today, over {t.Sessions} sitting{(t.Sessions == 1 ? "" : "s")}") +
            $". Time counts while you are using {app} (a key or the mouse in the last two minutes), not while it sits idle.");
        md.AppendLine();

        md.AppendLine("## Review suggested").AppendLine();
        if (d.ReviewSuggested.Count == 0)
            md.AppendLine("Nothing to review. If an answer in a module check is wrong, or an exercise you passed later fails, it is listed here with what to read again.").AppendLine();
        foreach (var r in d.ReviewSuggested)
        {
            var why = string.Join("; ", r.Outcome.Evidence.Where(e => e.Contains("wrong", StringComparison.Ordinal) || e.Contains("did not pass", StringComparison.Ordinal)));
            md.AppendLine($"- ↺ **Module {r.Module.Number}:** {r.Outcome.Outcome.Text}" + (why.Length > 0 ? $" *({why}.)*" : ""));
        }
        if (d.ReviewSuggested.Count > 0) md.AppendLine();

        md.AppendLine("## Module checks").AppendLine();
        md.AppendLine("Each module ends with a module check: questions on the whole module, complete when the items it covers are complete and you pass them. " +
                      "Progress reports to hand in to your instructor come in a later version.").AppendLine();
        md.AppendLine("| Module check | Where you are |").AppendLine("|---|---|");
        foreach (var s in d.Submissions)
            md.AppendLine($"| {s.Check} | {(s.State == SubmissionState.NotReached ? "○ " : "✓ ")}{(s.State == SubmissionState.NotReached ? s.Detail : "Complete")} |");
        md.AppendLine();
        md.AppendLine("*○ not complete yet. ✓ module check complete.*").AppendLine();

        md.AppendLine("## The path").AppendLine();
        md.AppendLine("| Module | Where you are | Items | Time |").AppendLine("|---|---|---|---|");
        foreach (var e in d.Modules)
            md.AppendLine($"| {e.Module.Number}. {e.Module.Title} | {CoreDashboard.Mark(e.State)} {CoreDashboard.Words(e)} | " +
                          (e.State == PathState.ComingLater ? "–" : $"{e.Complete} of {e.Total}") + $" | {(e.Time > TimeSpan.Zero ? CoreDashboard.Duration(e.Time) : "–")} |");
        md.AppendLine();
        md.AppendLine("*★ mastered: complete, and every outcome mastered. ✓ complete, not every outcome mastered yet. ↺ complete, review suggested. ◐ in progress. ○ not started (\"after module N\": recommended first, not required). – coming later.*").AppendLine();

        md.AppendLine("## Outcomes").AppendLine();
        foreach (var e in d.Modules.Where(e => LearningProgress.Learnable(e.Module) && e.Outcomes.Count > 0))
        {
            md.AppendLine($"**Module {e.Module.Number}, {e.Module.Title}**").AppendLine();
            foreach (var o in e.Outcomes)
                md.AppendLine(o.State switch
                {
                    MasteryState.Mastered => $"- ★ {o.Outcome.Text} (mastered)",
                    MasteryState.NeedsReview => $"- ↺ {o.Outcome.Text} (review suggested)",
                    MasteryState.InProgress => $"- ◐ {o.Outcome.Text} (in progress)",
                    _ => $"- ○ {o.Outcome.Text}",
                });
            md.AppendLine();
        }
        return md.ToString();
    }
}

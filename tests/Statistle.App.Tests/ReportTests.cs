using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using Learning.Core;
using Learning.Core.Content;
using Learning.Core.Learners;
using Learning.Core.Progress;
using Learning.Core.Reports;
using Learning.Reports.Pdf;
using Statistle.App.Learning;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>
/// Plan 14.1 and 14.2: a learner who passed an exercise gets a report with that exercise's results and figure, the PDF is
/// genuine to the verifier, and its STAT1 result line checks out with the class code.
/// </summary>
public sealed class ReportTests
{
    private static readonly StatsContent Content = StatsContent.Load(Egn3443());

    private static string Egn3443()
    {
        var dir = AppContext.BaseDirectory;
        while (dir is not null && !File.Exists(Path.Combine(dir, "Statistle.sln"))) dir = Path.GetDirectoryName(dir);
        return Path.Combine(dir ?? throw new InvalidOperationException("Statistle.sln not found"), "content", "egn3443");
    }

    /// <summary>The first graded exercise in the course, passed with the values the analysis calculates.</summary>
    private static (LearningItem Item, LearningItem Check, LearnerData Data) PassedOne()
    {
        var item = Content.Package.Modules.Where(m => m.Items.Any(i => i.Kind == ItemKind.Assess)).SelectMany(m => m.Items).First(i =>
            Content.Find(i.Exercise) is { } t && StatsTaskRunner.Build(t).Expected.Count > 0);
        var task = Content.Find(item.Exercise)!;
        var built = StatsTaskRunner.Build(task);
        var work = StatsTaskRunner.Work(built.Expected.ToDictionary(e => e.Asked.Result, e => e.Value.ToString("R", CultureInfo.InvariantCulture), StringComparer.Ordinal));
        var outcome = new StatsTaskRunner().Run(task.Json, work);
        Assert.True(outcome.Passed);

        var data = new LearnerData { Profile = new LearnerProfile("p1", "Ada Lovelace", null, DateTimeOffset.Now) };
        data.ReportDetails = new ReportDetails { Course = "EGN3443", ClassCode = ClassCode.New(), Confirmed = true };
        data.Progress.Record(item, outcome, DateTimeOffset.Now);
        data.Work[ReportBuilder.PassedWorkKey(outcome.ChallengeId)] = new SavedWork { Content = work, Saved = DateTimeOffset.Now };
        var check = item.Module.Items.Single(i => i.Kind == ItemKind.Assess);
        return (item, check, data);
    }

    [Fact]
    public void A_report_shows_the_passed_exercise_with_its_results_and_is_genuine_with_its_line()
    {
        WpfHost.Run(() =>
        {
            LearningApp.Name = "Statistics+";
            var (item, check, data) = PassedOne();
            using var key = ComputerKey.New();
            var report = ReportBuilder.Build(Content.Package, data, check, StatsReports.Options, DateTimeOffset.Now, key.Fingerprint);
            Assert.Equal(ReportStatus.Incomplete, report.Status);   // one exercise passed; the rest of the module is not
            var evidence = Assert.Single(report.Evidence);
            Assert.Equal(item.Id, evidence.ItemId);

            var png = ExerciseEvidence.Png(Content, evidence);
            Assert.NotNull(png);
            Assert.Equal([0x89, (byte)'P', (byte)'N', (byte)'G'], png![..4]);

            var pictures = 0;
            var pdf = ReportPdf.Render(report, key.PublicKey, StatsReports.Words, e => { pictures++; return ExerciseEvidence.Png(Content, e); });
            Assert.Equal(1, pictures);
            var code = data.ReportDetails!.ClassCode;
            var sealedReport = ReportSeal.Seal(pdf, key, code);
            var verified = ReportSeal.Verify(sealedReport.Pdf, code);
            Assert.Equal(ReportCheck.Genuine, verified.Outcome);
            Assert.Equal(ClassCheck.Matches, verified.Class);
            Assert.Equal("StatisticsPlus_Lovelace_Module" + check.Module.Number + "Check_" + DateTimeOffset.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + ".pdf",
                report.FileName(StatsReports.Options.FilePrefix));

            var line = ResultLine.Issue(report, StatsReports.Options.LineMarker, sealedReport.Signature, code).Text();
            Assert.StartsWith("STAT1|Ada Lovelace|Module " + check.Module.Number + " check", line, StringComparison.Ordinal);
            Assert.Equal(LineCheck.Matches, ResultLine.Check(line, "STAT1", code, null).Outcome);
            Assert.Equal(LineCheck.CannotCheck, ResultLine.Check(line, "STAT1", null, sealedReport.Pdf).Outcome);   // signed with the class code: it needs the code
            Assert.Equal(LineCheck.DoesNotMatch, ResultLine.Check(line.Replace("Ada", "Bob", StringComparison.Ordinal), "STAT1", code, null).Outcome);
        });
    }

    [Fact]
    public void The_report_page_lists_every_module_check_and_the_full_report_for_a_signed_in_learner()
    {
        var folder = Path.Combine(Path.GetTempPath(), "statistle-report-" + Guid.NewGuid().ToString("N"));
        try
        {
            WpfHost.Run(() =>
            {
                LearningApp.Name = "Statistics+";
                var page = new CoursePage(Content);
                var window = new Window { Content = page, Width = 1300, Height = 860, Left = -10000, Top = -10000, ShowInTaskbar = false, ShowActivated = false };
                window.Show();

                page.ShowReports();   // nobody signed in: the page says to sign in, and makes nothing
                WpfHost.Pump();
                Assert.Null(FindList(window, "What the report covers"));

                Directory.CreateDirectory(folder);
                var store = new LearnerStore(folder, LearnerPlaces.Options);
                var (file, _) = store.Create("Ada Lovelace", null, "2468", DateTimeOffset.Now, PassedOne().Data);
                using (file)
                {
                    page.SignIn(file);
                    page.ShowReports();
                    WpfHost.Pump();
                    var points = FindList(window, "What the report covers");
                    Assert.NotNull(points);
                    var checks = Content.Package.Modules.Count(m => m.Items.Any(i => i.Kind == ItemKind.Assess));
                    Assert.Equal(checks + 1, points!.Items.Count);   // every module check, then Everything so far
                    Assert.NotNull(points.SelectedItem);
                    page.SignIn(null);
                }
                window.Close();
            });
        }
        finally
        {
            try { Directory.Delete(folder, recursive: true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
    }

    /// <summary>A sample report and its evidence picture, when STATISTLE_REPORT_SAMPLES names a folder (to look at by eye).</summary>
    [Fact]
    public void Write_a_sample_report_when_asked()
    {
        var folder = Environment.GetEnvironmentVariable("STATISTLE_REPORT_SAMPLES");
        if (string.IsNullOrEmpty(folder)) return;
        Directory.CreateDirectory(folder);
        WpfHost.Run(() =>
        {
            LearningApp.Name = "Statistics+";
            var (_, check, data) = PassedOne();
            using var key = ComputerKey.New();
            var report = ReportBuilder.Build(Content.Package, data, check, StatsReports.Options, DateTimeOffset.Now, key.Fingerprint);
            File.WriteAllBytes(Path.Combine(folder, "evidence.png"), ExerciseEvidence.Png(Content, report.Evidence[0])!);
            var pdf = ReportPdf.Render(report, key.PublicKey, StatsReports.Words, e => ExerciseEvidence.Png(Content, e));
            File.WriteAllBytes(Path.Combine(folder, report.FileName(StatsReports.Options.FilePrefix)), ReportSeal.Seal(pdf, key, data.ReportDetails!.ClassCode).Pdf);
        });
    }

    private static ListBox? FindList(DependencyObject root, string name)
    {
        if (root is ListBox list && AutomationProperties.GetName(list) == name && list.IsVisible) return list;
        foreach (var child in LogicalTreeHelper.GetChildren(root).OfType<DependencyObject>())
            if (FindList(child, name) is { } found) return found;
        return null;
    }
}

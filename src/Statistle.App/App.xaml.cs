using System.IO;
using System.Windows;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;

namespace Statistle.App;

public partial class App : Application
{
    /// <summary>
    /// Headless switch: writes the sample projects into the folder named after it and exits
    /// without showing a window. It is how <c>samples/</c> is produced, so a sample can never
    /// drift from what the current build reads.
    /// </summary>
    private const string WriteSamplesSwitch = "--write-samples";

    /// <summary>The project file the app was activated with, if any.</summary>
    public string? ActivationFilePath { get; private set; }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string[] args = e.Args;

        int samplesIndex = Array.IndexOf(args, WriteSamplesSwitch);
        if (samplesIndex >= 0)
        {
            string folder = samplesIndex + 1 < args.Length
                ? args[samplesIndex + 1]
                : Path.Combine(Environment.CurrentDirectory, "samples");

            WriteSamples(folder);
            Shutdown(0);
            return;
        }

        ActivationFilePath = args.FirstOrDefault(
            a => a.EndsWith(".statistle", StringComparison.OrdinalIgnoreCase));

        // Documents\Statistle is the home of every Statistle file. It is created on first run so
        // the Open and Save dialogs have somewhere to land; if it cannot be created, the dialogs
        // simply fall back to wherever Windows puts them.
        try
        {
            StatistlePaths.EnsureDocumentsFolder();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Nothing to do: the folder is a convenience, not a requirement.
        }

        var window = new MainWindow();
        MainWindow = window;
        window.Show();
    }

    /// <summary>
    /// Writes the sample projects. One demonstration project: two data tables from the course,
    /// with a descriptive-statistics analysis and a regression already set up on them.
    /// </summary>
    private static void WriteSamples(string folder)
    {
        Directory.CreateDirectory(folder);

        var document = new StatistleDocument
        {
            Name = "Demonstration statistics project",
            Description =
                "Two data tables and two analyses, to show what a Statistle project holds. " +
                "Tensile strength: ten specimens of each of three aluminium alloys. Solar panel: " +
                "the measured efficiency of a photovoltaic panel at ten cell temperatures.",
        };

        DataTable strength = document.AddTable("Tensile strength");
        strength.Description = "Ultimate tensile strength, MPa; ten specimens of each alloy.";
        strength.Columns.AddColumn("AA2024", [470, 485, 478, 492, 460, 473, 488, 484, 482, 475]);
        strength.Columns.AddColumn("AA7075", [505, 518, 523, 508, 515, 522, 510, 529, 520, 514]);
        strength.Columns.AddColumn("AA-X", [495, 508, 512, 492, 516, 505, 498, 510, 504, 489]);

        DataTable panel = document.AddTable("Solar panel");
        panel.Description = "Cell temperature in °C against conversion efficiency in per cent.";
        panel.Columns.AddColumn("Temperature", [15, 20, 25, 30, 35, 40, 45, 50, 55, 60]);
        panel.Columns.AddColumn("Efficiency", [19.1, 18.7, 18.2, 17.6, 17.1, 16.3, 15.8, 15.1, 14.5, 13.9]);

        DescriptiveStatistics statistics = document.Tool<DescriptiveStatistics>();
        statistics.Name = "AA2024 tensile strength";
        statistics.Description = "The summary numbers for the first alloy's ten specimens.";
        statistics.Column = DataSources.Reference(strength.Id, "AA2024");
        statistics.Recalculate();

        SimpleRegression regression = document.Tool<SimpleRegression>();
        regression.Name = "Efficiency against temperature";
        regression.Description = "How far the panel's efficiency falls per degree of cell temperature.";
        regression.XColumn = DataSources.Reference(panel.Id, "Temperature");
        regression.YColumn = DataSources.Reference(panel.Id, "Efficiency");
        regression.Recalculate();

        document.Save(Path.Combine(folder, "demo-statistics.statistle"));
    }
}

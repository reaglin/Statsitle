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
                "Four data tables and five analyses, to show what a Statistle project holds. " +
                "Tensile strength: ten specimens of each of three aluminium alloys. Solar panel: " +
                "the measured efficiency of a photovoltaic panel at ten cell temperatures. Housing: twenty houses for multiple regression. Battery life: a balanced two-factor experiment.",
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
        regression.PredictAt = 42;
        regression.Recalculate();

        DataTable housing = document.AddTable("Housing");
        housing.Description = "Twenty houses (synthetic, in the shape of the course's Module 12 problem): size in sq ft, age in years, bedrooms, distance to the centre in km, price in thousands.";
        housing.Columns.AddColumn("size", [1610, 1120, 3170, 2690, 1190, 2160, 2900, 1840, 1820, 2610, 2070, 2390, 1780, 1850, 2430, 2030, 1150, 2330, 2250, 2030]);
        housing.Columns.AddColumn("age", [13, 31, 27, 27, 27, 4, 24, 34, 2, 30, 39, 33, 28, 28, 37, 9, 28, 2, 4, 25]);
        housing.Columns.AddColumn("bedrooms", [4, 2, 5, 4, 2, 2, 4, 4, 2, 4, 5, 5, 3, 4, 4, 3, 4, 3, 3, 5]);
        housing.Columns.AddColumn("distance", [5.5, 29.4, 26.3, 13.1, 25.3, 5.3, 24.1, 23.4, 7.5, 16.8, 19.6, 7.5, 3.3, 13.4, 5.5, 3.1, 4.8, 12.5, 14.5, 7.2]);
        housing.Columns.AddColumn("price", [234.0, 89.1, 391.8, 331.9, 88.9, 310.7, 345.5, 215.5, 280.7, 313.4, 261.4, 310.4, 230.6, 238.7, 329.2, 261.5, 199.4, 326.4, 298.7, 296.5]);

        DataTable battery = document.AddTable("Battery life");
        battery.Description = "A balanced two-factor experiment (synthetic, in the shape of Montgomery's battery example): temperature level 1–3, material 1–2, three batteries per cell, life in hours.";
        battery.Columns.AddColumn("temperature", [1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3]);
        battery.Columns.AddColumn("material", [1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 2, 2, 1, 1, 1, 2, 2, 2]);
        battery.Columns.AddColumn("life", [130, 155, 174, 150, 188, 159, 34, 40, 80, 136, 122, 106, 20, 70, 82, 25, 70, 58]);

        OneWayAnova oneWay = document.Tool<OneWayAnova>();
        oneWay.Name = "Alloy tensile strength";
        oneWay.Description = "Module 14 sample problem 1: do the three alloys differ in mean tensile strength, and which pairs?";
        oneWay.Table = strength.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        oneWay.Recalculate();

        TwoWayAnova twoWay = document.Tool<TwoWayAnova>();
        twoWay.Name = "Battery life by temperature and material";
        twoWay.Description = "Do temperature and material each matter, and does the best material depend on the temperature?";
        twoWay.Table = battery.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        twoWay.FactorA = "temperature";
        twoWay.FactorB = "material";
        twoWay.Response = "life";
        twoWay.Recalculate();

        MultipleRegression multiple = document.Tool<MultipleRegression>();
        multiple.Name = "House price on size, age, bedrooms, distance";
        multiple.Description = "Which of the four factors move the price, and by how much each.";
        multiple.Table = housing.Id.ToString(System.Globalization.CultureInfo.InvariantCulture);
        multiple.Response = "price";
        multiple.Recalculate();

        document.Save(Path.Combine(folder, "demo-statistics.statistle"));
    }
}

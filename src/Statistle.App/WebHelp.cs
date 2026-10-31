using System.Diagnostics;
using System.Windows;
using Statistle.Core.Analyses;
using Statistle.Core.Domain;

namespace Statistle.App;

/// <summary>
/// Statistle's help on SoftwarePlus.ai, page by page, so the help for a screen can say where
/// to read more (USABILITY-CHECKLIST 1.3, 2026-10-31). Opening a page opens the browser;
/// Statistle itself makes no connection.
/// </summary>
public static class WebHelp
{
    public const string Root = "https://softwareplus.ai/statistle/help/";

    /// <summary>The help page for the item selected: its analysis family, data tables, or the index.</summary>
    public static string PageFor(object? selected) => selected switch
    {
        DataTable => "data.html",
        DescriptiveStatistics => "descriptive.html",
        ProbabilityRules or DiscreteDistribution or ContinuousDistribution or SamplingDistribution => "probability.html",
        ConfidenceInterval or SampleSize => "estimation.html",
        OneSampleTest or TwoSampleTest => "tests.html",
        SimpleRegression or MultipleRegression => "regression.html",
        OneWayAnova or TwoWayAnova => "anova.html",
        _ => "index.html",
    };

    /// <summary>The address as people read it: "softwareplus.ai/statistle/help/data.html".</summary>
    public static string Display(string page) => "softwareplus.ai/statistle/help/" + (page == "index.html" ? string.Empty : page);

    public static void Open(Window? owner, string page)
    {
        string url = Root + (page == "index.html" ? string.Empty : page);

        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            string message = $"Windows could not open a web browser. The help is at {url}";

            if (owner is not null)
            {
                MessageBox.Show(owner, message, "Statistle — Help on the web", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(message, "Statistle — Help on the web", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}

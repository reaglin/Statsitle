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

        // Only Statistle itself opens its window. A test host builds an App for its resources; were the
        // window opened there, its recovery question and Close would work on the person's real recovery
        // copy in %LOCALAPPDATA%\Statistle (that deleted one on 2026-10-31).
        if (System.Reflection.Assembly.GetEntryAssembly() != typeof(App).Assembly)
        {
            return;
        }

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
        // The demonstration project is installed there on first run so a new user has something
        // to open; an existing copy is never overwritten, so a user's edits to it survive.
        try
        {
            StatistlePaths.EnsureDocumentsFolder();
            SampleProjects.Install(StatistlePaths.DocumentsFolder);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Nothing to do: the folder and the sample are a convenience, not a requirement.
        }

        global::Learning.Core.LearningApp.Name = "Statistics+";
        var window = new MainWindow();
        MainWindow = window;
        window.Show();
        window.ShowLearnerScreen();
    }

    /// <summary>Writes the sample projects into the folder (overwriting), for <c>samples/</c>.</summary>
    private static void WriteSamples(string folder)
    {
        Directory.CreateDirectory(folder);
        SampleProjects.Demonstration().Save(Path.Combine(folder, SampleProjects.DemonstrationFileName));
    }

}

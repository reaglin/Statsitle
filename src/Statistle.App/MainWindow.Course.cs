using System.IO;
using System.Windows;
using System.Windows.Input;
using Statistle.Learning;

namespace Statistle.App;

/// <summary>
/// Statistics+ (plan 9): the Course beside the Workbench. This part of the window loads the content package,
/// switches between the two, and opens a Course task in the Workbench.
/// </summary>
public partial class MainWindow
{
    public static readonly RoutedCommand ShowCourseCommand = new(nameof(ShowCourseCommand), typeof(MainWindow),
        [new KeyGesture(Key.L, ModifierKeys.Control)]);

    public static readonly RoutedCommand ShowWorkbenchCommand = new(nameof(ShowWorkbenchCommand), typeof(MainWindow),
        [new KeyGesture(Key.B, ModifierKeys.Control)]);

    /// <summary>The Course's content, or null when it could not be read (then the Workbench is all there is).</summary>
    public StatsContent? Course { get; private set; }

    /// <summary>Why the Course could not be read, in words for the person.</summary>
    public string? CourseProblem { get; private set; }

    /// <summary>Where the content package is: beside the exe, or the STATISTLE_CONTENT folder (authors, tests).</summary>
    public static string ContentFolder =>
        Environment.GetEnvironmentVariable("STATISTLE_CONTENT") is { Length: > 0 } custom
            ? custom
            : Path.Combine(AppContext.BaseDirectory, "content", "egn3443");

    private void InitializeCourse()
    {
        CommandBindings.Add(new CommandBinding(ShowCourseCommand, (_, _) => CourseMode.IsChecked = true));
        CommandBindings.Add(new CommandBinding(ShowWorkbenchCommand, (_, _) => WorkbenchMode.IsChecked = true));

        try
        {
            Course = StatsContent.Load(ContentFolder);
        }
        catch (Exception ex) when (ex is global::Learning.Core.Content.ContentException or IOException or UnauthorizedAccessException)
        {
            CourseProblem = "The Course could not be read, so only the Workbench is available. Reinstalling Statistics+ from the "
                            + "Microsoft Store puts it back. Details for support: " + ex.Message;
        }

        BuildCoursePane();
        if (Course is null) WorkbenchMode.IsChecked = true;
        else CourseMode.IsChecked = true;
    }

    /// <summary>Shows the Workbench (the tests that draw its rows, and Open in the Workbench).</summary>
    public void ShowWorkbench() => WorkbenchMode.IsChecked = true;

    /// <summary>Shows the Course, when there is one.</summary>
    public void ShowCourse()
    {
        if (Course is not null) CourseMode.IsChecked = true;
    }

    private void Mode_Checked(object sender, RoutedEventArgs e)
    {
        bool course = ReferenceEquals(sender, CourseMode);
        CoursePane.Visibility = course ? Visibility.Visible : Visibility.Collapsed;
        WorkbenchPane.Visibility = course ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>Copies a Course task into the Workbench and shows it there (TaskWorkspace's Open in the Workbench).</summary>
    private void OpenTaskInWorkbench(BuiltTask built)
    {
        if (_workspace.OpenTask(built.Task) is { } problem)
        {
            MessageBox.Show(this, problem, "Statistics+ — Could not open in the Workbench", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        WorkbenchMode.IsChecked = true;
    }
}

public partial class MainWindow
{
    private void BuildCoursePane()
    {
        // Filled in once the learning path view is wired (plan 9.2-9.4).
        CoursePane.Content = new System.Windows.Controls.TextBlock
        {
            Text = CourseProblem ?? $"{Course?.Package.Title}: {Course?.Package.Modules.Count} module(s).",
            Margin = new Thickness(16), TextWrapping = TextWrapping.Wrap,
        };
    }

    private void SignOut_Click(object sender, RoutedEventArgs e)
    {
    }
}

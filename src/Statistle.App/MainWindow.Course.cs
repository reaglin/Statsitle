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
    private Statistle.App.Learning.CoursePage? _coursePage;
    private Statistle.App.Learning.LearnerScreen? _learnerScreen;

    /// <summary>The Course page, when the content could be read.</summary>
    public Statistle.App.Learning.CoursePage? CoursePage => _coursePage;

    private void BuildCoursePane()
    {
        if (Course is null)
        {
            CoursePane.Content = new System.Windows.Controls.TextBlock
            {
                Text = CourseProblem, Margin = new Thickness(16), TextWrapping = TextWrapping.Wrap,
            };
            CourseMode.IsEnabled = false;
            CourseMode.ToolTip = CourseProblem;
            return;
        }

        _coursePage = new Statistle.App.Learning.CoursePage(Course);
        _coursePage.Status += text => _workspace.Status = text;
        _coursePage.OpenInWorkbench += OpenTaskInWorkbench;
        _coursePage.SignInRequested += () => ShowLearnerScreen();
        CoursePane.Content = _coursePage;
        Closing += (_, _) => EndSession();
    }

    /// <summary>
    /// Covers the window with "Who is learning?" (plan 9.1). Called by the program at start-up and by Sign out; the
    /// tests never call it, so they never read the real learners' folder.
    /// </summary>
    public void ShowLearnerScreen(string? banner = null)
    {
        if (_coursePage is null) return;
        if (_learnerScreen is null)
        {
            _learnerScreen = new Statistle.App.Learning.LearnerScreen();
            _learnerScreen.SignedIn += file =>
            {
                EndSession();
                _coursePage.SignIn(file);
                LearnerName.Text = file.Data.Profile.DisplayName;
                SignOutButton.Visibility = Visibility.Visible;
                LearnerOverlay.Visibility = Visibility.Collapsed;
                ShowCourse();
                _workspace.Status = $"Welcome, {file.Data.Profile.DisplayName}. Your progress is saved as you go.";
            };
            _learnerScreen.WorkbenchOnly += () =>
            {
                LearnerOverlay.Visibility = Visibility.Collapsed;
                ShowWorkbench();
                _workspace.Status = "The Workbench, without signing in. The Course is still there to read (Ctrl+L), but progress is kept only when you sign in.";
            };
            _learnerScreen.Done += text => _workspace.Status = text;
            LearnerOverlay.Content = _learnerScreen;
        }

        _learnerScreen.Show(banner);
        LearnerOverlay.Visibility = Visibility.Visible;
    }

    /// <summary>Saves the signed-in learner's progress and closes their file.</summary>
    private void EndSession()
    {
        if (_coursePage?.Session is not { } session) return;
        _coursePage.Save();
        _coursePage.SignIn(null);
        session.Dispose();
        LearnerName.Text = "";
        SignOutButton.Visibility = Visibility.Collapsed;
    }

    private void SignOut_Click(object sender, RoutedEventArgs e)
    {
        EndSession();
        ShowLearnerScreen("Signed out. Your progress is saved.");
    }
}

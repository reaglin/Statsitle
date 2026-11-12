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
        if (_workspace.Document.Tools.ContainsKey(built.Task.Analysis)
            && MessageBox.Show(this,
                $"Your project already has entries in {built.Analysis.Title}. Replace them with this example's? " +
                "Its data is added as a new data table, and Edit ▸ Undo (Ctrl+Z) brings your entries back.",
                "Statistics+ — Open in the Workbench", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        if (_workspace.OpenTask(built.Task) is { } problem)
        {
            MessageBox.Show(this, problem, "Statistics+ — Could not open in the Workbench", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        WorkbenchMode.IsChecked = true;
        _workspace.Status += _coursePage?.CurrentItem is { } item ? $" Course (Ctrl+L) takes you back to {item.Number}." : " Course (Ctrl+L) takes you back.";
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
        UpdateLearnerUi();
    }

    /// <summary>
    /// Covers the window with "Who is learning?" (plan 9.1). Called by the program at start-up and by Sign out; the
    /// tests never call it, so they never read the real learners' folder.
    /// </summary>
    public void ShowLearnerScreen(string? banner = null, string? selectProfileId = null)
    {
        if (_coursePage is null) return;
        if (_learnerScreen is null)
        {
            _learnerScreen = new Statistle.App.Learning.LearnerScreen();
            _learnerScreen.SignedIn += SignIn;
            _learnerScreen.WorkbenchOnly += () =>
            {
                // Nobody signed in may read the Course, so the last learner's unchecked answers go now.
                _resume = null;
                _coursePage.ForgetAnswers();
                SetLearnerOverlay(false);
                ShowWorkbench();
                _workspace.Status = "The Workbench, without signing in. The Course is still there to read (Ctrl+L), but progress is kept only when you sign in.";
            };
            _learnerScreen.Done += text => _workspace.Status = text;
            LearnerOverlay.Content = _learnerScreen;
            PreviewKeyDown += Overlay_PreviewKeyDown;
            StartIdleLock();
        }

        _learnerScreen.Show(banner, selectProfileId);
        SetLearnerOverlay(true);
    }

    /// <summary>Signs a learner in, as "Who is learning?" does after the PIN (also the Store-screenshot test).</summary>
    public void SignIn(global::Learning.Core.Learners.LearnerFile file)
    {
        if (_coursePage is null) return;
        // The learner the idle lock signed out, coming back: their item (and the Workbench, if they were there) again.
        var resume = _resume is { } r && r.ProfileId == file.Header.ProfileId ? r : null;
        _resume = null;
        EndSession();
        _coursePage.SignIn(file);
        UpdateLearnerUi();
        SetLearnerOverlay(false);
        if (resume?.Item is { } item) _coursePage.Path.Select(item);
        if (resume is { Workbench: true }) ShowWorkbench();
        else ShowCourse();
        _workspace.Status = resume is null
            ? $"Welcome, {file.Data.Profile.DisplayName}. Your progress is saved as you go."
            : $"Welcome back, {file.Data.Profile.DisplayName}. You are where you left off, with the answers you were typing.";
    }

    /// <summary>Where the idle lock found a learner, so that learner comes back to it.</summary>
    private sealed record Resume(string ProfileId, global::Learning.Core.Content.LearningItem? Item, bool Workbench);

    private Resume? _resume;

    /// <summary>
    /// Shows or hides "Who is learning?". While it shows, the menu bar and the Course | Workbench strip are off, so nothing
    /// behind it can be used and their access keys do not compete with the screen's (UX sweep, plan 15.1).
    /// </summary>
    private void SetLearnerOverlay(bool visible)
    {
        LearnerOverlay.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        MainMenu.IsEnabled = !visible;
        ModeStrip.IsEnabled = !visible;
        // The status line under the start screen speaks about it, not about the page hidden behind it.
        if (visible) _workspace.Status = "Who is learning? Choose your name, type your PIN and press Open; or press New learner.";
    }

    /// <summary>Whether "Who is learning?" covers the window.</summary>
    public bool IsLearnerScreenShown => LearnerOverlay.Visibility == Visibility.Visible;

    /// <summary>
    /// Keys while "Who is learning?" covers the window: F1 shows the screen's own help; the window's shortcuts (Ctrl+N, O,
    /// S, Z, L, B) would act on what is hidden behind it, so they do nothing.
    /// </summary>
    private void Overlay_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsLearnerScreenShown || _learnerScreen is null) return;
        if (e.Key == Key.F1)
        {
            _learnerScreen.ShowHelp();
            e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key is Key.N or Key.O or Key.S or Key.Z or Key.L or Key.B)
        {
            e.Handled = true;
        }
    }

    /// <summary>The strip and the Learner menu follow who is signed in.</summary>
    private void UpdateLearnerUi()
    {
        var session = _coursePage?.Session;
        LearnerName.Text = session is null ? (_coursePage is null ? "" : "Not signed in") : session.Data.Profile.DisplayName;
        SignOutButton.Visibility = session is null ? Visibility.Collapsed : Visibility.Visible;
        SignInButton.Visibility = session is null && _coursePage is not null ? Visibility.Visible : Visibility.Collapsed;
    }

    private void LearnerMenu_Opened(object sender, RoutedEventArgs e)
    {
        var session = _coursePage?.Session;
        SignInMenu.IsEnabled = _coursePage is not null && session is null;
        SignInMenu.ToolTip = _coursePage is null ? "The Course could not be read, so there is nothing to sign in to"
            : session is null ? "Opens Who is learning?, so your progress in the Course is kept" : "Already signed in. Sign out first to sign in as someone else.";
        SignOutMenu.IsEnabled = session is not null;
        SignOutMenu.ToolTip = session is null ? "Nobody is signed in" : "Saves your progress, locks your file and goes back to Who is learning? (for the next student, or to switch learner)";
        HandInMenu.IsEnabled = session is not null;
        HandInMenu.ToolTip = _coursePage is null ? "The Course could not be read, so there is nothing to report"
            : session is null ? "Sign in first: a report shows your own progress" : "Makes a signed progress report (PDF) of a module check, or of everything so far, to hand in to your instructor";
        ChangePinMenu.IsEnabled = session is not null;
        ChangePinMenu.ToolTip = session is null ? "Sign in first: the PIN is the one that opens your own progress" : "Sets a new PIN; you type the one you use now first.";
        BackupMenu.IsEnabled = session is not null;
        BackupMenu.ToolTip = session is null ? "Sign in first: the backup is of your own progress file" : "Saves a copy of your progress file to a USB drive or a cloud folder, in case this computer is changed";
    }

    private void SignIn_Click(object sender, RoutedEventArgs e) => ShowLearnerScreen();

    /// <summary>Learner ▸ Hand in a progress report (plan 14.1): the report page in the Course.</summary>
    private void HandIn_Click(object sender, RoutedEventArgs e)
    {
        if (_coursePage is null) return;
        ShowCourse();
        _coursePage.ShowReports(_coursePage.CurrentItem is { Kind: global::Learning.Core.Content.ItemKind.Assess } check ? check.Id : null);
    }

    /// <summary>File ▸ Check reports and class codes (for instructors) (plan 14.2): works without the Course and without signing in.</summary>
    private void CheckReport_Click(object sender, RoutedEventArgs e) => new Statistle.App.Learning.VerifyWindow(this).ShowDialog();

    private void Backup_Click(object sender, RoutedEventArgs e)
    {
        if (_coursePage?.Session is not { } session) return;
        _coursePage.Save();
        if (Statistle.App.Learning.Backups.Run(this, session) is { } done) _workspace.Status = done;
    }

    // The idle lock (as PLC+): a signed-in learner who walks away from a shared computer is signed out.
    private DateTime _lastInput = DateTime.UtcNow;
    private System.Windows.Threading.DispatcherTimer? _idleTimer;

    private void StartIdleLock()
    {
        System.Windows.Input.InputManager.Current.PreProcessInput += (_, _) => _lastInput = DateTime.UtcNow;
        _idleTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _idleTimer.Tick += (_, _) =>
        {
            var minutes = _learnerScreen?.Settings.LockMinutes ?? 15;
            if (_coursePage?.Session is not { } session || minutes <= 0 || DateTime.UtcNow - _lastInput < TimeSpan.FromMinutes(minutes)) return;
            var id = session.Header.ProfileId;
            _resume = new Resume(id, _coursePage.CurrentItem, WorkbenchMode.IsChecked == true);
            EndSession();
            ShowLearnerScreen($"Statistics+ signed you out after {minutes} minutes without use, so nobody else could use your progress. " +
                              "Your progress and the answers you were typing are kept: type your PIN to go on. " +
                              "(Answers you have not checked yet are kept only while Statistics+ stays open.)", id);
        };
        _idleTimer.Start();
    }

    /// <summary>Saves the signed-in learner's progress and closes their file.</summary>
    private void EndSession()
    {
        if (_coursePage?.Session is not { } session) return;
        _coursePage.Save();
        _coursePage.SignIn(null);
        session.Dispose();
        UpdateLearnerUi();
    }

    /// <summary>Learner ▸ Change my PIN (UX sweep, plan 15.1): the PIN now, then the new one twice.</summary>
    private void ChangePin_Click(object sender, RoutedEventArgs e)
    {
        if (_coursePage?.Session is not { } session) return;
        var w = new Statistle.App.Learning.NewPinWindow(this, recovering: false, session.Data.Profile.DisplayName, (now, newPin) =>
        {
            if (!session.IsPin(now)) return "That is not the PIN you use now. Type it again; if you have forgotten it, sign out and press Forgot my PIN on the start screen.";
            try
            {
                session.ChangePin(newPin, DateTimeOffset.Now);
                return null;
            }
            catch (Exception ex) when (ex is ArgumentException or IOException or UnauthorizedAccessException)
            {
                return $"The PIN could not be changed: {ex.Message}";
            }
        });
        if (w.ShowDialog() == true)
            _workspace.Status = "Your new PIN is set: use it from now on. Backups made before now still open with your old PIN.";
    }

    private void SignOut_Click(object sender, RoutedEventArgs e)
    {
        _resume = null;
        EndSession();
        ShowLearnerScreen("Signed out. Your progress is saved.");
    }
}

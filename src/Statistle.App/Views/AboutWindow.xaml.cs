using System.Windows;

namespace Statistle.App.Views;

/// <summary>
/// Help ▸ About Statistle: who made it, what it is for, and the version — read from the
/// assembly, which takes it from <c>StatistleVersion</c> in Directory.Build.props.
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        VersionText.Text = $"Version {AppVersion}";
    }

    /// <summary>The program's version as a person reads it, e.g. "1.0.0".</summary>
    public static string AppVersion
    {
        get
        {
            Version? version = typeof(AboutWindow).Assembly.GetName().Version;
            return version is null ? "unknown" : version.ToString(3);
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}

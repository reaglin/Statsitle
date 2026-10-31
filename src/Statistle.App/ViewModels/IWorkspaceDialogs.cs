using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Statistle.App.ViewModels;

/// <summary>What the person answers when unsaved work would be lost.</summary>
public enum UnsavedChoice
{
    /// <summary>Save the project first, then go on.</summary>
    Save,

    /// <summary>Throw the changes away and go on.</summary>
    Discard,

    /// <summary>Go back; nothing happens.</summary>
    Cancel,
}

/// <summary>
/// Every question and message box the workspace puts to the person, behind one seam so the
/// tests can answer them (UX review 2026-10-31: unsaved work, open and save failures).
/// </summary>
public interface IWorkspaceDialogs
{
    /// <summary>"Save changes to {file} before {action}?" — Yes, No or Cancel.</summary>
    UnsavedChoice AskToSave(string fileName, string beforeWhat);

    /// <summary>A yes/no question; true for Yes.</summary>
    bool Confirm(string title, string message);

    /// <summary>Tells the person something went wrong, and what to do about it.</summary>
    void ShowError(string title, string message);

    /// <summary>The project file to open, or null when the person cancels.</summary>
    string? ChooseProjectToOpen(string initialFolder);

    /// <summary>Where to save the project, or null when the person cancels.</summary>
    string? ChooseProjectSavePath(string suggestedName, string initialFolder);
}

/// <summary>The real dialogs: Windows message boxes and file pickers over the main window.</summary>
public sealed class WpfWorkspaceDialogs : IWorkspaceDialogs
{
    private const string Filter = "Statistle project (*.statistle)|*.statistle|All files (*.*)|*.*";

    /// <summary>"Statistle — {subject}" for every box (USABILITY-CHECKLIST 6.6).</summary>
    private static string Titled(string title) =>
        title.StartsWith("Statistle", StringComparison.Ordinal) ? title : "Statistle — " + title;

    private static Window? Owner => Application.Current?.MainWindow is { IsLoaded: true } window ? window : null;

    public UnsavedChoice AskToSave(string fileName, string beforeWhat)
    {
        string message =
            $"Save changes to {fileName} before {beforeWhat}?\n\n" +
            "Yes saves them, No discards them, Cancel goes back.";

        MessageBoxResult answer = Owner is Window owner
            ? MessageBox.Show(owner, message, "Statistle", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes)
            : MessageBox.Show(message, "Statistle", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes);

        return answer switch
        {
            MessageBoxResult.Yes => UnsavedChoice.Save,
            MessageBoxResult.No => UnsavedChoice.Discard,
            _ => UnsavedChoice.Cancel,
        };
    }

    public bool Confirm(string title, string message) => Ask(Titled(title), message);

    private static bool Ask(string title, string message) =>
        (Owner is Window owner
            ? MessageBox.Show(owner, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No)
            : MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No))
        == MessageBoxResult.Yes;

    public void ShowError(string title, string message)
    {
        title = Titled(title);
        if (Owner is Window owner)
        {
            MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public string? ChooseProjectToOpen(string initialFolder)
    {
        var dialog = new OpenFileDialog
        {
            Filter = Filter,
            DefaultExt = ".statistle",
            InitialDirectory = initialFolder,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? ChooseProjectSavePath(string suggestedName, string initialFolder)
    {
        var dialog = new SaveFileDialog
        {
            Filter = Filter,
            DefaultExt = ".statistle",
            FileName = Path.GetFileName(suggestedName),
            InitialDirectory = initialFolder,
        };

        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}

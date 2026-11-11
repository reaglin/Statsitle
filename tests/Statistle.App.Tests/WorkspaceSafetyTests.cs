using System.IO;
using Statistle.App.ViewModels;
using Statistle.Core.Analyses;
using Statistle.Core.Document;
using Statistle.Core.Domain;

namespace Statistle.App.Tests;

/// <summary>
/// UX review 2026-10-31: work is never lost by surprise (C1), undo belongs to one project (C2),
/// what is typed is what is saved (M3), and only a data table can be deleted (M8).
/// </summary>
public sealed class WorkspaceSafetyTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "statistle-tests-" + Guid.NewGuid().ToString("N"));
    private readonly string _autosave;
    private readonly FakeDialogs _dialogs = new();

    public WorkspaceSafetyTests()
    {
        Directory.CreateDirectory(_folder);
        _autosave = Path.Combine(_folder, "autosave");
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_folder, recursive: true);
        }
        catch (IOException)
        {
        }
    }

    private WorkspaceViewModel Workspace() => new(_dialogs, _autosave);

    private string FileIn(string name) => Path.Combine(_folder, name);

    private static WorkspaceViewModel WithTable(WorkspaceViewModel workspace)
    {
        workspace.AddTableCommand.Execute(null);
        Assert.True(workspace.IsDirty);
        return workspace;
    }

    // ── C1: New, Open and Close ask before throwing work away ────────────────

    [Fact]
    public void A_clean_project_is_left_without_a_question()
    {
        WorkspaceViewModel workspace = Workspace();

        Assert.True(workspace.ConfirmDiscard("closing"));
        Assert.Empty(_dialogs.Asked);
    }

    [Fact]
    public void New_with_unsaved_changes_asks_and_Cancel_keeps_the_work()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        _dialogs.Answers.Enqueue(UnsavedChoice.Cancel);

        workspace.NewCommand.Execute(null);

        Assert.Equal("starting a new project", Assert.Single(_dialogs.Asked).BeforeWhat);
        Assert.Single(workspace.Tables);
        Assert.True(workspace.IsDirty);
    }

    [Fact]
    public void New_with_No_discards_the_changes_and_the_recovery_copy()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        Assert.True(workspace.HasRecovery);
        _dialogs.Answers.Enqueue(UnsavedChoice.Discard);

        workspace.NewCommand.Execute(null);

        Assert.Empty(workspace.Tables);
        Assert.False(workspace.IsDirty);
        Assert.False(workspace.HasRecovery);
    }

    [Fact]
    public void Yes_saves_first_and_goes_on_only_when_the_save_worked()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        string file = FileIn("first.statistle");

        // Untitled: Yes asks where; cancelling that picker means nothing happens.
        _dialogs.Answers.Enqueue(UnsavedChoice.Save);
        _dialogs.SavePaths.Enqueue(null);
        Assert.False(workspace.ConfirmDiscard("closing"));
        Assert.True(workspace.IsDirty);

        _dialogs.Answers.Enqueue(UnsavedChoice.Save);
        _dialogs.SavePaths.Enqueue(file);
        Assert.True(workspace.ConfirmDiscard("closing"));
        Assert.False(workspace.IsDirty);
        Assert.Single(StatistleDocument.Open(file).Tables);
        Assert.Contains("the untitled project", _dialogs.Asked[0].FileName, StringComparison.Ordinal);
    }

    [Fact]
    public void Open_with_unsaved_changes_and_Cancel_leaves_the_project_open()
    {
        string other = FileIn("other.statistle");
        new StatistleDocument { Name = "Other" }.Save(other);

        WorkspaceViewModel workspace = WithTable(Workspace());
        _dialogs.OpenPaths.Enqueue(other);
        _dialogs.Answers.Enqueue(UnsavedChoice.Cancel);

        workspace.OpenCommand.Execute(null);

        Assert.Single(workspace.Tables);
        Assert.Null(workspace.Document.FilePath);
        Assert.Equal("opening another project", Assert.Single(_dialogs.Asked).BeforeWhat);
    }

    [Fact]
    public void Unsaved_work_left_behind_is_offered_back_on_the_next_start()
    {
        string file = FileIn("mine.statistle");
        WorkspaceViewModel first = WithTable(Workspace());
        _dialogs.SavePaths.Enqueue(file);
        first.SaveAsCommand.Execute(null);
        first.AddTableCommand.Execute(null);   // two tables now, the second unsaved — then a "crash"

        WorkspaceViewModel next = Workspace();
        Assert.True(next.HasRecovery);
        _dialogs.Confirms.Enqueue(true);

        Assert.True(next.OfferRecovery());
        Assert.Equal(2, next.Tables.Count);
        Assert.True(next.IsDirty);
        Assert.Equal(file, next.Document.FilePath);
        Assert.Contains("mine.statistle", _dialogs.ConfirmMessages[0], StringComparison.Ordinal);
    }

    [Fact]
    public void Declining_the_recovery_copy_deletes_it()
    {
        WithTable(Workspace());
        WorkspaceViewModel next = Workspace();
        _dialogs.Confirms.Enqueue(false);

        Assert.False(next.OfferRecovery());
        Assert.False(next.HasRecovery);
        Assert.Empty(next.Tables);
    }

    [Fact]
    public void Saving_removes_the_recovery_copy()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        _dialogs.SavePaths.Enqueue(FileIn("saved.statistle"));

        workspace.SaveCommand.Execute(null);

        Assert.False(workspace.HasRecovery);
    }

    [Fact]
    public void A_file_that_will_not_open_says_so_and_keeps_the_current_project()
    {
        string broken = FileIn("broken.statistle");
        File.WriteAllText(broken, "this is not a zip");
        WorkspaceViewModel workspace = WithTable(Workspace());

        Assert.False(workspace.OpenPath(broken));

        Assert.Single(workspace.Tables);
        Assert.Contains("broken.statistle", Assert.Single(_dialogs.Errors), StringComparison.Ordinal);
    }

    // ── C2: undo is cleared by New and Open ──────────────────────────────────

    [Fact]
    public void New_clears_undo()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        Assert.True(workspace.UndoCommand.CanExecute(null));
        _dialogs.Answers.Enqueue(UnsavedChoice.Discard);

        workspace.NewCommand.Execute(null);

        Assert.False(workspace.UndoCommand.CanExecute(null));
    }

    [Fact]
    public void Open_clears_undo_so_the_old_project_cannot_overwrite_the_new_file()
    {
        string other = FileIn("other.statistle");
        var otherDocument = new StatistleDocument { Name = "Other" };
        otherDocument.AddTable("Kept");
        otherDocument.Save(other);

        WorkspaceViewModel workspace = WithTable(Workspace());
        _dialogs.SavePaths.Enqueue(FileIn("mine.statistle"));
        workspace.SaveCommand.Execute(null);

        Assert.True(workspace.OpenPath(other));

        Assert.False(workspace.UndoCommand.CanExecute(null));
        workspace.UndoCommand.Execute(null);
        Assert.Equal("Kept", Assert.Single(workspace.Tables).Name);
        Assert.Equal(other, workspace.Document.FilePath);
    }

    // ── M3: the value on screen is the value saved ───────────────────────────

    [Fact]
    public void Save_first_commits_what_is_typed_in_the_focused_box()
    {
        string file = FileIn("typed.statistle");
        WorkspaceViewModel workspace = WithTable(Workspace());

        // What the window does: push the focused box's text into its row.
        workspace.CommitPendingEdit = () =>
            Assert.Single(workspace.Rows, r => r.Property.XmlName == "Name").Text = "Tensile strength";

        _dialogs.SavePaths.Enqueue(file);
        workspace.SaveCommand.Execute(null);

        Assert.Equal("Tensile strength", Assert.Single(StatistleDocument.Open(file).Tables).Name);
        Assert.False(workspace.IsDirty);
    }

    [Fact]
    public void Closing_commits_the_typed_value_before_deciding_whether_to_ask()
    {
        string file = FileIn("clean.statistle");
        WorkspaceViewModel workspace = WithTable(Workspace());
        _dialogs.SavePaths.Enqueue(file);
        workspace.SaveCommand.Execute(null);
        Assert.False(workspace.IsDirty);

        workspace.CommitPendingEdit = () =>
            Assert.Single(workspace.Rows, r => r.Property.XmlName == "Name").Text = "Renamed";
        _dialogs.Answers.Enqueue(UnsavedChoice.Cancel);

        Assert.False(workspace.ConfirmDiscard("closing"));
        Assert.Single(_dialogs.Asked);
    }

    // ── M4: a rejected entry says why ────────────────────────────────────────

    [Fact]
    public void A_rejected_number_says_what_is_still_used()
    {
        WorkspaceViewModel workspace = Workspace();
        workspace.SelectTool(Assert.Single(workspace.Analyses, a => a.ToolType == typeof(ContinuousDistribution)));
        PropertyRow row = workspace.Rows.First(r =>
            !r.IsReadOnly && !r.IsSeries && !r.IsTable && !r.IsString && !r.IsEnum && !r.IsChoice
            && r.Property.Property.PropertyType == typeof(double));
        string before = row.Text;

        row.Text = "12,5x";

        Assert.False(row.IsValid);
        Assert.Equal($"'12,5x' is not a number, so Statistics+ is still using {before}. Correct it or press Esc.", row.RejectionMessage);
        Assert.StartsWith(row.RejectionMessage, row.HelpText, StringComparison.Ordinal);

        row.CancelEntry();
        Assert.False(row.HasRejection);
        Assert.True(row.IsValid);
    }

    [Fact]
    public void A_rejected_table_paste_names_the_line_and_column_and_keeps_the_data()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        PropertyRow data = Assert.Single(workspace.Rows, r => r.IsTable);
        data.Text = "a\tb\n1\t2";

        data.Text = "a\tb\n1\t2\n\n3\tx";

        Assert.Equal(
            "Line 4, column 'b': 'x' is not a number. The paste was not used; your earlier data is kept. " +
            "Correct that cell, or press Esc to go back to the data in use.",
            data.RejectionMessage);
        Assert.Equal(1, data.Table!.RowCount);
    }

    // ── M8: only a data table is deleted, and only after asking ──────────────

    [Fact]
    public void Delete_is_offered_for_a_data_table_only_and_asks_first()
    {
        WorkspaceViewModel workspace = WithTable(Workspace());
        DataTable table = Assert.Single(workspace.Tables);

        workspace.Selected = table;
        Assert.True(workspace.DeleteCommand.CanExecute(null));

        _dialogs.Confirms.Enqueue(false);
        workspace.DeleteCommand.Execute(null);
        Assert.Single(workspace.Tables);
        Assert.Contains(table.Name, _dialogs.ConfirmMessages[0], StringComparison.Ordinal);

        _dialogs.Confirms.Enqueue(true);
        workspace.DeleteCommand.Execute(null);
        Assert.Empty(workspace.Tables);

        workspace.SelectTool(workspace.Analyses[0]);
        Assert.False(workspace.DeleteCommand.CanExecute(null));
    }

    [Fact]
    public void Counts_read_as_words()
    {
        Assert.Equal("1 data table", WorkspaceViewModel.Count(1, "data table", "data tables"));
        Assert.Equal("0 analyses", WorkspaceViewModel.Count(0, "analysis", "analyses"));
    }

    internal sealed class FakeDialogs : IWorkspaceDialogs
    {
        public Queue<UnsavedChoice> Answers { get; } = new();

        public Queue<bool> Confirms { get; } = new();

        public Queue<string?> OpenPaths { get; } = new();

        public Queue<string?> SavePaths { get; } = new();

        public List<(string FileName, string BeforeWhat)> Asked { get; } = [];

        public List<string> ConfirmMessages { get; } = [];

        public List<string> Errors { get; } = [];

        public UnsavedChoice AskToSave(string fileName, string beforeWhat)
        {
            Asked.Add((fileName, beforeWhat));
            return Answers.Count > 0 ? Answers.Dequeue() : UnsavedChoice.Cancel;
        }

        public bool Confirm(string title, string message)
        {
            ConfirmMessages.Add(message);
            return Confirms.Count > 0 && Confirms.Dequeue();
        }

        public void ShowError(string title, string message) => Errors.Add(message);

        public string? ChooseProjectToOpen(string initialFolder) => OpenPaths.Count > 0 ? OpenPaths.Dequeue() : null;

        public string? ChooseProjectSavePath(string suggestedName, string initialFolder) =>
            SavePaths.Count > 0 ? SavePaths.Dequeue() : null;
    }
}

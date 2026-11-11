using System.IO;
using Statistle.App.ViewModels;
using Statistle.Core.Analyses;
using Statistle.Learning;

namespace Statistle.App.Tests;

/// <summary>Plan 10.3: Open in the Workbench copies a Course task into the open project, and Undo takes it out.</summary>
public sealed class OpenTaskTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "statistle-tests-" + Guid.NewGuid().ToString("N"));

    public OpenTaskTests() => Directory.CreateDirectory(_folder);

    public void Dispose()
    {
        try { Directory.Delete(_folder, recursive: true); }
        catch (IOException) { }
    }

    private const string Task = """
        {
          "id": "test.open",
          "title": "Bolts from a table",
          "analysis": "DescriptiveStatistics",
          "tables": [ { "name": "Bolts", "columns": { "Length": [3, 4, 4, 5, 9] } } ],
          "settings": { "Column": "@Bolts/Length" },
          "ask": [ "Mean" ]
        }
        """;

    [Fact]
    public void A_task_lands_in_the_project_as_a_table_and_the_analysis_reading_it()
    {
        var workspace = new WorkspaceViewModel(new WorkspaceSafetyTests.FakeDialogs(), Path.Combine(_folder, "autosave"));

        Assert.Null(workspace.OpenTask(StatsTask.Parse(Task)));

        var table = Assert.Single(workspace.Document.Tables, t => t.Name == "Bolts");
        Assert.Equal(5, table.Columns.Column("Length")!.Count);
        var analysis = Assert.IsType<DescriptiveStatistics>(workspace.Selected);
        Assert.Equal(5.0, analysis.Mean, 12);
        Assert.True(workspace.IsDirty);

        // A second copy gets its own table rather than overwriting the first.
        Assert.Null(workspace.OpenTask(StatsTask.Parse(Task)));
        Assert.Contains(workspace.Document.Tables, t => t.Name == "Bolts (2)");

        workspace.UndoCommand.Execute(null);
        Assert.DoesNotContain(workspace.Document.Tables, t => t.Name == "Bolts (2)");
    }
}

using Statistle.Core.Document;

namespace Statistle.Tests;

/// <summary>
/// Where files go: Documents\Statistle by default, wherever the exe is, and a save creates the
/// folder it is aimed at when it does not exist yet (Ron, 2026-09-08 — SMADA's pattern).
/// </summary>
public class PathsTests
{
    [Fact]
    public void The_default_folder_is_Statistle_under_the_users_Documents()
    {
        string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        Assert.Equal(Path.Combine(documents, "Statistle"), StatistlePaths.DocumentsFolder);
        Assert.Equal("Statistle", StatistlePaths.FolderName);
    }

    [Fact]
    public void Saving_into_a_folder_that_does_not_exist_creates_it()
    {
        string root = Path.Combine(Path.GetTempPath(), $"statistle-paths-{Guid.NewGuid():N}");
        string path = Path.Combine(root, "nested", "first.statistle");

        try
        {
            var document = new StatistleDocument();
            document.AddTable().Name = "A table";

            Assert.False(Directory.Exists(root));
            document.Save(path);

            Assert.True(File.Exists(path));
            Assert.Equal("A table", StatistleDocument.Open(path).Tables.Single().Name);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }

    [Fact]
    public void The_demonstration_project_is_installed_once_and_never_overwritten()
    {
        string folder = Path.Combine(Path.GetTempPath(), $"statistle-samples-{Guid.NewGuid():N}");
        string path = Path.Combine(folder, SampleProjects.DemonstrationFileName);

        try
        {
            Assert.True(SampleProjects.Install(folder));
            StatistleDocument demo = StatistleDocument.Open(path);
            Assert.Equal(4, demo.Tables.Count);
            Assert.Equal(5, demo.Tools.Count);
            Assert.False(demo.Diagnostics.HasWarnings, demo.Diagnostics.ToString());

            File.WriteAllText(path, "edited by the user");
            Assert.False(SampleProjects.Install(folder));
            Assert.Equal("edited by the user", File.ReadAllText(path));
        }
        finally
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
    }
}

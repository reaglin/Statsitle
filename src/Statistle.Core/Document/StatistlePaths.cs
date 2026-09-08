namespace Statistle.Core.Document;

/// <summary>
/// Where Statistle's files live. <c>Documents\Statistle</c> is the default folder for every user
/// file — projects, exported reports, sample data — wherever the exe happens to be. Recovery
/// copies (autosave) are app data and stay in local app data.
/// </summary>
public static class StatistlePaths
{
    public const string FolderName = "Statistle";

    /// <summary>The user's <c>Documents\Statistle</c>. May not exist yet; see <see cref="EnsureDocumentsFolder"/>.</summary>
    public static string DocumentsFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FolderName);

    /// <summary>Creates the folder if it is missing and returns it.</summary>
    public static string EnsureDocumentsFolder()
    {
        Directory.CreateDirectory(DocumentsFolder);
        return DocumentsFolder;
    }
}

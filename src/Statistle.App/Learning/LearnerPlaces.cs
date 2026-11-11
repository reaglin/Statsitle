using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using Learning.Core.Learners;

namespace Statistle.App.Learning;

/// <summary>Statistics+'s settings for this computer: where learners' files are kept, and when it locks itself (PLC+ plan 6.4, 6.8).</summary>
public sealed class LearnerSettings
{
    /// <summary>Null: the usual folder, %ProgramData%\Statistle\Learners.</summary>
    public string? LearnersFolder { get; set; }
    public int LockMinutes { get; set; } = 15;

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static LearnerSettings Load()
    {
        try
        {
            return File.Exists(LearnerPlaces.SettingsFile)
                ? JsonSerializer.Deserialize<LearnerSettings>(File.ReadAllText(LearnerPlaces.SettingsFile), Json) ?? new LearnerSettings()
                : new LearnerSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new LearnerSettings();
        }
    }

    public void Save()
    {
        LearnerPlaces.EnsureShared(LearnerPlaces.DataFolder);
        File.WriteAllText(LearnerPlaces.SettingsFile, JsonSerializer.Serialize(this, Json));
    }

    public string Folder => string.IsNullOrWhiteSpace(LearnersFolder) ? LearnerPlaces.DefaultLearnersFolder : LearnersFolder;
}

/// <summary>
/// Where Statistics+ keeps learners' files: by default %ProgramData%\Statistle\Learners, one folder every Windows account on
/// the computer can use, so lab PCs work whether students share one login or have their own (as PLC+, Ron's question 10).
/// The folder and the files keep the code name Statistle; the person reads Statistics+ (plan D1).
/// STATISTLE_DATA points a test run at a folder of its own.
/// </summary>
public static class LearnerPlaces
{
    /// <summary>What a person reads: the program's name.</summary>
    public const string ProgramName = "Statistics+";

    public const string Extension = ".statprogress";

    /// <summary>The Open and Save dialogs' filter for progress files.</summary>
    public const string FileFilter = "Statistics+ progress files (*.statprogress)|*.statprogress";

    /// <summary>The environment variable that moves the data folder for a test run.</summary>
    public const string DataVariable = "STATISTLE_DATA";

    public static string DataFolder { get; } = Environment.GetEnvironmentVariable(DataVariable) is { Length: > 0 } test ? test
        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Statistle");

    public static string DefaultLearnersFolder => Path.Combine(DataFolder, "Learners");
    public static string SettingsFile => Path.Combine(DataFolder, "settings.json");

    /// <summary>The version as Help ▸ About shows it (from StatistleVersion in Directory.Build.props), such as 2.0.0.</summary>
    public static string AppVersion => Views.AboutWindow.AppVersion;

    /// <summary>The file options: the app's code name "Statistle" goes in each file, so PLC+ and Statistics+ files are told apart.</summary>
    public static LearnerFileOptions Options { get; } = new("Statistle", Extension, AppVersion, Environment.MachineName);

    /// <summary>
    /// Makes the folder, and lets every account on the computer change the files in it: by default a file under ProgramData
    /// can be changed only by the account that made it, which would lock a lab's second Windows account out.
    /// </summary>
    public static void EnsureShared(string folder)
    {
        if (Directory.Exists(folder)) return;
        var info = Directory.CreateDirectory(folder);
        var common = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (!Path.GetFullPath(folder).StartsWith(Path.GetFullPath(common), StringComparison.OrdinalIgnoreCase)) return;
        try
        {
            var security = info.GetAccessControl();
            security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null),
                FileSystemRights.Modify, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow));
            info.SetAccessControl(security);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or IOException or InvalidOperationException)
        {
            // The folder works for this account; another account may not be able to change its files. Settings can point elsewhere.
        }
    }

    /// <summary>Removes a file to the Recycle Bin, so a learner removed by mistake can be brought back.</summary>
    public static void Recycle(string path)
    {
        var op = new ShFileOp
        {
            Func = 3, // FO_DELETE
            From = Path.GetFullPath(path) + "\0\0",
            Flags = 0x0040 | 0x0010 | 0x0004 | 0x0400, // ALLOWUNDO | NOCONFIRMATION | SILENT | NOERRORUI
        };
        var result = SHFileOperationW(ref op);
        if (result != 0 || File.Exists(path)) File.Delete(path); // no Recycle Bin on this drive: delete
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileOp
    {
        public IntPtr Hwnd;
        public uint Func;
        [MarshalAs(UnmanagedType.LPWStr)] public string From;
        [MarshalAs(UnmanagedType.LPWStr)] public string? To;
        public ushort Flags;
        [MarshalAs(UnmanagedType.Bool)] public bool AnyOperationsAborted;
        public IntPtr NameMappings;
        [MarshalAs(UnmanagedType.LPWStr)] public string? ProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperationW(ref ShFileOp op);
}

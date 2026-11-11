using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Windows.Services.Store;

namespace Statistle.App;

/// <summary>
/// Help ▸ Check for a new version (Ron, 2026-10-30, for every Store program; copied from Accessibility+). Asks the Microsoft Store whether a newer version
/// is published; if so, asks the person, then lets the Store download and install it (Windows shows its own progress). A copy
/// not installed from the Store (a hand-test build) cannot ask, and says so, offering the Store page instead.
/// </summary>
public static class StoreUpdates
{
    public const string ProgramName = "Statistics+";
    public const string StorePage = "https://apps.microsoft.com/detail/9MV608S7M9X6";

    /// <summary>The version running now, as people read it (1.0.0) — the same text Help ▸ About shows.</summary>
    public static string CurrentVersion => Views.AboutWindow.AppVersion;

    /// <param name="beforeInstall">Asked just before the install, which closes the program: the save question. False stops it.</param>
    /// <param name="announce">Puts "Asking the Microsoft Store…" and the outcome in the status line.</param>
    public static async Task CheckAsync(Window owner, Func<bool> beforeInstall, Action<string> announce)
    {
        ArgumentNullException.ThrowIfNull(beforeInstall);
        ArgumentNullException.ThrowIfNull(announce);

        const string title = ProgramName + " — Check for a new version";
        if (!IsFromStore())
        {
            if (MessageBox.Show(owner,
                    $"This copy of {ProgramName} ({CurrentVersion}) was not installed from the Microsoft Store, so it cannot check for a " +
                    "new version itself.\n\nOpen the program's page on the Microsoft Store to see the latest version?",
                    title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                OpenStorePage(owner);
            return;
        }

        IReadOnlyList<StorePackageUpdate> updates;
        StoreContext store;
        announce("Asking the Microsoft Store…");
        System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
        try
        {
            store = StoreContext.GetDefault();
            // A desktop program must tell the Store which window its own dialogs belong to.
            WinRT.Interop.InitializeWithWindow.Initialize(store, new WindowInteropHelper(owner).Handle);
            updates = await store.GetAppAndOptionalStorePackageUpdatesAsync();
        }
        catch (Exception ex)
        {
            System.Windows.Input.Mouse.OverrideCursor = null;
            announce("The Microsoft Store could not be asked about a new version.");
            if (MessageBox.Show(owner,
                    "The Microsoft Store could not be asked about a new version (are you connected to the internet?).\n\n" +
                    $"Open {ProgramName}'s page on the Microsoft Store instead?\n\nDetails for support: {ex.Message}",
                    title, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                OpenStorePage(owner);
            return;
        }
        finally
        {
            System.Windows.Input.Mouse.OverrideCursor = null;
        }

        if (updates.Count == 0)
        {
            announce($"You have the latest version of {ProgramName} ({CurrentVersion}).");
            MessageBox.Show(owner, $"You have the latest version of {ProgramName} ({CurrentVersion}).", title,
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (MessageBox.Show(owner,
                $"A new version of {ProgramName} is on the Microsoft Store (you have {CurrentVersion}).\n\n" +
                $"Download and install it now? Windows closes {ProgramName} to install it; open it again from the Start menu. " +
                "If you have unsaved changes, you are asked to save them first. (No: the Store installs it later on its own.)",
                title, MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        // The install closes the program: unsaved work is saved or deliberately discarded first.
        if (!beforeInstall())
        {
            announce("The new version was not installed; your project is still open.");
            return;
        }

        try
        {
            var result = await store.RequestDownloadAndInstallStorePackageUpdatesAsync(updates);
            string message = result.OverallState switch
            {
                StorePackageUpdateState.Completed => $"The new version is installed. Close {ProgramName} and open it again to use it.",
                StorePackageUpdateState.Canceled => "The update was cancelled. You can check again at any time, or the Store will install it later.",
                _ => "The update did not finish. The Microsoft Store will try again later, or open " +
                     $"{ProgramName}'s page on the Store and press Update.",
            };
            MessageBox.Show(owner, message, title, MessageBoxButton.OK,
                result.OverallState == StorePackageUpdateState.Completed ? MessageBoxImage.Information : MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(owner, $"The update could not be started. Open {ProgramName}'s page on the Microsoft Store " +
                $"and press Update instead.\n\nDetails for support: {ex.Message}", title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private static void OpenStorePage(Window owner)
    {
        try { Process.Start(new ProcessStartInfo(StorePage) { UseShellExecute = true }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(owner, $"Windows could not open a web browser. The Store page is at {StorePage}", ProgramName + " — Check for a new version",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>True when the program runs from its Store package (it has a package identity).</summary>
    public static bool IsFromStore()
    {
        int length = 0;
        // APPMODEL_ERROR_NO_PACKAGE (15700) means it was not installed as a package.
        return GetCurrentPackageFullName(ref length, null) != 15700;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, char[]? packageFullName);
}

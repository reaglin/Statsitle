<#
.SYNOPSIS
    Screenshots of Statistle for its help on SoftwarePlus.ai (softwareplus.ai/statistle/help/).

.DESCRIPTION
    Starts the hand-test build (manual-test\Statistle.exe), drives it with UI Automation only
    (Invoke, SelectionItem, ExpandCollapse, Value, SetFocus: never keystrokes or the mouse) and
    photographs its own windows with PrintWindow (flag 2, PW_RENDERFULLCONTENT): never a screen
    grab, so nothing else on the desktop is caught.

    It opens the sample project with File > Open the sample project (no file dialogs) and never
    saves: every save question is answered No or Cancel. A recovery copy already in
    %LOCALAPPDATA%\Statistle (unsaved work from an earlier session) is moved aside before
    Statistle starts and put back, byte for byte, at the end, so the "Restore it?" question on the
    next start is still the person's own. Documents\Statistle\demo-statistics.statistle is checked
    before and after: if this run had to create it, the run removes it again.

    Windows PowerShell 5.1 (powershell.exe), because UIAutomationClient is part of .NET Framework.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\help-shots.ps1
    powershell -ExecutionPolicy Bypass -File tools\help-shots.ps1 -Out C:\temp\shots
#>
param(
    [string]$Exe = (Join-Path $PSScriptRoot '..\manual-test\Statistle.exe'),
    [string]$Out = (Join-Path $PSScriptRoot '..\..\SoftwarePlus\site\statistle\help\images'),
    # Pictures that replace the program page's own (site\statistle\images): the data table and the Code tab.
    [string]$PageImages = (Join-Path $PSScriptRoot '..\..\SoftwarePlus\site\statistle\images'),
    # Widest picture kept; wider captures are scaled down (the help column is about 900 px wide).
    [int]$MaxWidth = 1200
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, WindowsBase, System.Drawing

Add-Type -ReferencedAssemblies UIAutomationClient, UIAutomationTypes, WindowsBase, System.Drawing -TypeDefinition @"
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Automation;

public class WinInfo {
    public IntPtr Handle; public string Title; public string Class; public int Left, Top, Width, Height;
    public override string ToString() { return string.Format("{0} [{1}] {2}x{3} at {4},{5}", Title, Class, Width, Height, Left, Top); }
}

public static class Shots {
    [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd, out RECT r);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc f, IntPtr l);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr h, StringBuilder s, int n);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, EnumProc f, IntPtr l);
    [DllImport("dwmapi.dll")] static extern int DwmGetWindowAttribute(IntPtr h, int attr, out RECT r, int size);
    delegate bool EnumProc(IntPtr h, IntPtr l);
    [StructLayout(LayoutKind.Sequential)] struct RECT { public int Left, Top, Right, Bottom; }

    /// Every visible top-level window of the process, biggest first.
    public static List<WinInfo> Windows(int pid) {
        var list = new List<WinInfo>();
        EnumWindows(delegate(IntPtr h, IntPtr l) {
            uint p; GetWindowThreadProcessId(h, out p);
            if (p != (uint)pid || !IsWindowVisible(h)) return true;
            RECT r; if (!GetWindowRect(h, out r)) return true;
            RECT f; if (DwmGetWindowAttribute(h, 9, out f, Marshal.SizeOf(typeof(RECT))) == 0) r = f;   // 9 = the frame people see, without the shadow
            var t = new StringBuilder(512); GetWindowText(h, t, 512);
            var c = new StringBuilder(256); GetClassName(h, c, 256);
            list.Add(new WinInfo { Handle = h, Title = t.ToString(), Class = c.ToString(),
                Left = r.Left, Top = r.Top, Width = r.Right - r.Left, Height = r.Bottom - r.Top });
            return true;
        }, IntPtr.Zero);
        list.Sort(delegate(WinInfo a, WinInfo b) { return (b.Width * b.Height).CompareTo(a.Width * a.Height); });
        return list;
    }

    /// The window as it looks: PrintWindow draws from the outer rectangle (with the invisible
    /// resize border), so the picture is cut to the frame people see.
    /// The child controls of a dialog (a Windows message box): its text and its buttons.
    public static List<WinInfo> Children(IntPtr parent) {
        var list = new List<WinInfo>();
        EnumChildWindows(parent, delegate(IntPtr h, IntPtr l) {
            var t = new StringBuilder(2048); GetWindowText(h, t, 2048);
            var c = new StringBuilder(256); GetClassName(h, c, 256);
            list.Add(new WinInfo { Handle = h, Title = t.ToString(), Class = c.ToString() });
            return true;
        }, IntPtr.Zero);
        return list;
    }

    public static Bitmap Print(WinInfo w) {
        RECT outer; GetWindowRect(w.Handle, out outer);
        using (var full = new Bitmap(outer.Right - outer.Left, outer.Bottom - outer.Top, PixelFormat.Format32bppArgb)) {
            using (var g = Graphics.FromImage(full)) {
                IntPtr hdc = g.GetHdc();
                PrintWindow(w.Handle, hdc, 2);
                g.ReleaseHdc(hdc);
            }
            var crop = Rectangle.Intersect(new Rectangle(w.Left - outer.Left, w.Top - outer.Top, w.Width, w.Height),
                                           new Rectangle(0, 0, full.Width, full.Height));
            if (crop.Width <= 0 || crop.Height <= 0) crop = new Rectangle(0, 0, full.Width, full.Height);
            var cut = new Bitmap(crop.Width, crop.Height, PixelFormat.Format32bppArgb);   // a real copy: Clone can share the pixels
            using (var g = Graphics.FromImage(cut)) {
                g.DrawImage(full, new Rectangle(0, 0, crop.Width, crop.Height), crop, GraphicsUnit.Pixel);
            }
            return cut;
        }
    }

    public static void Capture(WinInfo[] windows, string path) {
        using (Bitmap canvas = Print(windows[0])) { canvas.Save(path, ImageFormat.Png); }
    }

    /// A window with an open menu drawn on it: only the menu's own rectangle (screen
    /// coordinates, from UI Automation) is taken from the pop-up, whose window is larger.
    public static void CaptureWithMenu(WinInfo main, WinInfo popup, int x, int y, int w, int h, string path) {
        RECT pr;
        if (!GetWindowRect(popup.Handle, out pr) || pr.Right <= pr.Left || pr.Bottom <= pr.Top) {
            pr.Left = popup.Left; pr.Top = popup.Top; pr.Right = popup.Left + popup.Width; pr.Bottom = popup.Top + popup.Height;
        }
        using (Bitmap canvas = Print(main))
        using (var part = new Bitmap(pr.Right - pr.Left, pr.Bottom - pr.Top, PixelFormat.Format32bppArgb)) {
            using (var pg = Graphics.FromImage(part)) {
                IntPtr hdc = pg.GetHdc();
                PrintWindow(popup.Handle, hdc, 2);
                pg.ReleaseHdc(hdc);
            }
            var src = Rectangle.Intersect(new Rectangle(x - pr.Left, y - pr.Top, w, h), new Rectangle(0, 0, part.Width, part.Height));
            using (var g = Graphics.FromImage(canvas)) {
                g.DrawImage(part, new Rectangle(pr.Left + src.Left - main.Left, pr.Top + src.Top - main.Top, src.Width, src.Height), src, GraphicsUnit.Pixel);
            }
            canvas.Save(path, ImageFormat.Png);   // after the Graphics is closed, or GDI+ refuses
        }
    }

    /// Invoke on its own thread: a button that opens a modal window may not return until it closes.
    public static void Invoke(AutomationElement e) {
        var t = new Thread(delegate() {
            try { ((InvokePattern)e.GetCurrentPattern(InvokePattern.Pattern)).Invoke(); } catch (Exception) { }
        });
        t.IsBackground = true; t.Start(); t.Join(4000);
    }

    public static AutomationElement Find(AutomationElement root, string name, ControlType type) {
        Condition c = name == null
            ? (Condition)new PropertyCondition(AutomationElement.ControlTypeProperty, type)
            : new PropertyCondition(AutomationElement.NameProperty, name);
        if (type != null && name != null) c = new AndCondition(c, new PropertyCondition(AutomationElement.ControlTypeProperty, type));
        for (int i = 0; i < 30; i++) {
            AutomationElement e = root.FindFirst(TreeScope.Descendants, c);
            if (e != null) return e;
            Thread.Sleep(200);
        }
        throw new Exception("Not found: " + name);
    }
}
"@

function Log($text) { Write-Host "[help-shots] $text" }

# ---------------------------------------------------------------- the person's files, before
$autosaveDir = Join-Path $env:LOCALAPPDATA 'Statistle'
$aside = Join-Path ([IO.Path]::GetTempPath()) ("statistle-autosave-aside-" + [Guid]::NewGuid().ToString('N'))
$movedAside = @()
if (Test-Path $autosaveDir) {
    $files = @(Get-ChildItem -Force -File $autosaveDir)
    if ($files.Count -gt 0) {
        New-Item -ItemType Directory $aside | Out-Null
        foreach ($f in $files) {
            $hash = (Get-FileHash $f.FullName).Hash
            Move-Item -LiteralPath $f.FullName -Destination (Join-Path $aside $f.Name)
            $movedAside += [pscustomobject]@{ Name = $f.Name; Hash = $hash; Written = $f.LastWriteTimeUtc }
            Log "moved aside $($f.Name) ($hash)"
        }
    }
}

$sample = Join-Path ([Environment]::GetFolderPath('MyDocuments')) 'Statistle\demo-statistics.statistle'
$sampleHash = if (Test-Path $sample) { (Get-FileHash $sample).Hash } else { $null }
Log ("sample project before: " + $(if ($sampleHash) { $sampleHash } else { 'not there' }))

New-Item -ItemType Directory -Force $Out | Out-Null
$raw = Join-Path ([IO.Path]::GetTempPath()) ("statistle-shots-" + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory $raw | Out-Null

$proc = $null
$T = [System.Windows.Automation.ControlType]

function Main-Window { (([Shots]::Windows($proc.Id)) | Where-Object { $_.Title -like 'Statistle — *' -and $_.Class -like 'HwndWrapper*' } | Select-Object -First 1) }
function Root { [System.Windows.Automation.AutomationElement]::FromHandle((Main-Window).Handle) }
function Wait-Window($titleLike, [int]$ms = 6000) {
    $deadline = (Get-Date).AddMilliseconds($ms)
    while ((Get-Date) -lt $deadline) {
        $w = [Shots]::Windows($proc.Id) | Where-Object { $_.Title -like $titleLike } | Select-Object -First 1
        if ($w) { return $w }
        Start-Sleep -Milliseconds 200
    }
    throw "No window like '$titleLike'"
}
function Shot($name, $windows) {
    if (-not $windows) { $windows = @(Main-Window) }
    Start-Sleep -Milliseconds 900   # let the layout and plots settle
    [Shots]::Capture([WinInfo[]]$windows, (Join-Path $raw $name))
    Log "captured $name"
}
function Invoke-Named($name, $type, $root) {
    if (-not $root) { $root = Root }
    [Shots]::Invoke([Shots]::Find($root, $name, $type))
    Start-Sleep -Milliseconds 600
}
function Menu-Item($menu, $item) {
    $m = [Shots]::Find((Root), $menu, $T::MenuItem)
    $ec = $m.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    for ($i = 0; $i -lt 5 -and $ec.Current.ExpandCollapseState -ne 'Expanded'; $i++) {
        $ec.Expand()
        Start-Sleep -Milliseconds 600
    }
    if ($item) { [Shots]::Invoke([Shots]::Find($m, $item, $T::MenuItem)); Start-Sleep -Milliseconds 700 }
    return $m
}
function Tab($name) {
    $tab = [Shots]::Find((Root), $name, $T::TabItem)
    $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 800
}
function Rail($title) { Invoke-Named $title $T::Button; Start-Sleep -Milliseconds 600 }
function Tree-Select($name) {
    # The table nodes do not carry the table's name for UI Automation (they report the class
    # name), so the node is found by the text inside it, or failing that by its place in the list.
    $group = [Shots]::Find((Root), 'Data tables', $T::TreeItem)
    $nodes = $group.FindAll([System.Windows.Automation.TreeScope]::Children,
        (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, $T::TreeItem)))
    $item = $null
    foreach ($n in $nodes) {
        if ($n.Current.Name -eq $name -or
            $n.FindFirst([System.Windows.Automation.TreeScope]::Descendants,
                (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::NameProperty, $name)))) { $item = $n; break }
    }
    if (-not $item) {
        $order = @('Tensile strength', 'Solar panel', 'Housing', 'Battery life')   # the sample project's tables
        $item = $nodes[[Array]::IndexOf($order, $name)]
        Log ("tree node for '$name' found by position; UI Automation calls it '" + $item.Current.Name + "'")
    }
    $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 700
}
function Focus-Box($label) {
    $box = [Shots]::Find((Root), $label, $T::Edit)
    $box.SetFocus()
    Start-Sleep -Milliseconds 500
    return $box
}
function Answer-Box($titleLike, $button) {
    # A Windows message box: its static text and buttons are child windows; each button is
    # pressed through UI Automation (Invoke) on its own window handle.
    $w = Wait-Window $titleLike
    $kids = [Shots]::Children($w.Handle)
    Log ("'" + $w.Title + "' says: " + (($kids | Where-Object { $_.Class -eq 'Static' -and $_.Title } | ForEach-Object { $_.Title }) -join ' | ').Replace("`r", ' ').Replace("`n", ' '))
    $hit = $kids | Where-Object { $_.Class -eq 'Button' -and $_.Title.Replace('&', '') -eq $button } | Select-Object -First 1
    if (-not $hit) { throw ("No button '$button' in '" + $w.Title + "': " + (($kids | ForEach-Object { $_.Class + ':' + $_.Title }) -join ', ')) }
    [Shots]::Invoke([System.Windows.Automation.AutomationElement]::FromHandle($hit.Handle))
    Start-Sleep -Milliseconds 700
}

try {
    # ------------------------------------------------------------ 1. a fresh start: the empty state
    $proc = Start-Process -FilePath (Resolve-Path $Exe) -PassThru
    $deadline = (Get-Date).AddSeconds(30)
    while (-not (Main-Window) -and (Get-Date) -lt $deadline) { Start-Sleep -Milliseconds 300 }
    if (-not (Main-Window)) { throw 'Statistle did not open a window' }
    Start-Sleep -Seconds 2
    Log ("window: " + (Main-Window))
    Shot 'empty-state.png'

    # ------------------------------------------------------------ 2. the Help menu, then About
    $help = Menu-Item 'Help' $null
    $main = Main-Window
    $popup = $null
    for ($i = 0; $i -lt 30 -and -not $popup; $i++) {
        Start-Sleep -Milliseconds 300
        # The menu's rectangle, from its items, and the pop-up window that holds it.
        $items = $help.FindAll([System.Windows.Automation.TreeScope]::Children,
            (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, $T::MenuItem)))
        $ml = 1e9; $mt = 1e9; $mr = 0; $mb = 0
        foreach ($it in $items) {
            $bb = $it.Current.BoundingRectangle
            if ($bb.IsEmpty -or $bb.Width -le 0) { continue }
            $ml = [Math]::Min($ml, $bb.Left); $mt = [Math]::Min($mt, $bb.Top); $mr = [Math]::Max($mr, $bb.Right); $mb = [Math]::Max($mb, $bb.Bottom)
        }
        if ($mr -le 0) { continue }
        $popup = [Shots]::Windows($proc.Id) | Where-Object {
            $_.Handle -ne $main.Handle -and $_.Left -le $ml -and $_.Top -le $mt -and ($_.Left + $_.Width) -ge $mr -and ($_.Top + $_.Height) -ge $mb } | Select-Object -First 1
    }
    if (-not $popup) {
        Log ("menu state: " + $help.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Current.ExpandCollapseState + "; items: " + $items.Count + "; bounds: $ml,$mt,$mr,$mb")
        [Shots]::Windows($proc.Id) | ForEach-Object { Log ("window: " + $_) }
        throw 'The Help menu did not open'
    }
    $pad = 3   # the menu's border round its items
    Start-Sleep -Milliseconds 600
    Log ("menu at $ml,$mt,$mr,$mb in pop-up " + $popup + "; main " + $main)
    for ($try = 1; ; $try++) {
        # The pop-up can still be opening (its window not yet drawn); try again a moment later.
        try { [Shots]::CaptureWithMenu($main, $popup, [int]$ml - $pad, [int]$mt - $pad, [int]($mr - $ml) + 2 * $pad, [int]($mb - $mt) + 2 * $pad, (Join-Path $raw 'help-menu.png')); break }
        catch { if ($try -ge 8) { throw }; Log "menu not ready ($($_.Exception.InnerException.Message)), again"; Start-Sleep -Milliseconds 500 }
    }
    Log 'captured help-menu.png'
    [Shots]::Invoke([Shots]::Find($help, 'About Statistle…', $T::MenuItem))
    $about = Wait-Window 'Statistle — About'
    Shot 'about.png' @($about)
    Invoke-Named 'Close' $T::Button ([System.Windows.Automation.AutomationElement]::FromHandle($about.Handle))

    # ------------------------------------------------------------ 3. the sample project
    Invoke-Named 'Open the sample project' $T::Button
    Start-Sleep -Seconds 2
    Log ("window: " + (Main-Window).Title)
    Tree-Select 'Tensile strength'
    Shot 'data-table.png'

    # ------------------------------------------------------------ 4. Descriptive statistics, every tab
    Rail 'Descriptive statistics'
    Focus-Box 'Mean' | Out-Null
    Shot 'descriptive.png'
    Tab 'Plots';        Shot 'descriptive-plots.png'
    Tab 'Report';       Shot 'report.png'
    Tab 'Code';         Shot 'code.png'
    Tab 'Instructions'; Shot 'instructions.png'
    Tab 'Properties'

    # ------------------------------------------------------------ 5. a value that is refused
    Rail 'Confidence interval'
    $box = Focus-Box 'Confidence level'
    $box.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('95%')
    Focus-Box 'Name' | Out-Null      # leaving the box commits it, and the refusal shows
    $box.SetFocus(); Start-Sleep -Milliseconds 500
    Shot 'rejected.png'
    # A real change (never saved), so the save question on Exit and the recovery copy have something to offer.
    $box.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('0.9')
    Focus-Box 'Name' | Out-Null

    # ------------------------------------------------------------ 6. one per group
    Rail 'Continuous distribution'; Tab 'Plots'; Shot 'continuous-plots.png'; Tab 'Properties'
    Rail 'Two-sample test'; Shot 'two-sample.png'
    Rail 'Simple linear regression'; Shot 'regression.png'; Tab 'Plots'; Shot 'regression-plots.png'; Tab 'Properties'
    Rail 'One-way ANOVA'; Shot 'anova-one.png'
    # Tukey's pairs are one long line each, so they are read on the Report tab: its text is
    # scrolled to them through UI Automation's text pattern.
    Tab 'Report'
    $doc = [Shots]::Find((Root), $null, $T::Document)
    $found = $doc.GetCurrentPattern([System.Windows.Automation.TextPattern]::Pattern).DocumentRange.FindText('Tukey HSD pairs', $false, $false)
    if ($found) { $found.ScrollIntoView($false) } else { Log 'WARNING: Tukey HSD pairs not found in the report' }
    Shot 'anova-one-report.png'
    Tab 'Properties'
    Rail 'Two-way ANOVA'; Tab 'Plots'; Shot 'anova-two-plots.png'; Tab 'Properties'

    # ------------------------------------------------------------ 7. Delete asks first (answered No)
    Tree-Select 'Battery life'
    Invoke-Named 'Delete selected data table…' $T::Button
    $confirm = Wait-Window 'Statistle — Delete data table'
    Shot 'delete-question.png' @($confirm)
    Answer-Box 'Statistle — Delete data table' 'No'

    # ------------------------------------------------------------ 8. the save question on Exit (Cancel)
    Menu-Item 'File' 'Exit' | Out-Null
    $ask = Wait-Window 'Statistle'
    Shot 'save-question.png' @($ask)
    Answer-Box 'Statistle' 'Cancel'

    # ------------------------------------------------------------ 9. the restore question on the next start
    # Stopping the process leaves this run's own recovery copy, which the next start offers back.
    $proc.Kill(); $proc.WaitForExit()
    Start-Sleep -Seconds 1
    $proc = Start-Process -FilePath (Resolve-Path $Exe) -PassThru
    $restore = Wait-Window 'Statistle — Restore unsaved work?' 30000
    Shot 'restore-question.png' @($restore)
    Answer-Box 'Statistle — Restore unsaved work?' 'No'     # No deletes this run's copy
    Start-Sleep -Seconds 1
    Menu-Item 'File' 'Exit' | Out-Null
    $proc.WaitForExit(10000) | Out-Null
}
finally {
    if ($proc -and -not $proc.HasExited) { $proc.Kill(); $proc.WaitForExit() }

    # The person's recovery copy back, exactly as it was.
    if (Test-Path $autosaveDir) { Get-ChildItem -Force -File $autosaveDir | Remove-Item -Force }
    foreach ($f in $movedAside) {
        $dest = Join-Path $autosaveDir $f.Name
        Move-Item -LiteralPath (Join-Path $aside $f.Name) -Destination $dest
        $now = (Get-FileHash $dest).Hash
        if ($now -ne $f.Hash) { throw "Recovery copy $($f.Name) did not come back unchanged" }
        Log "restored $($f.Name) ($now, last written $((Get-Item $dest).LastWriteTimeUtc.ToString('u')))"
    }
    if (Test-Path $aside) { Remove-Item $aside -Recurse -Force }

    if ($sampleHash -and (Get-FileHash $sample).Hash -ne $sampleHash) { Log 'WARNING: the sample project changed' }
    if (-not $sampleHash -and (Test-Path $sample)) { Remove-Item $sample; Log 'removed the sample project this run created' }
}

# ---------------------------------------------------------------- the Import window, with a preview
# Reaching it in Statistle means choosing a file in a Windows file dialog, which this script never
# drives. Instead a small program shows Statistle's own ImportDialog (built from this repository)
# with a two-sheet workbook made in memory, and it is photographed the same way.
$work = Join-Path ([IO.Path]::GetTempPath()) ("statistle-import-shot-" + [Guid]::NewGuid().ToString('N'))
$harnessProc = $null
try {
    New-Item -ItemType Directory "$work\app", "$work\harness" | Out-Null
    $repo = Resolve-Path (Join-Path $PSScriptRoot '..')
    & dotnet build (Join-Path $repo 'src\Statistle.App\Statistle.App.csproj') -c Release -o "$work\app" -v quiet -nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'Statistle.App did not build' }
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
    <UseWPF>true</UseWPF>
    <Nullable>disable</Nullable>
    <AssemblyName>ImportShot</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Statistle.App"><HintPath>$work\app\Statistle.App.dll</HintPath></Reference>
    <Reference Include="Statistle.Core"><HintPath>$work\app\Statistle.Core.dll</HintPath></Reference>
  </ItemGroup>
</Project>
"@ | Set-Content -Encoding UTF8 "$work\harness\ImportShot.csproj"
    @'
using System;
using System.Windows;
using Statistle.App.Views;
using Statistle.Core.Persistence;

static class Program
{
    [STAThread]
    static void Main()
    {
        var app = new Application();
        var week1 = TableImport.ReadText(
            "Specimen,AA2024,AA7075,AA-X,Notes\n" +
            "S1,470,505,495,first batch\nS2,485,518,508,\nS3,478,523,512,\nS4,492,508,492,re-tested\nS5,460,515,516,\n" +
            "S6,473,522,505,\nS7,488,510,498,\nS8,484,529,510,\nS9,482,520,504,\nS10,475,514,489,", "Week 1");
        var week2 = TableImport.ReadText(
            "Specimen,AA2024,AA7075,AA-X\nS11,481,512,501\nS12,476,519,497\nS13,490,507,506", "Week 2");
        var dialog = new ImportDialog(new[] { week1, week2 }, ImportDialog.ImportKind.Table, "Data", "tensile-tests.xlsx", 3)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
        };
        app.Run(dialog);
    }
}
'@ | Set-Content -Encoding UTF8 "$work\harness\Program.cs"
    & dotnet build "$work\harness\ImportShot.csproj" -c Release -o "$work\out" -v quiet -nologo | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'The import-window program did not build' }

    $harnessProc = Start-Process -FilePath "$work\out\ImportShot.exe" -PassThru
    $proc = $harnessProc   # Wait-Window and Shot look at $proc
    $dialog = Wait-Window 'Statistle — Import data' 30000
    Shot 'import-preview.png' @($dialog)
}
catch { Log "WARNING: the Import window was not photographed: $($_.Exception.Message)" }
finally {
    if ($harnessProc -and -not $harnessProc.HasExited) { $harnessProc.Kill(); $harnessProc.WaitForExit() }
    if (Test-Path $work) { Remove-Item $work -Recurse -Force -ErrorAction SilentlyContinue }
}

# ---------------------------------------------------------------- small PNGs for the web
foreach ($png in Get-ChildItem $raw -Filter *.png) {
    $dest = Join-Path $Out $png.Name
    & magick $png.FullName -resize "${MaxWidth}x>" -strip -define png:compression-level=9 $dest
    if ((Get-Item $dest).Length -gt 250KB) {
        & magick $png.FullName -resize "${MaxWidth}x>" -strip -colors 256 -define png:compression-level=9 "PNG8:$dest"
    }
    Log ("{0}: {1:N0} KB" -f $png.Name, ((Get-Item $dest).Length / 1KB))
}
foreach ($name in 'data-table.png', 'code.png') {
    if (Test-Path (Join-Path $Out $name)) {
        $target = if ($name -eq 'data-table.png') { 'main.png' } else { 'code.png' }
        Move-Item (Join-Path $Out $name) (Join-Path $PageImages $target) -Force   # the help pages use ../images/ for these two
    }
}
Remove-Item $raw -Recurse -Force
Log 'done'

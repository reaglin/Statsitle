using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Statistle.App.Plotting;
using Statistle.App.Reporting;
using Statistle.App.ViewModels;
using Statistle.App.Views;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Reporting;
using Statistle.Core.Analyses;

namespace Statistle.App;

public partial class MainWindow : Window
{
    private const int ReportTab = 1;
    private const int PlotsTab = 2;
    private const int InstructionsTab = 4;

    private readonly WorkspaceViewModel _workspace;
    private readonly ScottPlotFigureRenderer _renderer = new();
    private bool _reportStale = true;
    private bool _plotsStale = true;
    private bool _instructionsStale = true;

    public MainWindow()
        : this(new WorkspaceViewModel())
    {
    }

    /// <summary>
    /// The window over a given workspace — the tests pass one with their own dialogs and autosave
    /// folder, so drawing every row never touches the real recovery copy.
    /// </summary>
    public MainWindow(WorkspaceViewModel workspace)
    {
        _workspace = workspace;
        InitializeComponent();

        // Wide enough for the two-column forms at its default size, but never larger than the screen.
        Rect work = SystemParameters.WorkArea;
        Width = Math.Min(Width, work.Width);
        Height = Math.Min(Height, work.Height);

        DataContext = _workspace;
        _workspace.FigureRenderer = _renderer;
        _workspace.CommitPendingEdit = CommitFocusedBox;
        _workspace.PropertyChanged += Workspace_PropertyChanged;
        _workspace.Rows.CollectionChanged += (_, _) => ScheduleSeriesHeight();
        ContentTabs.SelectionChanged += ContentTabs_SelectionChanged;
        InitializeCourse();

        Loaded += (_, _) =>
        {
            // Unsaved work from a session that ended without saving (a crash, a power cut) is
            // offered back first; a file the app was opened with waits until that is settled.
            bool restored = _workspace.OfferRecovery();
            string? path = (Application.Current as App)?.ActivationFilePath;

            if (path is not null && !restored)
            {
                _workspace.OpenPath(path);
            }
            else if (path is not null)
            {
                _workspace.Announce(
                    $"Restored your unsaved work. {System.IO.Path.GetFileName(path)} was not opened; open it with File ▸ Open… when you are ready.");
            }

            RefreshOutputTabs();
        };
    }

    /// <summary>
    /// The report and the plots are rebuilt only when their tab is showing; otherwise they are
    /// marked stale and rebuilt when it is. Rendering figures on every keystroke would not
    /// be noticed on the Properties tab, but it would be paid for.
    /// </summary>
    private void Workspace_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(WorkspaceViewModel.Figures))
        {
            _reportStale = true;
            _plotsStale = true;
            RefreshOutputTabs();
        }

        if (e.PropertyName == nameof(WorkspaceViewModel.CurrentInstructions))
        {
            _instructionsStale = true;
            RefreshOutputTabs();
        }
    }

    // ── Series boxes fill the space the form has left ─────────────────────────

    private const double MinimumSeriesBoxHeight = 120;

    private bool _seriesHeightScheduled;

    private void Editor_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        FitSplitWidth();
        ScheduleSeriesHeight();
    }

    /// <summary>
    /// The two-column form is as wide as the space it has, so its results column is what is left
    /// beside the entry values and a result in words wraps there instead of pushing the form past
    /// the window (UX review 2026-10-31). Below the grid's MinWidth the scrollbar takes over.
    /// </summary>
    private void FitSplitWidth()
    {
        double room = SplitScroll.ActualWidth - SystemParameters.VerticalScrollBarWidth - 2;

        if (room > 0)
        {
            SplitGrid.Width = Math.Max(SplitGrid.MinWidth, room);
        }
    }

    /// <summary>
    /// Measures after the next layout pass rather than during one, so setting the height
    /// cannot re-enter layout; a second request while one is pending is folded into it.
    /// </summary>
    private void ScheduleSeriesHeight()
    {
        if (_seriesHeightScheduled)
        {
            return;
        }

        _seriesHeightScheduled = true;
        Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Loaded,
            () =>
            {
                _seriesHeightScheduled = false;
                MeasureSeriesHeight();
            });
    }

    /// <summary>
    /// The space below the form's other rows, shared among its series boxes: the editor's
    /// viewport height less every row that is not a series, less the series rows' own
    /// chrome (label, hint, margins), floored so a small window still gets a usable box.
    /// </summary>
    private void MeasureSeriesHeight()
    {
        bool split = _workspace.ResultsBeside;
        ScrollViewer viewport = split ? SplitScroll : EditorScroll;
        ItemsControl rows = split ? EntryRowsControl : PropertyRows;
        IReadOnlyList<PropertyRow> items = split ? _workspace.EntryRows : _workspace.Rows;

        if (viewport.ViewportHeight <= 0 || items.Count == 0)
        {
            return;
        }

        double others = 0;
        int seriesRows = 0;
        double currentBox = _workspace.SeriesBoxHeight;

        for (int i = 0; i < items.Count; i++)
        {
            if (rows.ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement container)
            {
                continue;
            }

            if (items[i].IsSeries || items[i].IsTable)
            {
                // Everything in the row except the box itself (label, hint, margins, buttons).
                seriesRows++;
                others += Math.Max(container.ActualHeight - currentBox, 0);
            }
            else
            {
                others += container.ActualHeight;
            }
        }

        if (seriesRows == 0)
        {
            return;
        }

        // A little slack keeps the vertical scrollbar from appearing over a rounding error.
        double available = viewport.ViewportHeight - others - 12 * seriesRows - 4;
        _workspace.SeriesBoxHeight = Math.Max(MinimumSeriesBoxHeight, Math.Floor(available / seriesRows));
    }

    private void ContentTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, ContentTabs))
        {
            RefreshOutputTabs();
        }
    }

    private void RefreshOutputTabs()
    {
        if (ContentTabs.SelectedIndex == ReportTab && _reportStale)
        {
            RebuildReport();
        }

        if (ContentTabs.SelectedIndex == PlotsTab && _plotsStale)
        {
            RebuildPlots();
        }

        if (ContentTabs.SelectedIndex == InstructionsTab && _instructionsStale)
        {
            _instructionsStale = false;
            InstructionsView.Document = InstructionsDocumentBuilder.Build(_workspace.CurrentInstructions);
        }
    }

    private void RebuildReport()
    {
        _reportStale = false;

        Report? report = _workspace.CurrentReport;

        if (report is null)
        {
            var empty = new System.Windows.Documents.FlowDocument(
                new System.Windows.Documents.Paragraph(
                    new System.Windows.Documents.Run("Select a data table or an analysis to see its report.")))
            {
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 13,
                PagePadding = new Thickness(28, 20, 28, 24),
            };

            ReportView.Document = empty;
            return;
        }

        ReportView.Document = ReportDocumentBuilder.Build(report, _renderer);
    }

    private void RebuildPlots()
    {
        _plotsStale = false;
        PlotsPanel.Children.Clear();

        IReadOnlyList<PlotSpec> figures = _workspace.Figures;

        if (figures.Count == 0)
        {
            PlotsPanel.Children.Add(new TextBlock
            {
                Text = _workspace.Selected is null
                    ? "Select an item to see its plots."
                    : $"{_workspace.SelectedHeading} has nothing to plot yet.",
                Foreground = (Brush)FindResource("MutedTextBrush"),
                Margin = new Thickness(0, 8, 0, 0),
            });

            return;
        }

        foreach (PlotSpec figure in figures)
        {
            var panel = new FigurePanel();
            panel.Show(figure);
            PlotsPanel.Children.Add(panel);
        }
    }

    private void ProjectTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) =>
        _workspace.Selected = e.NewValue as StatistleObject;

    /// <summary>
    /// Opens whichever discovered analysis was clicked. The handler names no analysis — the
    /// descriptor comes from the rail item, which came from reflection.
    /// </summary>
    private void Tool_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: AnalysisDescriptor descriptor })
        {
            _workspace.SelectTool(descriptor);
            ContentTabs.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// The Import link beside a series: read a file (text, CSV, DISTRIB .xd2, Excel .xlsx),
    /// let the user pick the column or columns in a preview, and replace the series.
    /// </summary>
    private void ImportSeries_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { DataContext: PropertyRow { CanImport: true } row })
        {
            return;
        }

        var open = new Microsoft.Win32.OpenFileDialog
        {
            Filter = TableImport.FileFilter,
            Title = $"Import {row.Label}",
        };

        if (open.ShowDialog(this) != true)
        {
            return;
        }

        IReadOnlyList<ImportedTable> tables;
        try
        {
            tables = TableImport.Read(open.FileName);
        }
        catch (Exception ex) when (ex is System.IO.IOException or System.IO.InvalidDataException
                                       or UnauthorizedAccessException or System.Xml.XmlException)
        {
            MessageBox.Show(this,
                $"Statistics+ could not read {System.IO.Path.GetFileName(open.FileName)}: {ex.Message}\n\n" +
                "Check that it is a text, CSV, .xd2 or Excel .xlsx file and that it is not open in another program, then try again. " +
                "Nothing was changed.",
                "Statistics+ — Could not read the file", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string fileName = System.IO.Path.GetFileName(open.FileName);

        if (tables.Count == 0)
        {
            MessageBox.Show(this, $"{fileName} holds no data to import. Nothing was changed.",
                "Statistics+ — Nothing to import", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (row.IsTable)
        {
            // A preview first, with the sheet to choose: the import replaces every column (UX review 2026-10-31).
            var preview = new ImportDialog(tables, ImportDialog.ImportKind.Table, row.Label, fileName, row.Table?.ColumnCount ?? 0)
            {
                Owner = this,
            };

            if (preview.ShowDialog() != true || preview.Table is not ImportedTable table)
            {
                return;
            }

            int[] numeric = ImportDialog.NumericColumns(table);

            _workspace.PushUndo();
            row.ImportColumns(numeric.Select(i => new DataColumn(table.Headers[i], table.Column(i))));

            string sheet = tables.Count > 1 ? $", sheet '{table.Name}'" : string.Empty;
            _workspace.Announce(
                $"Imported {WorkspaceViewModel.Count(numeric.Length, "column", "columns")} from {fileName}{sheet}. Edit ▸ Undo (Ctrl+Z) puts the old data back.");
            return;
        }

        var dialog = new ImportDialog(tables, pairs: !row.IsSingleColumnSeries, row.Label, fileName)
        {
            Owner = this,
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        _workspace.PushUndo();

        if (dialog.Pairs is DataPoint[] pairs)
        {
            row.ImportPairs(pairs);
            _workspace.Announce($"Imported {WorkspaceViewModel.Count(pairs.Length, "pair", "pairs")} from {fileName}. Edit ▸ Undo (Ctrl+Z) puts the old data back.");
        }
        else if (dialog.Values is double[] values)
        {
            row.ImportValues(values);
            _workspace.Announce($"Imported {WorkspaceViewModel.Count(values.Length, "value", "values")} from {fileName}. Edit ▸ Undo (Ctrl+Z) puts the old data back.");
        }
    }

    /// <summary>The Edit link beside a reference: goes to the object it refers to.</summary>
    private void EditLink_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PropertyRow { Link: StatistleObject linked } })
        {
            _workspace.Selected = linked;
            ContentTabs.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// The "Calculate…" link beside a value: opens the analysis that declared it can fill that
    /// property, remembering where the result is to go.
    /// </summary>
    private void Calculate_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: PropertyRow { Provider: not null } row })
        {
            _workspace.BeginHandoff(row.Target, row.Property, row.Provider);
            ContentTabs.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Entering a value box selects what is in it, so the number is replaced by typing rather
    /// than backspaced over. The mouse handler gives focus first and swallows the click, which
    /// would otherwise land a caret and undo the selection.
    /// </summary>
    private void ValueBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
    {
        if (sender is TextBox box)
        {
            box.SelectAll();
        }
    }

    /// <summary>
    /// Enter uses what is typed (the box keeps the focus, so the result is seen at once); Esc
    /// throws it away and shows the value in use again.
    /// </summary>
    private void ValueBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is not TextBox box)
        {
            return;
        }

        if (e.Key == System.Windows.Input.Key.Enter)
        {
            box.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            box.SelectAll();
            e.Handled = true;
        }
        else if (e.Key == System.Windows.Input.Key.Escape)
        {
            CancelBox(box);
            box.SelectAll();
            e.Handled = true;
        }
    }

    /// <summary>Esc in a series or table box: the data in use comes back. Enter stays a new line.</summary>
    private void SeriesBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (sender is TextBox box && e.Key == System.Windows.Input.Key.Escape)
        {
            CancelBox(box);
            e.Handled = true;
        }
    }

    private static void CancelBox(TextBox box)
    {
        (box.DataContext as PropertyRow)?.CancelEntry();
        box.GetBindingExpression(TextBox.TextProperty)?.UpdateTarget();
    }

    /// <summary>
    /// Pushes what is typed in the focused value box into its row, so Save, Copy, the exports and
    /// closing use the value on screen. Only a box with a change not yet used is pushed; an
    /// unchanged series box would otherwise be re-read and mark the project changed.
    /// </summary>
    private void CommitFocusedBox()
    {
        // A menu is its own focus scope: while File ▸ Save is clicked the keyboard focus is on the
        // menu item, but the window's logical focus is still the box being typed in.
        object? focused = System.Windows.Input.FocusManager.GetFocusedElement(this) ?? System.Windows.Input.Keyboard.FocusedElement;

        if (focused is TextBox { DataContext: PropertyRow } box
            && box.GetBindingExpression(TextBox.TextProperty) is { IsDirty: true } binding)
        {
            binding.UpdateSource();
        }
    }

    private void ValueBox_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is TextBox { IsKeyboardFocusWithin: false } box)
        {
            box.Focus();
            e.Handled = true;
        }
    }

    // ── Help panel: which field is current ────────────────────────────────────

    /// <summary>
    /// Keyboard focus landing anywhere inside a row (a value box, a drop-down, a unit button)
    /// makes that row the one the help panel describes. Tabbing through a form walks the help.
    /// </summary>
    private void Editor_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) =>
        _workspace.FocusedRow = RowOf(e.NewFocus as DependencyObject) ?? _workspace.FocusedRow;

    /// <summary>
    /// A click on a row — its label included, which takes no focus — selects it for help; a
    /// click on empty form space clears the selection so the item's overall help shows. A click
    /// on a selectable result also makes it the chosen one.
    /// </summary>
    private void Editor_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        PropertyRow? row = RowOf(e.OriginalSource as DependencyObject);
        _workspace.FocusedRow = row;

        if (row is { IsSelectable: true })
        {
            row.Choose();
        }
    }

    /// <summary>Walks up from an element to the row it belongs to; null if it is not in one.</summary>
    private static PropertyRow? RowOf(DependencyObject? element)
    {
        for (DependencyObject? current = element; current is not null; current = ParentOf(current))
        {
            if (current is FrameworkElement { DataContext: PropertyRow row })
            {
                return row;
            }

            if (current is ScrollViewer || (current is ItemsControl and not DataGrid))
            {
                return null;
            }
        }

        return null;
    }

    private static DependencyObject? ParentOf(DependencyObject element) =>
        element is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(element) ?? LogicalTreeHelper.GetParent(element)
            : LogicalTreeHelper.GetParent(element);

    private void Exit_Click(object sender, RoutedEventArgs e) => Close();

    /// <summary>Closing asks to save unsaved changes first (Yes / No / Cancel); Cancel keeps Statistle open.</summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        if (!_workspace.ConfirmDiscard("closing"))
        {
            e.Cancel = true;
            return;
        }

        if (!_workspace.IsDirty)
        {
            // Saved, or nothing changed: there is nothing for the next start to offer back.
            _workspace.DeleteAutosave();
        }

        base.OnClosing(e);
    }

    private void About_Click(object sender, RoutedEventArgs e) =>
        new Views.AboutWindow { Owner = this }.ShowDialog();

    /// <summary>F1 / Help ▸ Help for this screen: the Instructions tab for whatever is selected.</summary>
    private void HelpForScreen_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
    {
        if (CourseMode.IsChecked == true && _coursePage is not null)
        {
            _coursePage.ShowHelp();
            return;
        }

        ContentTabs.SelectedIndex = InstructionsTab;
        _workspace.Announce($"Instructions for {_workspace.CurrentInstructions.Title}. More on the web: {WebHelp.Display(_workspace.WebHelpPage)}");
    }

    private void WebHelp_Click(object sender, RoutedEventArgs e) => WebHelp.Open(this, "index.html");

    private void WebHelpForItem_Click(object sender, RoutedEventArgs e) => WebHelp.Open(this, _workspace.WebHelpPage);

    private async void CheckForUpdate_Click(object sender, RoutedEventArgs e) =>
        await StoreUpdates.CheckAsync(this, () => _workspace.ConfirmDiscard("installing the new version"), _workspace.Announce);
}

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

    private readonly WorkspaceViewModel _workspace = new();
    private readonly ScottPlotFigureRenderer _renderer = new();
    private bool _reportStale = true;
    private bool _plotsStale = true;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = _workspace;
        _workspace.FigureRenderer = _renderer;
        _workspace.PropertyChanged += Workspace_PropertyChanged;
        _workspace.Rows.CollectionChanged += (_, _) => ScheduleSeriesHeight();
        ContentTabs.SelectionChanged += ContentTabs_SelectionChanged;

        Loaded += (_, _) =>
        {
            if ((Application.Current as App)?.ActivationFilePath is string path)
            {
                _workspace.OpenPath(path);
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
    }

    // ── Series boxes fill the space the form has left ─────────────────────────

    private const double MinimumSeriesBoxHeight = 120;

    private bool _seriesHeightScheduled;

    private void Editor_SizeChanged(object sender, SizeChangedEventArgs e) => ScheduleSeriesHeight();

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
            MessageBox.Show(this, ex.Message, "Could not read the file");
            return;
        }

        if (row.IsTable)
        {
            ImportedTable table = tables[0];

            _workspace.PushUndo();
            row.ImportColumns(
                Enumerable.Range(0, table.ColumnCount)
                    .Select(i => new DataColumn(table.Headers[i], table.Column(i))));
            return;
        }

        var dialog = new ImportDialog(tables, pairs: !row.IsSingleColumnSeries, row.Label, System.IO.Path.GetFileName(open.FileName))
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
        }
        else if (dialog.Values is double[] values)
        {
            row.ImportValues(values);
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
}

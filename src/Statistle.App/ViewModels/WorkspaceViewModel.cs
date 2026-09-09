using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Statistle.Core.Document;
using Statistle.Core.Domain;
using Statistle.Core.Methods;
using Statistle.Core.Persistence;
using Statistle.Core.Plotting;
using Statistle.Core.Reporting;
using Statistle.Core.Analyses;
using Statistle.Core.Code;

namespace Statistle.App.ViewModels;

/// <summary>
/// The workspace: the project tree and the analysis rail on the left, the selected item's
/// Properties, Report and Plots on the right. It names no data type and no analysis — everything
/// it shows is read off the metadata the selected object carries.
/// </summary>
public sealed class WorkspaceViewModel : INotifyPropertyChanged
{
    private const string Filter = "Statistle project (*.statistle)|*.statistle|All files (*.*)|*.*";

    private const int UndoDepth = 50;

    private readonly List<string> _undo = [];

    private StatistleDocument _document = new();
    private StatistleObject? _selected;
    private string _status = "New project";
    private bool _isDirty;
    private bool _restoring;
    private Handoff? _handoff;
    private PropertyRow? _focusedRow;
    private double _seriesBoxHeight = 170;

    public WorkspaceViewModel()
    {
        NewCommand = new RelayCommand(New);
        OpenCommand = new RelayCommand(Open);
        SaveCommand = new RelayCommand(() => Save(saveAs: false));
        SaveAsCommand = new RelayCommand(() => Save(saveAs: true));
        AddTableCommand = new RelayCommand(
            () =>
            {
                PushUndo();
                Select(Document.AddTable());
            },
            MarkDirtyAfterAdd);
        DeleteCommand = new RelayCommand(Delete, () => Selected is not null);
        ExportCommand = new RelayCommand(Export, () => Selected is not null);
        UndoCommand = new RelayCommand(Undo, () => _undo.Count > 0);
        ReportCommand = new RelayCommand(ShowReport, () => Selected is not null);
        ExportPdfCommand = new RelayCommand(ExportPdf, () => Selected is not null);
        ExportCsvCommand = new RelayCommand(ExportCsv, () => Selected is not null);
        UseResultCommand = new RelayCommand(UseResult, () => _handoff is not null);
        CancelHandoffCommand = new RelayCommand(CancelHandoff, () => _handoff is not null);
        CopyCodeCommand = new RelayCommand(CopyCode, () => Selected is ICodeSource);
        SaveCodeCommand = new RelayCommand(SaveCode, () => Selected is ICodeSource);
        OpenInstructionsCommand = new RelayCommand(OpenInstructions);
        SaveInstructionsCommand = new RelayCommand(SaveInstructions);

        NewProject();
    }

    /// <summary>
    /// An analysis opened from another object's field, and where its result is to go.
    /// </summary>
    private sealed record Handoff(StatistleObject Target, StatistleProperty Property, ValueProvider Provider);

    public event PropertyChangedEventHandler? PropertyChanged;

    public RelayCommand NewCommand { get; }

    public RelayCommand OpenCommand { get; }

    public RelayCommand SaveCommand { get; }

    public RelayCommand SaveAsCommand { get; }

    public RelayCommand AddTableCommand { get; }

    public RelayCommand DeleteCommand { get; }

    public RelayCommand ExportCommand { get; }

    public RelayCommand UndoCommand { get; }

    public RelayCommand ReportCommand { get; }

    public RelayCommand OpenInstructionsCommand { get; }

    public RelayCommand SaveInstructionsCommand { get; }

    public RelayCommand ExportPdfCommand { get; }

    public RelayCommand ExportCsvCommand { get; }

    public RelayCommand UseResultCommand { get; }

    public RelayCommand CancelHandoffCommand { get; }

    public RelayCommand CopyCodeCommand { get; }

    public RelayCommand SaveCodeCommand { get; }

    // ── The Code tab ──────────────────────────────────────────────────────────

    private CodeLanguage _codeLanguage = CodeLanguage.Python;

    public bool IsPython
    {
        get => _codeLanguage == CodeLanguage.Python;
        set => SetCodeLanguage(value ? CodeLanguage.Python : CodeLanguage.R);
    }

    public bool IsR
    {
        get => _codeLanguage == CodeLanguage.R;
        set => SetCodeLanguage(value ? CodeLanguage.R : CodeLanguage.Python);
    }

    /// <summary>
    /// The script that reproduces the selected item in the chosen language: a data table as
    /// variables, an analysis as the data plus the same results printed in the same order.
    /// The item writes it; the workspace only asks.
    /// </summary>
    public string CodeText =>
        Selected is ICodeSource source ? source.Code(_codeLanguage)
        : Selected is null ? "Select a data table or an analysis to see its code."
        : $"{SelectedHeading} has no code to show.";

    private void SetCodeLanguage(CodeLanguage language)
    {
        if (_codeLanguage == language)
        {
            return;
        }

        _codeLanguage = language;
        Raise(nameof(IsPython));
        Raise(nameof(IsR));
        Raise(nameof(CodeText));
    }

    private void CopyCode()
    {
        try
        {
            System.Windows.Clipboard.SetText(CodeText);
            Status = $"{(_codeLanguage == CodeLanguage.Python ? "Python" : "R")} code copied to the clipboard";
        }
        catch (System.Runtime.InteropServices.ExternalException ex)
        {
            Status = $"Could not copy: {ex.Message}";
        }
    }

    private void SaveCode()
    {
        if (Selected is null)
        {
            return;
        }

        bool python = _codeLanguage == CodeLanguage.Python;
        var dialog = new SaveFileDialog
        {
            Filter = python ? "Python script (*.py)|*.py" : "R script (*.R)|*.R",
            DefaultExt = python ? ".py" : ".R",
            FileName = CodeText_FileName(python),
            InitialDirectory = DefaultFolder(),
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            File.WriteAllText(dialog.FileName, CodeText);
            Status = $"Saved {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not save the code: {ex.Message}";
        }
    }

    /// <summary>
    /// The instruction page for the selected item's kind — what it is, when to use it, the steps,
    /// every entry and result explained, the methods and sources — or the overview of Statistle
    /// when nothing is selected. Built from the metadata and the item's guide; nothing here names
    /// an analysis.
    /// </summary>
    public Statistle.Core.Instructions.InstructionSet CurrentInstructions =>
        Selected is null ? Statistle.Core.Instructions.InstructionsBuilder.Overview()
        : Statistle.Core.Instructions.InstructionsBuilder.For(Selected.GetType());

    private void OpenInstructions()
    {
        try
        {
            string file = Path.Combine(Path.GetTempPath(), $"statistle-instructions-{Guid.NewGuid():N}.html");
            File.WriteAllText(file, Statistle.Core.Instructions.InstructionsHtmlWriter.Write(CurrentInstructions));
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });
            Status = $"Instructions opened for {CurrentInstructions.Title}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception)
        {
            Status = $"Could not open the instructions: {ex.Message}";
        }
    }

    private void SaveInstructions()
    {
        Statistle.Core.Instructions.InstructionSet page = CurrentInstructions;
        var dialog = new SaveFileDialog
        {
            Filter = "Web page (*.html)|*.html|Plain text (*.txt)|*.txt",
            DefaultExt = ".html",
            FileName = Statistle.Core.Code.CodeText.Identifier(page.Title, "statistle").Replace('_', '-') + "-instructions.html",
            InitialDirectory = DefaultFolder(),
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            bool text = dialog.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
            File.WriteAllText(dialog.FileName, text
                ? Statistle.Core.Instructions.InstructionsTextWriter.Write(page)
                : Statistle.Core.Instructions.InstructionsHtmlWriter.Write(page));
            Status = $"Saved {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not save the instructions: {ex.Message}";
        }
    }

    private string CodeText_FileName(bool python) =>
        CodeText_Base() + (python ? ".py" : ".R");

    private string CodeText_Base() =>
        Selected is null ? "statistle" : Statistle.Core.Code.CodeText.Identifier(Selected.Name, "statistle").Replace('_', '-');

    public StatistleDocument Document
    {
        get => _document;
        private set
        {
            _document = value;
            Raise(nameof(Document));
            Raise(nameof(Tables));
            Raise(nameof(Title));
        }
    }

    public ObservableCollection<DataTable> Tables => Document.Tables;

    public ObservableCollection<PropertyRow> Rows { get; } = [];

    /// <summary>
    /// <see cref="Rows"/> split for an analysis that asks for two columns: entry values, then
    /// results. Both stay in sync with <see cref="Rows"/>; the window shows one layout or the
    /// other according to <see cref="ResultsBeside"/>.
    /// </summary>
    public ObservableCollection<PropertyRow> EntryRows { get; } = [];

    public ObservableCollection<PropertyRow> ResultRows { get; } = [];

    /// <summary>
    /// Height of a multi-line series box: whatever the form has left below its other rows, so
    /// a data set fills the space above the help panel. Measured by the window after layout;
    /// never less than the floor the window applies.
    /// </summary>
    public double SeriesBoxHeight
    {
        get => _seriesBoxHeight;
        set
        {
            if (Math.Abs(_seriesBoxHeight - value) < 0.5)
            {
                return;
            }

            _seriesBoxHeight = value;
            Raise(nameof(SeriesBoxHeight));
        }
    }

    /// <summary>Declared by the analysis (<see cref="Analysis.ResultsBeside"/>); the window only asks.</summary>
    public bool ResultsBeside => Selected is Analysis { ResultsBeside: true };

    public bool ResultsBelow => !ResultsBeside;

    /// <summary>Heading of the results column; says the results are clickable only when they are.</summary>
    public string ResultsHeading =>
        Selected is IChoosesMethod ? "RESULTS — CLICK THE ONE TO USE" : "RESULTS";

    /// <summary>
    /// The row whose field has the focus, or null when none does — then the help panel shows
    /// the overall help for the selected item. Set by the window from focus and click events.
    /// </summary>
    public PropertyRow? FocusedRow
    {
        get => _focusedRow;
        set
        {
            if (ReferenceEquals(_focusedRow, value))
            {
                return;
            }

            _focusedRow = value;
            Raise(nameof(FocusedRow));
            RaiseHelpChanged();
        }
    }

    /// <summary>Heading of the help panel: the field's label, or the item's.</summary>
    public string HelpTitle =>
        FocusedRow is not null ? FocusedRow.HelpTitle
        : Selected is null ? "Statistle"
        : SelectedHeading;

    /// <summary>
    /// What the help panel says: the focused field's help, or — with nothing focused — the
    /// item's overall help from its <see cref="StatistleHelpAttribute"/>, followed by the method(s)
    /// currently in force on it and their sources. Nothing here names a type or a method.
    /// </summary>
    public string HelpText
    {
        get
        {
            if (FocusedRow is not null)
            {
                return FocusedRow.HelpText;
            }

            if (Selected is null)
            {
                return "Select a data table in the project tree, add one with + Data table, or open " +
                       "an analysis. This panel describes whichever field you click into; with no " +
                       "field selected it describes the item as a whole.";
            }

            string overall = StatistleObject.HelpOf(Selected.GetType());

            if (overall.Length == 0 && Selected is Analysis analysis)
            {
                overall = analysis.Summary + ".";
            }

            IEnumerable<ICalculationMethod> inForce = Selected is IChoosesMethod chooser
                ? (chooser.ChosenMethod() is ICalculationMethod chosen ? [chosen] : [])
                : MethodScope.ActiveMethods(Selected);

            string methods = string.Join(
                "\n\n",
                inForce.Select(m => $"{m.Name}: {m.Description}\nSource: {m.Citation}"));

            return methods.Length == 0 ? overall : $"{overall}\n\n{methods}";
        }
    }

    /// <summary>Why the selected item could not compute, or empty. Comes from the item itself.</summary>
    public string ProblemNote => (Selected as IReportsProblem)?.Problem ?? string.Empty;

    private void RaiseHelpChanged()
    {
        Raise(nameof(HelpTitle));
        Raise(nameof(HelpText));
        Raise(nameof(ProblemNote));
    }

    /// <summary>
    /// The rail, filled by reflection over every <see cref="Analysis"/> in the build. There is
    /// no list of analyses anywhere in the UI — adding an analysis class is the only edit.
    /// </summary>
    public IReadOnlyList<AnalysisDescriptor> Analyses { get; } =
        AnalysisCatalog.OfKind(ToolKind.FreeStanding, typeof(Analysis).Assembly);

    public IReadOnlyList<AnalysisDescriptor> ProjectTools { get; } =
        AnalysisCatalog.OfKind(ToolKind.ProjectBound, typeof(Analysis).Assembly);

    /// <summary>Opens an analysis from the rail, restoring the parameters this project saved for it.</summary>
    public void SelectTool(AnalysisDescriptor descriptor)
    {
        Analysis tool = Document.Tool(descriptor);
        tool.Recalculate();

        Selected = tool;
        Status = $"{descriptor.Title} — {descriptor.Summary}";
    }

    public StatistleObject? Selected
    {
        get => _selected;
        set
        {
            // Walking away from the analysis abandons the hand-off; the result stays where
            // it is and nothing is written anywhere.
            if (_handoff is not null && value?.GetType() != _handoff.Provider.Tool.ToolType)
            {
                _handoff = null;
                RaiseHandoffChanged();
            }

            _selected = value;
            _focusedRow = null;
            Recompute(value);
            BuildRows();
            Raise(nameof(Selected));
            Raise(nameof(SelectedHeading));
            Raise(nameof(ResultsBeside));
            Raise(nameof(ResultsBelow));
            Raise(nameof(ResultsHeading));
            Raise(nameof(FocusedRow));
            RaiseHelpChanged();
            Raise(nameof(Figures));
            Raise(nameof(CodeText));
            Raise(nameof(CurrentInstructions));
            CopyCodeCommand.RaiseCanExecuteChanged();
            SaveCodeCommand.RaiseCanExecuteChanged();
            DeleteCommand.RaiseCanExecuteChanged();
            ExportCommand.RaiseCanExecuteChanged();
            ReportCommand.RaiseCanExecuteChanged();
            ExportPdfCommand.RaiseCanExecuteChanged();
            ExportCsvCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// Says what the current method selection is hiding, so a reader is never left wondering
    /// where a field went.
    /// </summary>
    public string ScopeNote
    {
        get
        {
            if (Selected is null)
            {
                return string.Empty;
            }

            int hidden = MethodScope.HiddenProperties(Selected).Count;

            if (hidden == 0)
            {
                return string.Empty;
            }

            string methods = string.Join(
                ", ", MethodScope.ActiveMethods(Selected).Select(m => m.Name));

            return $"Showing {methods}. {hidden} field(s) belonging to other methods are hidden.";
        }
    }

    /// <summary>
    /// Where the open analysis's result will go, or empty when it was opened on its own.
    /// </summary>
    public string HandoffNote =>
        _handoff is null
            ? string.Empty
            : $"Result goes to {Describe(_handoff.Target)} → {_handoff.Property.Label}. " +
              "Set the inputs, then click Use result.";

    /// <summary>
    /// Opens the tool that declared it can fill <paramref name="property"/> on
    /// <paramref name="target"/>, remembering where to put the answer. Nothing here knows
    /// which tool or which property: the row asked the catalog, and the catalog asked the tools.
    /// </summary>
    public void BeginHandoff(StatistleObject target, StatistleProperty property, ValueProvider provider)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(property);
        ArgumentNullException.ThrowIfNull(provider);

        _handoff = new Handoff(target, property, provider);

        // The analysis that belongs to this object: blank the first time, then remembering
        // the numbers last used for it — apart from the rail's own instance.
        Analysis tool = Document.Tool(provider.Tool, target);
        tool.Recalculate();
        Selected = tool;
        Status = $"{provider.Tool.Title} — {provider.Tool.Summary}";

        // The tool declared which of its inputs to take from the object it was opened from.
        if (Seed(tool, target, provider.Value) > 0)
        {
            tool.Recalculate();
            BuildRows();
        }

        RaiseHandoffChanged();

        Status = $"{provider.Tool.Title} — result goes to {Describe(target)}, {property.Label}";
    }

    /// <summary>Copies the declared seed values from the target onto the tool; returns how many.</summary>
    private static int Seed(Analysis tool, StatistleObject target, ProvidedValue provided)
    {
        int copied = 0;

        foreach ((string toolName, string targetName) in provided.Seeds)
        {
            StatistleProperty? into = tool.Properties().FirstOrDefault(p => p.XmlName == toolName);
            StatistleProperty? from = target.Properties().FirstOrDefault(p => p.XmlName == targetName);

            if (into is null || from is null || !into.Property.CanWrite || from.Read(target) is not object value)
            {
                continue;
            }

            try
            {
                Type type = Nullable.GetUnderlyingType(into.Property.PropertyType) ?? into.Property.PropertyType;
                into.Property.SetValue(tool, Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture));
                copied++;
            }
            catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
            {
                // A seed that does not fit is simply not applied; the user can still type it.
            }
        }

        return copied;
    }

    private void UseResult()
    {
        if (_handoff is null || Selected is not Analysis tool)
        {
            return;
        }

        StatistleProperty? result = tool.Properties()
            .FirstOrDefault(p => p.XmlName == _handoff.Provider.Value.ResultProperty);

        if (result is null)
        {
            Status = $"{tool.Title} has no '{_handoff.Provider.Value.ResultProperty}' to hand over.";
            return;
        }

        (StatistleObject target, StatistleProperty property, _) = _handoff;
        object? value = result.Read(tool);

        PushUndo();

        try
        {
            // Both sides hold base units, so the value crosses as it is; only the type may differ.
            Type type = Nullable.GetUnderlyingType(property.Property.PropertyType) ?? property.Property.PropertyType;
            property.Property.SetValue(
                target, value is null ? null : Convert.ChangeType(value, type, System.Globalization.CultureInfo.InvariantCulture));
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            Status = $"Could not put a {value?.GetType().Name} into {property.Label}: {ex.Message}";
            return;
        }

        _handoff = null;
        Selected = target;
        MarkDirty();
        Autosave();
        RaiseHandoffChanged();

        Status = $"{property.Label} set to {target.Display(property, Document.Units)} " +
                 $"{Document.Units.LabelFor(property.Unit)} from {tool.Title}".TrimEnd();
    }

    private void CancelHandoff()
    {
        if (_handoff is null)
        {
            return;
        }

        StatistleObject target = _handoff.Target;
        _handoff = null;
        Selected = target;
        RaiseHandoffChanged();
        Status = "Calculation cancelled; nothing was changed";
    }

    private void RaiseHandoffChanged()
    {
        Raise(nameof(HandoffNote));
        UseResultCommand.RaiseCanExecuteChanged();
        CancelHandoffCommand.RaiseCanExecuteChanged();
    }

    private static string Describe(StatistleObject artifact) => artifact switch
    {
        Analysis t => t.Title,
        _ => $"{artifact.GetType().Name.ToLowerInvariant()} '{artifact.Name}'",
    };

    public string SelectedHeading => Selected switch
    {
        null => "Nothing selected",
        Analysis a => a.Title,
        DataTable t => $"Data table — {t.Name}",
        _ => Selected.Name,
    };

    public string Title
    {
        get
        {
            string file = Document.FilePath is null
                ? "Untitled"
                : Path.GetFileName(Document.FilePath);

            return $"Statistle — {file}{(IsDirty ? " *" : string.Empty)}";
        }
    }

    public bool IsDirty
    {
        get => _isDirty;
        private set
        {
            _isDirty = value;
            Raise(nameof(IsDirty));
            Raise(nameof(Title));
        }
    }

    public string Status
    {
        get => _status;
        private set
        {
            _status = value;
            Raise(nameof(Status));
        }
    }

    /// <summary>
    /// What the Plots tab shows and the report carries: every figure the selected item
    /// declares. A regression gives its points and fitted curve; another analysis gives whatever
    /// it has. Nothing is named here — the item says how it is drawn.
    /// </summary>
    public IReadOnlyList<PlotSpec> Figures =>
        Selected is IHasFigures drawable ? drawable.Figures(Document.Units) : [];

    /// <summary>
    /// Draws figures for the report writers. Set by the window, which owns the charting
    /// library; null leaves the writers listing figures by title.
    /// </summary>
    public IFigureRenderer? FigureRenderer { get; set; }

    /// <summary>The report for whatever is selected — the one object every writer renders.</summary>
    public Report? CurrentReport =>
        Selected is null ? null : ReportBuilder.For(Selected, Document.Units);

    /// <summary>Opens a project the app was activated with (double-clicked .statistle file).</summary>
    public void OpenPath(string path)
    {
        try
        {
            Document = StatistleDocument.Open(path);
            Selected = Document.Tables.FirstOrDefault();
            IsDirty = false;

            Status = Document.Diagnostics.HasWarnings
                ? $"Opened {Path.GetFileName(path)} with {Document.Diagnostics.Warnings.Count} warning(s): " +
                  Document.Diagnostics.Warnings[0]
                : $"Opened {Path.GetFileName(path)} — " +
                  $"{Document.Tables.Count} data table(s), {Document.Tools.Count} analysis/analyses";
        }
        catch (Exception ex) when (ex is UnsupportedSchemaVersionException or InvalidDataException or IOException)
        {
            Status = $"Could not open {Path.GetFileName(path)}: {ex.Message}";
        }
    }

    /// <summary>Re-renders every row — used after a unit change.</summary>
    public void RefreshRows()
    {
        foreach (PropertyRow row in Rows)
        {
            row.Refresh();
        }

        Raise(nameof(Figures));

        Raise(nameof(CodeText));

        CopyCodeCommand.RaiseCanExecuteChanged();

        SaveCodeCommand.RaiseCanExecuteChanged();
    }

    public void MarkDirty()
    {
        IsDirty = true;
        Raise(nameof(Figures));
        Raise(nameof(CodeText));
        CopyCodeCommand.RaiseCanExecuteChanged();
        SaveCodeCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Runs after any edit made through a row: snapshot for undo, recompute, and rebuild the
    /// rows if the edit changed which method applies.
    /// </summary>
    private void OnEdited()
    {
        if (_restoring)
        {
            return;
        }

        IsDirty = true;

        Recompute(Selected);

        // Changing the method changes which fields belong on
        // the form. Compared as a set of names: two methods with one parameter each keep the
        // count the same while the row that should show changes.
        if (Selected is not null && !VisibleNames(Selected).SequenceEqual(Rows.Select(r => r.Property.XmlName)))
        {
            BuildRows();
        }
        else
        {
            foreach (PropertyRow row in Rows)
            {
                row.Refresh();
            }
        }

        Raise(nameof(ScopeNote));
        RaiseHelpChanged();
        Raise(nameof(Figures));
        Raise(nameof(CodeText));
        CopyCodeCommand.RaiseCanExecuteChanged();
        SaveCodeCommand.RaiseCanExecuteChanged();
        Autosave();
    }

    /// <summary>
    /// Brings an object's derived values up to date: an analysis recalculates; anything else has
    /// the methods that currently apply to it write their outputs. Display-only in effect, so it
    /// does not dirty the project.
    /// </summary>
    private static void Recompute(StatistleObject? subject)
    {
        switch (subject)
        {
            case null:
                return;
            case Analysis tool:
                tool.Recalculate();
                return;
            default:
                foreach (ICalculationMethod method in MethodScope.ActiveMethods(subject))
                {
                    method.Apply(subject);
                }

                return;
        }
    }

    /// <summary>Takes a snapshot of the project as it stands, before an edit changes it.</summary>
    public void PushUndo()
    {
        if (_restoring)
        {
            return;
        }

        try
        {
            _undo.Add(Snapshot());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return;
        }

        if (_undo.Count > UndoDepth)
        {
            _undo.RemoveAt(0);
        }

        UndoCommand.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// Restores the whole project from the snapshot taken before the last edit. Coarse, but it
    /// cannot leave the document half-restored the way a per-property stack can.
    /// </summary>
    private void Undo()
    {
        if (_undo.Count == 0)
        {
            return;
        }

        string snapshot = _undo[^1];
        _undo.RemoveAt(_undo.Count - 1);

        string? selectedName = Selected?.Name;
        string? path = Document.FilePath;

        _restoring = true;

        try
        {
            string temporary = Path.Combine(Path.GetTempPath(), $"statistle-undo-{Guid.NewGuid():N}.statistle");
            File.WriteAllBytes(temporary, Convert.FromBase64String(snapshot));

            StatistleDocument restored = StatistleDocument.Open(temporary);
            File.Delete(temporary);

            Document = restored;
            Document.SetFilePath(path);

            Selected = (StatistleObject?)Document.Tables.FirstOrDefault(t => t.Name == selectedName)
                       ?? Document.Tools.Values.FirstOrDefault(t => t.Name == selectedName);

            IsDirty = true;
            Status = "Undone";
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException or FormatException)
        {
            Status = $"Could not undo: {ex.Message}";
        }
        finally
        {
            _restoring = false;
            UndoCommand.RaiseCanExecuteChanged();
        }
    }

    private string Snapshot()
    {
        string temporary = Path.Combine(Path.GetTempPath(), $"statistle-snap-{Guid.NewGuid():N}.statistle");
        string? path = Document.FilePath;

        try
        {
            Document.Save(temporary);

            // Saving records where it was saved to; a snapshot is not a Save As.
            Document.SetFilePath(path);

            return Convert.ToBase64String(File.ReadAllBytes(temporary));
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    /// <summary>
    /// Writes a recovery copy to local app data after every edit. It is not the project file -
    /// saving stays explicit - but a crash costs nothing.
    /// </summary>
    private void Autosave()
    {
        string? path = Document.FilePath;

        try
        {
            string folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Statistle");

            Directory.CreateDirectory(folder);
            Document.Save(Path.Combine(folder, "autosave.statistle"));
            Document.SetFilePath(path);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Autosave must never interrupt the work it is protecting.
            Document.SetFilePath(path);
        }
    }

    private bool MarkDirtyAfterAdd()
    {
        return true;
    }

    private void New()
    {
        NewProject();
        Status = "New project";
    }

    private void NewProject()
    {
        Document = new StatistleDocument { Name = "Untitled project" };
        Selected = null;
        IsDirty = false;
    }

    private void Open()
    {
        // Documents\Statistle is the home of every Statistle file, wherever the exe lives.
        var dialog = new OpenFileDialog
        {
            Filter = Filter,
            DefaultExt = ".statistle",
            InitialDirectory = DefaultFolder(),
        };
        if (dialog.ShowDialog() == true)
        {
            OpenPath(dialog.FileName);
        }
    }

    private void Save(bool saveAs)
    {
        string? path = Document.FilePath;

        if (saveAs || path is null)
        {
            var dialog = new SaveFileDialog
            {
                Filter = Filter,
                DefaultExt = ".statistle",
                FileName = string.IsNullOrWhiteSpace(Document.Name) ? "project.statistle" : Document.Name + ".statistle",
                InitialDirectory = Path.GetDirectoryName(path) ?? DefaultFolder(),
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            path = dialog.FileName;
        }

        try
        {
            Document.Save(path);
            IsDirty = false;
            Status = $"Saved {Path.GetFileName(path)}";
            Raise(nameof(Title));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not save: {ex.Message}";
        }
    }

    /// <summary>
    /// <c>Documents\Statistle</c>, created on demand. If it cannot be created the dialog simply
    /// opens wherever Windows would have put it.
    /// </summary>
    private static string DefaultFolder()
    {
        try
        {
            return StatistlePaths.EnsureDocumentsFolder();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return string.Empty;
        }
    }

    private void Delete()
    {
        PushUndo();

        switch (Selected)
        {
            case DataTable table:
                Document.Tables.Remove(table);
                break;
            default:
                return;
        }

        Selected = null;
        MarkDirty();
        Status = "Deleted";
    }

    private void Export()
    {
        if (Selected is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "XML artifact (*.xml)|*.xml",
            DefaultExt = ".xml",
            FileName = $"{Selected.GetType().Name.ToLowerInvariant()}-{Selected.Id}.xml",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            StatistleDocument.ExportArtifact(Selected, dialog.FileName);
            Status = $"Exported {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not export: {ex.Message}";
        }
    }

    /// <summary>
    /// The report for whatever is selected, as HTML with its figures embedded. Built from the
    /// property metadata, so every tool has one without writing any output code.
    /// </summary>
    public string ReportHtml =>
        CurrentReport is Report report ? HtmlReportWriter.Write(report, FigureRenderer) : string.Empty;

    private void ShowReport()
    {
        if (Selected is null)
        {
            return;
        }

        try
        {
            string file = Path.Combine(
                Path.GetTempPath(), $"statistle-report-{Guid.NewGuid():N}.html");

            File.WriteAllText(file, ReportHtml);

            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(file) { UseShellExecute = true });

            Status = $"Report opened for {Selected.Name}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException
                                       or System.ComponentModel.Win32Exception)
        {
            Status = $"Could not open the report: {ex.Message}";
        }
    }

    private void ExportPdf()
    {
        if (Selected is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "PDF (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = $"{Sanitise(Selected.Name)}.pdf",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            PdfReportWriter.Write(ReportBuilder.For(Selected, Document.Units), dialog.FileName, FigureRenderer);
            Status = $"Wrote {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not write the PDF: {ex.Message}";
        }
    }

    private void ExportCsv()
    {
        if (Selected is null)
        {
            return;
        }

        var dialog = new SaveFileDialog
        {
            Filter = "CSV (*.csv)|*.csv",
            DefaultExt = ".csv",
            FileName = $"{Sanitise(Selected.Name)}.csv",
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        try
        {
            // An item that holds a series exports its numbers; anything else exports the
            // report, which is every value it shows under the headings it shows them under.
            string csv = CsvExport.SeriesToCsv(Selected, Document.Units) is { Length: > 0 } series
                ? series
                : CsvExport.ReportToCsv(ReportBuilder.For(Selected, Document.Units));

            File.WriteAllText(dialog.FileName, csv);
            Status = $"Wrote {Path.GetFileName(dialog.FileName)}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Status = $"Could not write the CSV: {ex.Message}";
        }
    }

    private static string Sanitise(string name)
    {
        string cleaned = string.IsNullOrWhiteSpace(name) ? "statistle" : name;

        return string.Concat(cleaned.Split(Path.GetInvalidFileNameChars()));
    }

    private void Select(StatistleObject artifact)
    {
        Selected = artifact;
        MarkDirty();
        Status = $"Added {artifact.Name}";
    }

    private void BuildRows()
    {
        foreach (PropertyRow old in Rows)
        {
            old.Detach();
        }

        Rows.Clear();
        EntryRows.Clear();
        ResultRows.Clear();

        if (Selected is null)
        {
            return;
        }

        // The rows shown are the ones the selected method actually uses. Nothing here knows
        // what Horton is - MethodScope asks the methods, and they declared it. A property
        // declared Hidden is set some other way (a result clicked) and is not offered.
        foreach (StatistleProperty property in MethodScope.VisibleProperties(Selected).Where(p => !p.Hidden))
        {
            var row = new PropertyRow(Selected, property, Document.Units, OnEdited);
            Rows.Add(row);

            if (property.IsResult)
            {
                ResultRows.Add(row);
            }
            else
            {
                EntryRows.Add(row);
            }
        }

        Raise(nameof(ScopeNote));
    }

    /// <summary>The visible rows the scoping check compares against, hidden rows excluded.</summary>
    private static IEnumerable<string> VisibleNames(StatistleObject subject) =>
        MethodScope.VisibleProperties(subject).Where(p => !p.Hidden).Select(p => p.XmlName);

    private void Raise([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

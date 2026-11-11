using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using Learning.Core.Content;

namespace Statistle.App.Learning;

/// <summary>
/// A row of the learning path's tree (ported from PLC+): My progress, a module, or an item, with its mark
/// (★ mastered, ✓ complete, ↺ review suggested, ◐ in progress, ○ not started), a line of detail, the words a screen reader
/// says and a tooltip. Written with plain <see cref="INotifyPropertyChanged"/>, so Statistle needs no MVVM package for it.
/// The main window may add rows of its own (the Workbench) and keep what it needs in <see cref="Tag"/>.
/// </summary>
public sealed class PathNode : INotifyPropertyChanged
{
    /// <summary>The "My progress" row at the top of the tree.</summary>
    public bool IsDashboard { get; init; }

    /// <summary>The module of a module row, or the module an item row is in.</summary>
    public LearningModule? Module { get; init; }

    /// <summary>The item of an item row.</summary>
    public LearningItem? Item { get; init; }

    public PathNode? Parent { get; init; }

    /// <summary>Anything the window keeps with a row of its own (a Workbench node, say). The path never reads it.</summary>
    public object? Tag { get; init; }

    public ObservableCollection<PathNode> Children { get; } = [];

    private string _header = "";
    private string _mark = "";
    private Brush _markBrush = Brushes.Gray;
    private string _detail = "";
    private string _spokenName = "";
    private string _tip = "";
    private FontWeight _weight = FontWeights.Normal;
    private bool _isExpanded;
    private bool _isSelected;

    public string Header { get => _header; set => Set(ref _header, value); }
    public string Mark { get => _mark; set => Set(ref _mark, value); }
    public Brush MarkBrush { get => _markBrush; set => Set(ref _markBrush, value); }
    public string Detail { get => _detail; set => Set(ref _detail, value); }
    public string SpokenName { get => _spokenName; set => Set(ref _spokenName, value); }
    public string Tip { get => _tip; set => Set(ref _tip, value); }
    public FontWeight Weight { get => _weight; set => Set(ref _weight, value); }
    public bool IsExpanded { get => _isExpanded; set => Set(ref _isExpanded, value); }
    public bool IsSelected { get => _isSelected; set => Set(ref _isSelected, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public override string ToString() => SpokenName.Length > 0 ? SpokenName : Header;

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

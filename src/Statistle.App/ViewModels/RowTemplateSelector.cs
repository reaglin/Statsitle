using System.Windows;
using System.Windows.Controls;

namespace Statistle.App.ViewModels;

/// <summary>
/// Picks the editor for a row from the property's own type — a combo box for an enum, a grid
/// for a series, a text box for everything else. Adding a domain property needs no edit here.
/// </summary>
public sealed class RowTemplateSelector : DataTemplateSelector
{
    public DataTemplate? Text { get; set; }

    public DataTemplate? Enum { get; set; }

    public DataTemplate? Series { get; set; }

    public DataTemplate? Choice { get; set; }

    public DataTemplate? Table { get; set; }

    public override DataTemplate? SelectTemplate(object item, DependencyObject container) =>
        item switch
        {
            PropertyRow { IsTable: true } => Table,
            PropertyRow { IsChoice: true } => Choice,
            PropertyRow { IsEnum: true } => Enum,
            PropertyRow { IsSeries: true } => Series,
            _ => Text,
        };
}

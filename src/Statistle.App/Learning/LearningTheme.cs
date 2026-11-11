using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Statistle.App.Learning;

/// <summary>
/// The colours and text styles the learning pieces use (the start screen, its dialogs, the quiz, the path's marks), as one
/// resource dictionary. Each piece merges it into its own resources, so it works whether or not App.xaml has them; the main
/// window can merge <see cref="Dictionary"/> into its own resources to share them. The keys are PLC+'s, with Statistle's blue
/// as the accent. Colours as resources, so a high-contrast theme can replace them later (checklist).
/// </summary>
public static class LearningTheme
{
    private static ResourceDictionary? _dictionary;

    /// <summary>
    /// PageBrush, PanelBrush, InkBrush, MutedTextBrush, LineBrush, AccentBrush, PowerOnBrush (right, complete) and WrongBrush;
    /// the TextBlock styles SectionTitle, PanelTitle and Lead.
    /// </summary>
    public static ResourceDictionary Dictionary => _dictionary ??= Build();

    private static ResourceDictionary Build()
    {
        var d = new ResourceDictionary
        {
            ["PageBrush"] = Frozen(0xF7, 0xF8, 0xFA),
            ["PanelBrush"] = Frozen(0xFF, 0xFF, 0xFF),
            ["InkBrush"] = Frozen(0x1B, 0x24, 0x30),
            ["MutedTextBrush"] = Frozen(0x4A, 0x55, 0x63),
            ["LineBrush"] = Frozen(0xD5, 0xDB, 0xE1),
            ["AccentBrush"] = Frozen(0x1F, 0x5F, 0xA8),
            ["PowerOnBrush"] = Frozen(0x1E, 0x7B, 0x34),
            ["WrongBrush"] = Frozen(0xA4, 0x1E, 0x1E),
        };

        var section = new Style(typeof(TextBlock));
        section.Setters.Add(new Setter(TextBlock.FontSizeProperty, 20.0));
        section.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        section.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 4)));
        section.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
        d["SectionTitle"] = section;

        var panel = new Style(typeof(TextBlock));
        panel.Setters.Add(new Setter(TextBlock.FontSizeProperty, 16.0));
        panel.Setters.Add(new Setter(TextBlock.FontWeightProperty, FontWeights.SemiBold));
        panel.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 6)));
        d["PanelTitle"] = panel;

        var lead = new Style(typeof(TextBlock));
        lead.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));
        lead.Setters.Add(new Setter(TextBlock.ForegroundProperty, d["MutedTextBrush"]));
        lead.Setters.Add(new Setter(FrameworkElement.MarginProperty, new Thickness(0, 0, 0, 8)));
        d["Lead"] = lead;

        // Buttons with room around the words, and their tooltips shown even when they are off (they say why).
        var button = new Style(typeof(Button));
        button.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(12, 5, 12, 5)));
        button.Setters.Add(new Setter(ToolTipService.ShowOnDisabledProperty, true));
        d[typeof(Button)] = button;
        return d;
    }

    /// <summary>A brush from the theme, or from the application's resources for a key the theme does not have.</summary>
    public static Brush Brush(string key) =>
        Dictionary.Contains(key) ? (Brush)Dictionary[key] : Application.Current?.TryFindResource(key) as Brush ?? Brushes.Gray;

    /// <summary>Puts the theme into an element's resources (once).</summary>
    public static void Apply(FrameworkElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (!element.Resources.MergedDictionaries.Contains(Dictionary)) element.Resources.MergedDictionaries.Add(Dictionary);
    }

    private static SolidColorBrush Frozen(byte r, byte g, byte b)
    {
        var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
        brush.Freeze();
        return brush;
    }
}

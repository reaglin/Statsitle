using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Learning.Core.Content;
using Learning.Core.Reports;
using Statistle.App.Plotting;
using Statistle.Core.Plotting;
using Statistle.Learning;

namespace Statistle.App.Learning;

/// <summary>
/// The picture of an exercise that passed, for a report's exercise evidence (plan 14.1): the results the learner typed beside
/// the values the analysis calculated, then the exercise's first figure. Drawn on white with no transparency (PDF/A).
/// </summary>
public static class ExerciseEvidence
{
    private const double Width = 760;

    /// <summary>The picture as PNG bytes, or null when the exercise is no longer in the course (the report then leaves it out).</summary>
    public static byte[]? Png(StatsContent content, ProgramEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(evidence);
        if (content.Find(evidence.ChallengeId) is not { } task) return null;
        BuiltTask built;
        try { built = StatsTaskRunner.Build(task); }
        catch (ContentException) { return null; }

        var answers = StatsTaskRunner.ReadAnswers(evidence.Program);
        var panel = new StackPanel { Background = Brushes.White, Width = Width };
        panel.Children.Add(new TextBlock
        {
            Text = built.Analysis.Title, FontFamily = new FontFamily("Segoe UI"), FontSize = 15, FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.Black, Margin = new Thickness(12, 10, 12, 6),
        });
        panel.Children.Add(Results(built, answers));

        if (built.Analysis is IHasFigures drawable && drawable.Figures(null) is { Count: > 0 } figures)
        {
            var png = new ScottPlotFigureRenderer().RenderPng(figures[0], (int)(Width - 24), 420);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.StreamSource = new MemoryStream(png);
            image.EndInit();
            panel.Children.Add(new Image { Source = image, Width = Width - 24, Margin = new Thickness(12, 4, 12, 12) });
        }

        return Render(panel);
    }

    /// <summary>One row per asked result: what it is, the learner's answer, the calculated value, and ✓ or ✗.</summary>
    private static Grid Results(BuiltTask built, IReadOnlyDictionary<string, string> answers)
    {
        var grid = new Grid { Margin = new Thickness(12, 0, 12, 8) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
        Row(grid, 0, ["Result", "Learner's answer", "Calculated", ""], header: true);
        var r = 1;
        foreach (var expected in built.Expected)
        {
            answers.TryGetValue(expected.Asked.Result, out var typed);
            var right = StatsTaskRunner.Check(expected, typed) is null;
            Row(grid, r++, [expected.Label, string.IsNullOrWhiteSpace(typed) ? "–" : typed.Trim(), expected.Shown, right ? "✓" : "✗"], header: false);
        }
        return grid;
    }

    private static void Row(Grid grid, int row, string[] cells, bool header)
    {
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var c = 0; c < cells.Length; c++)
        {
            var cell = new Border
            {
                BorderBrush = new SolidColorBrush(Color.FromRgb(0xC8, 0xD0, 0xDA)), BorderThickness = new Thickness(0, 0, 0, 1),
                Background = header ? new SolidColorBrush(Color.FromRgb(0xEE, 0xF3, 0xF8)) : Brushes.White,
                Padding = new Thickness(6, 3, 6, 3),
                Child = new TextBlock
                {
                    Text = cells[c], TextWrapping = TextWrapping.Wrap, FontFamily = new FontFamily("Segoe UI"), FontSize = 12.5,
                    FontWeight = header ? FontWeights.SemiBold : FontWeights.Normal, Foreground = Brushes.Black,
                    TextAlignment = c is 1 or 2 ? TextAlignment.Right : c == 3 ? TextAlignment.Center : TextAlignment.Left,
                },
            };
            Grid.SetRow(cell, row);
            Grid.SetColumn(cell, c);
            grid.Children.Add(cell);
        }
    }

    private static byte[] Render(FrameworkElement panel)
    {
        panel.Measure(new Size(Width, double.PositiveInfinity));
        panel.Arrange(new Rect(new Size(Width, panel.DesiredSize.Height)));
        panel.UpdateLayout();
        const double dpi = 192;
        var bitmap = new RenderTargetBitmap((int)Math.Ceiling(Width * dpi / 96), (int)Math.Ceiling(panel.ActualHeight * dpi / 96), dpi, dpi, PixelFormats.Pbgra32);
        bitmap.Render(panel);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(new FormatConvertedBitmap(bitmap, PixelFormats.Bgr24, null, 0)));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }
}

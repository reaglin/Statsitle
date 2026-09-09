using Statistle.Core.Analyses;
using Statistle.Core.Domain;
using Statistle.Core.Instructions;
using Statistle.Core.Persistence;

namespace Statistle.Tests;

/// <summary>
/// Every item in the rail has a full instruction page: a hand-written guide plus everything the
/// metadata already says. A new analysis without a guide fails here, so the instructions ship
/// with the analysis rather than after it.
/// </summary>
public class InstructionsTests
{
    private static IEnumerable<Type> ShippedTypes() =>
        AnalysisCatalog.Discover(typeof(Analysis).Assembly).Select(d => d.ToolType).Append(typeof(DataTable));

    [Fact]
    public void Every_shipped_item_has_a_guide_with_all_its_parts()
    {
        foreach (Type type in ShippedTypes())
        {
            Guide? guide = GuideCatalog.For(type);
            Assert.True(guide is not null, $"{type.Name} has no entry in GuideCatalog.");
            Assert.True(guide.WhenToUse.Length > 200, $"{type.Name}: 'when to use' is too thin.");
            Assert.True(guide.Steps.Count >= 3, $"{type.Name}: fewer than three steps.");
            Assert.All(guide.Steps, s => Assert.True(s.Length > 30, $"{type.Name}: a step is too thin."));
            Assert.True(guide.ReadingResults.Length > 150, $"{type.Name}: 'reading the results' is too thin.");
            Assert.True(guide.Pitfalls.Count >= 2, $"{type.Name}: fewer than two pitfalls.");
            Assert.True(guide.Example.Length > 80, $"{type.Name}: the worked example is too thin.");
            Assert.True(guide.Code.Length > 40, $"{type.Name}: the code note is too thin.");
        }

        Assert.All(GuideCatalog.Types, t => Assert.Contains(t, ShippedTypes()));   // no guide for a type that no longer ships
    }

    [Fact]
    public void The_page_carries_every_entry_result_and_method_of_the_item()
    {
        foreach (Type type in ShippedTypes())
        {
            InstructionSet page = InstructionsBuilder.For(type);
            string text = InstructionsTextWriter.Write(page);
            string html = InstructionsHtmlWriter.Write(page);

            Assert.False(string.IsNullOrWhiteSpace(page.Title));
            Assert.Contains(page.Blocks, b => b is Heading { Text: "When to use it" });
            Assert.Contains(page.Blocks, b => b is Steps);

            foreach (StatistleProperty property in StatistleObject.PropertiesOf(type).Where(p => !p.Hidden && p.XmlName is not ("Id" or "Name" or "Description")))
            {
                Assert.True(text.Contains(property.Label + ": ", StringComparison.Ordinal), $"{type.Name}: '{property.Label}' is not on the page.");
                Assert.True(property.HasHelp, $"{type.Name}.{property.XmlName} has no help, so the page cannot explain it.");
            }

            foreach (var method in Core.Methods.MethodCatalog.For(type))
            {
                Assert.Contains(method.Name + ": ", text, StringComparison.Ordinal);
                Assert.Contains(method.Citation, text, StringComparison.Ordinal);
            }

            Assert.Contains("<h1>", html, StringComparison.Ordinal);
            Assert.Contains("<ol>", html, StringComparison.Ordinal);
            Assert.DoesNotContain("<dt></dt>", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void The_overview_lists_every_analysis_in_course_order()
    {
        InstructionSet page = InstructionsBuilder.Overview();
        string text = InstructionsTextWriter.Write(page);
        IReadOnlyList<AnalysisDescriptor> analyses = AnalysisCatalog.Discover(typeof(Analysis).Assembly);

        int last = -1;

        foreach (AnalysisDescriptor analysis in analyses.OrderBy(a => a.Order))
        {
            int at = text.IndexOf(analysis.Title + ": ", StringComparison.Ordinal);
            Assert.True(at > last, $"{analysis.Title} is missing or out of order in the overview.");
            last = at;
        }

        Assert.Contains("Documents\\Statistle", text, StringComparison.Ordinal);
    }

    [Fact]
    public void An_enum_entry_lists_its_choices_and_a_scoped_entry_names_its_methods()
    {
        string text = InstructionsTextWriter.Write(InstructionsBuilder.For(typeof(OneWayAnova)));

        Assert.Contains("Choices: Each column is a group; A factor column and a response column.", text, StringComparison.Ordinal);
        Assert.Contains("Shown with: Groups as columns.", text, StringComparison.Ordinal);
        Assert.Contains("Range 0.0001", text, StringComparison.Ordinal);
    }
}

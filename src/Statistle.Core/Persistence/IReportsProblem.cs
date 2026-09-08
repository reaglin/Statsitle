namespace Statistle.Core.Persistence;

/// <summary>
/// An object that can say why it could not compute — an analysis whose chosen column is no
/// longer in the project, or is empty. Null when all is well. The workspace shows it beside the
/// form rather than leaving a blank result unexplained.
/// </summary>
public interface IReportsProblem
{
    string? Problem { get; }
}

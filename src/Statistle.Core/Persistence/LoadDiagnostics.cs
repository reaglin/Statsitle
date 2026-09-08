namespace Statistle.Core.Persistence;

/// <summary>Something in a file that could not be read, and what was done about it.</summary>
public sealed record LoadWarning(string Location, string Value, string Reason)
{
    public override string ToString() => $"{Location}: {Reason} (value was \"{Value}\")";
}

/// <summary>
/// Collects what a tolerant read had to skip.
///
/// Tolerance is deliberate — a missing element must take its default, and an unknown element
/// must be ignored, or adding a property breaks every existing file. But Statistle 10's
/// <c>SetValue</c> swallowed every conversion exception with no record, so a corrupt file
/// loaded as silently wrong numbers. Here the read still succeeds and still says what it lost.
/// </summary>
public sealed class LoadDiagnostics
{
    private readonly List<LoadWarning> _warnings = [];

    public IReadOnlyList<LoadWarning> Warnings => _warnings;

    public bool HasWarnings => _warnings.Count > 0;

    public void Add(string location, string value, string reason) =>
        _warnings.Add(new LoadWarning(location, value, reason));

    public override string ToString() => string.Join(Environment.NewLine, _warnings);
}

namespace Statistle.Core.Persistence;

/// <summary>
/// An object some of whose properties refer to another object in the project — an analysis's
/// chosen data table. The editor puts an Edit link beside such a property that takes the user to
/// the object; it names no type.
/// </summary>
public interface ILinksTo
{
    /// <summary>The object a property refers to, or null when it refers to none.</summary>
    StatistleObject? LinkedObject(string propertyName);
}

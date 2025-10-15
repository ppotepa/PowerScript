namespace PowerScript.Core.Syntax.Tokens.Interfaces;

/// <summary>
///     Interface for value tokens (literals like numbers, strings).
///     Provides access to the value representation.
/// </summary>
public interface IValue
{
    /// <summary>The string representation of the value</summary>
    public string Value { get; }
}
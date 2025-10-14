namespace Tokenez.Core.Syntax.Tokens.Interfaces;

/// <summary>
///     Interface for keyword tokens (FUNCTION, RETURN, etc.).
///     Provides access to the keyword string.
/// </summary>
internal interface IKeyWordToken
{
    /// <summary>The keyword text (e.g., "FUNCTION", "RETURN")</summary>
    public string KeyWord { get; }
}
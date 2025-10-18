using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

/// <summary>
/// Token representing a keyword from a custom syntax pattern loaded from .psx files.
/// Examples: TAKE, FILTER, MAP, etc. when they're part of pattern syntax
/// </summary>
public class CustomKeywordToken : Token, IKeyWordToken
{
    /// <summary>
    /// Pattern ID this keyword belongs to (e.g., "TAKE_FROM")
    /// </summary>
    public string PatternId { get; }

    /// <summary>
    /// Position of this keyword in the pattern sequence (0-based)
    /// Pattern "TAKE $count FROM $array" → TAKE=0, FROM=2
    /// </summary>
    public int PositionInPattern { get; }

    /// <summary>
    /// The pattern text this keyword belongs to
    /// </summary>
    public string PatternText { get; }

    public CustomKeywordToken(RawToken rawToken, string patternId, int positionInPattern, string patternText)
        : base(rawToken)
    {
        PatternId = patternId;
        PositionInPattern = positionInPattern;
        PatternText = patternText;
    }

    public override Type[] Expectations => Array.Empty<Type>();

    public override string KeyWord => RawToken?.Text?.ToUpperInvariant() ?? "";

    public override string ToString() =>
        $"CustomKeywordToken: {RawToken?.Text ?? "?"} (Pattern: {PatternId}, Pos: {PositionInPattern})";
}

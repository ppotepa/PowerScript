using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Values;

/// <summary>
///     Represents a decimal/floating-point number literal token.
///     Examples: 3.14, -2.5, 0.001
/// </summary>
public class DecimalToken : Token
{
    public double Value { get; set; }

    public DecimalToken()
    {
    }

    public DecimalToken(RawToken rawToken, double value) : base(rawToken)
    {
        Value = value;
    }

    /// <summary>After decimal value, context-dependent (handled by processors)</summary>
    public override Type[] Expectations => [];

    public override string ToString()
    {
        return $"DecimalToken '{RawToken?.Text}' (Value={Value})";
    }
}

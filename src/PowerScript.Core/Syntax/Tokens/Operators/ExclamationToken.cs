using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
/// Represents the '!' operator used for strict type marking.
/// Example: {name = "John"} as Person! (strict type, not extendable)
/// </summary>
public class ExclamationToken : Token
{
    public ExclamationToken()
    {
    }

    public ExclamationToken(RawToken raw) : base(raw)
    {
    }

    /// <summary>After '!', can continue with various tokens</summary>
    public override Type[] Expectations => [];
}

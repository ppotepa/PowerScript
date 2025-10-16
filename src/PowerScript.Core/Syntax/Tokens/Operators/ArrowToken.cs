using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the arrow operator (->).
///     Used for .NET member access (properties and methods).
///     Example: "str -> Length" or "person -> Speak()"
/// </summary>
public class ArrowToken : Token
{
    public ArrowToken()
    {
    }

    public ArrowToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After arrow, expect an identifier (property or method name)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];
}

using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Operators;

/// <summary>
///     Token representing the dot operator.
///     Used for member access in .NET paths.
///     Example: "System.Console.WriteLine"
/// </summary>
public class DotToken : Token
{
    public DotToken()
    {
    }

    public DotToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After dot, expect an identifier (class or member name)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken)];
}
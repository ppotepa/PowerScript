using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Identifiers;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;

namespace PowerScript.Core.Syntax.Tokens.Keywords.Types;

/// <summary>
///     Token representing 'STRING' - equivalent to 'CHAR CHAIN'.
///     Used for string literals and text data.
///     Example: "VAR STRING name = "Hello"" or "FUNCTION getName()[STRING]"
/// </summary>
public class StringToken : Token, IKeyWordToken, IBaseTypeToken
{
    public StringToken()
    {
    }

    public StringToken(RawToken rawToken) : base(rawToken)
    {
    }

    public override Type[] Expectations => [typeof(IdentifierToken)];
}
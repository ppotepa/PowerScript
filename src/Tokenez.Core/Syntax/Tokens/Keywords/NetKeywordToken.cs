using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Operators;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the NET keyword or # shorthand.
///     Used to access .NET framework functionality directly.
///     Example: "NET::System.Console.WriteLine(...)" or "#Console.WriteLine(...)"
/// </summary>
public class NetKeywordToken : Token, IKeyWordToken
{
    public NetKeywordToken()
    {
    }

    public NetKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After NET, expect namespace operator :: OR after #, expect identifier (class name)</summary>
    public override Type[] Expectations => [typeof(NamespaceOperatorToken), typeof(IdentifierToken)];

    public override string KeyWord => "NET";
}
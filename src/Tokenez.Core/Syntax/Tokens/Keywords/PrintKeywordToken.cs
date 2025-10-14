using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Delimiters;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing the PRINT keyword.
///     Used to print output to console (maps to Console.WriteLine).
///     Example: "PRINT ( "Hello World" )"
/// </summary>
public class PrintKeywordToken : Token, IKeyWordToken
{
    public PrintKeywordToken()
    {
    }

    public PrintKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After PRINT, expect either a string literal, template string, or opening parenthesis for arguments</summary>
    public override Type[] Expectations =>
        [typeof(StringLiteralToken), typeof(TemplateStringToken), typeof(ParenthesisOpen)];

    public override string KeyWord => "PRINT";
}
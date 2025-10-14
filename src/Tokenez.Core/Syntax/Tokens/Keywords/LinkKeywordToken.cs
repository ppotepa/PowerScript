using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Token representing 'LINK' - the library/file import keyword.
///     Used to import external libraries or files into the current context.
///     Example: "LINK System" or "LINK `path/to/file.ps`"
///     Must appear at the top of the script before any other statements.
/// </summary>
public class LinkKeywordToken : Token, IKeyWordToken
{
    public LinkKeywordToken()
    {
    }

    public LinkKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After LINK, expect identifier (library name) or string literal (file path)</summary>
    public override Type[] Expectations => [typeof(IdentifierToken), typeof(StringLiteralToken)];
}
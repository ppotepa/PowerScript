using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords;

/// <summary>
///     Represents the EXECUTE keyword token for running external PowerScript files.
///     Syntax: EXECUTE "filename.ps"
///     Example: EXECUTE "utils.ps"
/// </summary>
public class ExecuteKeywordToken : Token, IKeyWordToken
{
    public ExecuteKeywordToken()
    {
    }

    public ExecuteKeywordToken(RawToken rawToken) : base(rawToken)
    {
    }

    /// <summary>After EXECUTE, expect a string literal file path</summary>
    public override Type[] Expectations => [typeof(StringLiteralToken)];

    public override string KeyWord => "EXECUTE";
}
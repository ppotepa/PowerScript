using PowerScript.Core.Syntax.Tokens.Base;
using PowerScript.Core.Syntax.Tokens.Interfaces;
using PowerScript.Core.Syntax.Tokens.Raw;
using PowerScript.Core.Syntax.Tokens.Values;

namespace PowerScript.Core.Syntax.Tokens.Keywords;

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
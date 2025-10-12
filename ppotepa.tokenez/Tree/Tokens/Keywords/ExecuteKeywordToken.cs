using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
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
}
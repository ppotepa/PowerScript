using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the VAR keyword.
    /// Used to declare variables with optional type inference or explicit typing.
    /// Examples:
    /// - "VAR x = 10" - declares variable x with inferred type
    /// - "VAR INT x = 10" - declares variable x with explicit INT type
    /// </summary>
    public class VarKeywordToken : Token, IKeyWordToken
    {
        public VarKeywordToken()
        {
        }

        public VarKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After VAR, expect either a type token (INT) or an identifier (variable name)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken), typeof(IntToken)];

        public override string KeyWord => "VAR";
    }
}

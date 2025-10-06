using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the RETURN keyword.
    /// Used to return a value from a function.
    /// Example: "RETURN 42" or "RETURN x + y"
    /// </summary>
    public class ReturnKeywordToken : Token, IKeyWordToken
    {
        public ReturnKeywordToken()
        {
        }

        public ReturnKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After RETURN, expect an identifier or value (the expression to return)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

        public override string KeyWord => "RETURN";
    }
}
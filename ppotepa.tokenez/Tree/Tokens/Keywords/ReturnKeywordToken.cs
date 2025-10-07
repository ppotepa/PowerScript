using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the RETURN keyword.
    /// Used to return a value from a function or return void.
    /// Examples: 
    /// - "RETURN 42" - returns a value 
    /// - "RETURN x + y" - returns an expression
    /// - "RETURN" - void return (no value)
    /// </summary>
    public class ReturnKeywordToken : Token, IKeyWordToken
    {
        public ReturnKeywordToken()
        {
        }

        public ReturnKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After RETURN, expect an identifier, value, or scope end (for void return)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken), typeof(ScopeEndToken)];

        public override string KeyWord => "RETURN";
    }
}
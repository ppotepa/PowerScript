using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the IF keyword for conditional statements.
    ///     SQL-style syntax: IF condition { ... } or IF condition { ... } ELSE { ... }
    /// </summary>
    public class IfKeywordToken : Token, IKeyWordToken
    {
        public IfKeywordToken()
        {
        }

        public IfKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After IF, expect an identifier or value to start condition</summary>
        public override Type[] Expectations =>
        [
            typeof(IdentifierToken),
            typeof(ValueToken),
            typeof(StringLiteralToken)
        ];

        public override string KeyWord => "IF";
    }
}
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Interfaces;
using Tokenez.Core.Syntax.Tokens.Raw;
using Tokenez.Core.Syntax.Tokens.Values;

namespace Tokenez.Core.Syntax.Tokens.Keywords
{
    /// <summary>
    ///     Token representing the OR logical operator (SQL-style).
    ///     Used in conditional expressions: IF a > b OR c < d { ... }
    /// </summary>
    public class OrKeywordToken : Token, IKeyWordToken
    {
        public OrKeywordToken()
        {
        }

        public OrKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After OR, expect an identifier or value</summary>
        public override Type[] Expectations =>
        [
            typeof(IdentifierToken),
            typeof(ValueToken),
            typeof(StringLiteralToken)
        ];

        public override string KeyWord => "OR";
    }
}
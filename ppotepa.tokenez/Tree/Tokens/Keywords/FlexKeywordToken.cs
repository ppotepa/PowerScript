using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    /// <summary>
    /// Token representing the FLEX keyword for dynamic variable declaration.
    /// FLEX variables can change their type at runtime.
    /// Example: FLEX counter = 0  // Later can be: counter = "many"
    /// </summary>
    public class FlexKeywordToken : Token, IKeyWordToken
    {
        public FlexKeywordToken()
        {
        }

        public FlexKeywordToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After FLEX, expect a variable name (identifier)</summary>
        public override Type[] Expectations => [typeof(IdentifierToken)];

        public override string KeyWord => "FLEX";
    }
}

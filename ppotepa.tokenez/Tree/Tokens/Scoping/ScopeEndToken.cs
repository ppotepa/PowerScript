using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Scoping
{
    /// <summary>
    /// Token representing '}' - the end of a scope/block.
    /// Closes function bodies and other code blocks.
    /// Triggers validation that function scopes have RETURN statements.
    /// </summary>
    public class ScopeEndToken : Token
    {
        public ScopeEndToken()
        {
        }

        public ScopeEndToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After '}', no specific expectations (handled by parent scope)</summary>
        public override Type[] Expectations => [];

        public override string KeyWord => "}";
    }
}

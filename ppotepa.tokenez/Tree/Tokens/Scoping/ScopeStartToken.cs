using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens.Scoping
{
    /// <summary>
    /// Token representing '{' - the start of a scope/block.
    /// Used to begin function bodies and other code blocks.
    /// </summary>
    public class ScopeStartToken : Token
    {
        public ScopeStartToken()
        {
        }

        public ScopeStartToken(RawToken rawToken) : base(rawToken)
        {
        }

        /// <summary>After '{', expect PRINT, RETURN, FLEX, or nested FUNCTION declarations</summary>
        public override Type[] Expectations => [
            typeof(PrintKeywordToken),
            typeof(ReturnKeywordToken),
            typeof(FlexKeywordToken),
            typeof(FunctionToken),
            typeof(IfKeywordToken),
            typeof(CycleKeywordToken)
        ];

        public override string KeyWord => "{";
    }
}

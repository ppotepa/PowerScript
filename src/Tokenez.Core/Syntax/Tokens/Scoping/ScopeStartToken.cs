using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Core.Syntax.Tokens.Scoping
{
    /// <summary>
    ///     Token representing '{' - the start of a scope/block.
    ///     Used to begin function bodies and other code blocks.
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
        public override Type[] Expectations =>
        [
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
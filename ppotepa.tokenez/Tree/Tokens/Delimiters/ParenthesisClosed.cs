using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Tokens.Delimiters
{
    public class ParenthesisClosed : Token
    {
        public ParenthesisClosed()
        {
        }

        public ParenthesisClosed(RawToken rawToken) : base(rawToken)
        {
        }

        // After closing parenthesis - scope start is already provided by UserPrompt wrapping
        public override Type[] Expectations => [typeof(ScopeStartToken)];
    }
}
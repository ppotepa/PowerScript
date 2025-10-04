using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    public class FunctionToken : Token, IKeyWordToken
    {
        public FunctionToken()
        {

        }

        public FunctionToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ParameterArrayToken), typeof(ScopeStart), typeof(ReturnKeywordToken), typeof(ScopeEnd)];
        public string Word => "FUNCTION";

    }
}
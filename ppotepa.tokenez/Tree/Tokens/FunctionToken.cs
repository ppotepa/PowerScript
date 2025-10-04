using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    internal class FunctionToken : Token, IKeyWordToken
    {
        public FunctionToken()
        {
        }

        public FunctionToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Token[] Expects => [new IdentifierToken(), new ReturnKeywordToken()];

        public string Word => "FUNCTION";
    }
}
namespace ppotepa.tokenez.Tree.Tokens
{
    internal class ReturnKeywordToken : Token, IKeyWordToken
    {
        public ReturnKeywordToken()
        {
        }

        public override Token[] Expects => [new IdentifierToken(), new ValueToken()];

        public string Word => "RETURN";
    }
}
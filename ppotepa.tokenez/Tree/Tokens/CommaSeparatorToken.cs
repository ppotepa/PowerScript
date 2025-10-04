namespace ppotepa.tokenez.Tree.Tokens
{
    internal class CommaSeparatorToken : Token
    {
        public CommaSeparatorToken()
        {
        }

        public override Token[] Expects => [new IdentifierToken()];
    }
}
namespace ppotepa.tokenez.Tree.Tokens
{
    internal class ReturnKeywordToken : Token, IKeyWordToken
    {
        public ReturnKeywordToken()
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];

        public override string Word => "RETURN";
    }
}
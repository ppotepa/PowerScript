namespace ppotepa.tokenez.Tree.Tokens
{
    internal class CommaSeparatorToken : Token
    {
        public CommaSeparatorToken()
        {
        }

        public override Type[] Expectations => [typeof(IdentifierToken), typeof(ValueToken)];
      
    }
}
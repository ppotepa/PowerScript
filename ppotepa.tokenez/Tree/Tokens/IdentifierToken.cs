using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    public class IdentifierToken : Token, IValue
    {
        public IdentifierToken()
        {

        }

        public IdentifierToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];

        public string Value { get; }
    }
}
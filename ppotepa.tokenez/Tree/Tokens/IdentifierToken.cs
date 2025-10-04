using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree.Tokens
{
    internal class IdentifierToken : Token, IValue
    {
        public IdentifierToken()
        {
        }

        public IdentifierToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Token[] Expects => [];

        public string Value { get; }
    }
}
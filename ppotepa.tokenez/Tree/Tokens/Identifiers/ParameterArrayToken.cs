using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Tokens.Identifiers
{
    internal class ParameterArrayToken : Token
    {
        public int ParameterCount = 0;
        public override Type[] Expectations => [];
    }
}
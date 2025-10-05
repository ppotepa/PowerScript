using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Raw;

namespace ppotepa.tokenez.Tree
{
    internal class FunctionParametersToken : Token
    {
        public FunctionParametersToken()
        {
        }

        public FunctionParametersToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [];
        public List<Declaration> Declarations { get; set; } = new();  
    }
}
using Tokenez.Core.Syntax.Tokens.Base;

namespace Tokenez.Core.Syntax.Tokens.Identifiers
{
    internal class ParameterArrayToken : Token
    {
        public int ParameterCount = 0;
        public override Type[] Expectations => [];
    }
}
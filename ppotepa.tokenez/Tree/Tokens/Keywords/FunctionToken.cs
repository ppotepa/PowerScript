using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;

namespace ppotepa.tokenez.Tree.Tokens.Keywords
{
    public class FunctionToken : Token, IKeyWordToken
    {
        public FunctionToken()
        {

        }

        public FunctionToken(RawToken rawToken) : base(rawToken)
        {
        }

        public override Type[] Expectations => [
            typeof(IdentifierToken),
            typeof(ParameterArrayToken),
            typeof(ScopeStart),            
            typeof(Scope),
            typeof(ParenthesisClosed),
            typeof(ScopeEnd)
        ];

        public override string KeyWord => "FUNCTION";
    }
}

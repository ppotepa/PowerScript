using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Core.Syntax.Tokens.Identifiers;

internal class ParameterArrayToken : Token
{
    public int ParameterCount = 0;
    public override Type[] Expectations => [];
}
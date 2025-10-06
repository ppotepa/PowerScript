using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    public class ReturnStatement : Statement
    {
        public Expression ReturnValue { get; set; }

        public override string StatementType => "RETURN";

        public ReturnStatement(Token returnToken, Expression returnValue)
        {
            StartToken = returnToken;
            ReturnValue = returnValue;
        }
    }
}

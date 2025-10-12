using ppotepa.tokenez.Tree.Tokens.Base;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    ///     Represents a function call statement.
    ///     Example: sayHello() or add(5, 3)
    /// </summary>
    public class FunctionCallStatement : Statement
    {
        public string FunctionName { get; set; } = string.Empty;
        public List<Token> Arguments { get; set; } = [];

        public override string StatementType => "FUNCTION_CALL";

        public override string ToString()
        {
            return $"FunctionCall: {FunctionName}({Arguments.Count} args)";
        }
    }
}
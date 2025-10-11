using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents a variable declaration statement.
    /// Example: "VAR x = 10" or "VAR INT y = 20"
    /// </summary>
    public class VariableDeclarationStatement : Statement
    {
        /// <summary>The variable declaration (name, type)</summary>
        public VariableDeclaration Declaration { get; set; }

        /// <summary>The initial value expression</summary>
        public Expression InitialValue { get; set; }

        public override string StatementType => "VAR";

        public VariableDeclarationStatement(VariableDeclaration declaration, Expression initialValue)
        {
            Declaration = declaration;
            InitialValue = initialValue;
        }

        public override string ToString()
        {
            if (Declaration.DeclarativeType != null)
            {
                return $"VAR {Declaration.DeclarativeType.RawToken?.Text} {Declaration.Identifier.RawToken?.Text} = {InitialValue}";
            }
            return $"VAR {Declaration.Identifier.RawToken?.Text} = {InitialValue}";
        }
    }
}

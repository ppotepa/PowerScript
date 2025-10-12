using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    ///     Represents a variable declaration statement.
    ///     Supports both statically typed (INT x = 10) and dynamically typed (FLEX x = 10) variables.
    /// </summary>
    public class VariableDeclarationStatement(VariableDeclaration declaration, Expression initialValue,
            bool isDynamic = false) : Statement
    {
        /// <summary>The variable declaration (name, type)</summary>
        public VariableDeclaration Declaration { get; set; } = declaration;

        /// <summary>The initial value expression</summary>
        public Expression InitialValue { get; set; } = initialValue;

        /// <summary>Whether this is a dynamically-typed FLEX variable</summary>
        public bool IsDynamic { get; set; } = isDynamic;

        public override string StatementType => IsDynamic ? "FLEX" : "VAR";

        public override string ToString()
        {
            var prefix = IsDynamic ? "FLEX" : "";

            return Declaration.DeclarativeType != null
                ? $"{prefix} {Declaration.DeclarativeType.RawToken?.Text} {Declaration.Identifier.RawToken?.Text} = {InitialValue}"
                : $"{prefix} {Declaration.Identifier.RawToken?.Text} = {InitialValue}";
        }
    }
}
using ppotepa.tokenez.Tree.Expressions;

namespace ppotepa.tokenez.Tree.Statements
{
    /// <summary>
    /// Represents a CYCLE loop statement.
    /// Count-based: CYCLE 5 AS i { ... }
    /// Collection-based: CYCLE IN collection AS item { ... }
    /// Supports automatic index variables (a, b, c based on nesting).
    /// </summary>
    public class CycleLoopStatement : Statement
    {
        /// <summary>The collection expression to iterate over (or count for count-based loops)</summary>
        public Expression CollectionExpression { get; }

        /// <summary>The loop variable name (automatic 'a', 'b', etc. or custom via AS)</summary>
        public string LoopVariableName { get; }

        /// <summary>The scope (body) of the loop</summary>
        public Scope LoopBody { get; set; }

        /// <summary>Nesting level of this loop (0 = outermost)</summary>
        public int NestingLevel { get; }

        /// <summary>True if this is a count-based loop (CYCLE 5), false if collection-based (CYCLE IN items)</summary>
        public bool IsCountBased { get; set; }

        public override string StatementType => "CYCLE_LOOP";

        public CycleLoopStatement(
            Expression collectionExpression,
            string loopVariableName,
            int nestingLevel = 0)
        {
            CollectionExpression = collectionExpression;
            LoopVariableName = loopVariableName;
            NestingLevel = nestingLevel;
            IsCountBased = false; // Default to collection-based
        }

        public override string ToString()
        {
            if (IsCountBased)
            {
                return $"CYCLE {CollectionExpression} AS {LoopVariableName}";
            }
            return $"CYCLE IN {CollectionExpression} AS {LoopVariableName}";
        }
    }
}

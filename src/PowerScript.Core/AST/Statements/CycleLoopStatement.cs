using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.AST.Statements;

/// <summary>
///     Represents a CYCLE loop statement.
///     Count-based: CYCLE 5 AS i { ... }
///     Collection-based: CYCLE IN collection AS item { ... }
///     Range-based: CYCLE -5 TO 5 AS i { ... }
///     While-based: CYCLE WHILE condition AS i { ... }
///     Supports automatic index variables (a, b, c based on nesting).
/// </summary>
public class CycleLoopStatement(
    Expression collectionExpression,
    string loopVariableName,
    int nestingLevel = 0) : Statement
{
    /// <summary>The collection expression to iterate over (or count for count-based loops)</summary>
    public Expression CollectionExpression { get; } = collectionExpression;

    /// <summary>The loop variable name (automatic 'a', 'b', etc. or custom via AS)</summary>
    public string LoopVariableName { get; } = loopVariableName;

    /// <summary>The scope (body) of the loop</summary>
    public Scope? LoopBody { get; set; }

    /// <summary>Nesting level of this loop (0 = outermost)</summary>
    public int NestingLevel { get; } = nestingLevel;

    /// <summary>True if this is a count-based loop (CYCLE 5), false if collection-based (CYCLE IN items)</summary>
    public bool IsCountBased { get; set; } = false; // Default to collection-based

    /// <summary>True if this is a while loop (CYCLE WHILE condition)</summary>
    public bool IsWhileLoop { get; set; } = false;

    /// <summary>True if this is a range loop (CYCLE start TO end)</summary>
    public bool IsRangeLoop { get; set; } = false;

    /// <summary>For range loops: the end expression</summary>
    public Expression? RangeEndExpression { get; set; }

    public override string StatementType => "CYCLE_LOOP";

    public override string ToString()
    {
        if (IsWhileLoop)
            return $"CYCLE WHILE {CollectionExpression} AS {LoopVariableName}";
        if (IsRangeLoop)
            return $"CYCLE {CollectionExpression} TO {RangeEndExpression} AS {LoopVariableName}";
        return IsCountBased
            ? $"CYCLE {CollectionExpression} AS {LoopVariableName}"
            : $"CYCLE IN {CollectionExpression} AS {LoopVariableName}";
    }
}
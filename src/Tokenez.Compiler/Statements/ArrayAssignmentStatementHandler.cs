using System;
using Tokenez.Common.Logging;
using Tokenez.Compiler.Core;
using Tokenez.Compiler.Core.Variables;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.AST.Statements;

namespace Tokenez.Compiler.Statements;

/// <summary>
/// Handles array element assignment statements.
/// Single Responsibility: Array element modification
/// </summary>
public class ArrayAssignmentStatementHandler
{
    private readonly VariableRegistry _variableRegistry;
    private readonly Func<Expression, object> _evaluateExpression;

    public ArrayAssignmentStatementHandler(
        VariableRegistry variableRegistry,
        Func<Expression, object> evaluateExpression)
    {
        _variableRegistry = variableRegistry ?? throw new ArgumentNullException(nameof(variableRegistry));
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public void ExecuteArrayAssignment(ArrayAssignmentStatement arrayAssignment)
    {
        if (arrayAssignment == null)
        {
            throw new ArgumentNullException(nameof(arrayAssignment));
        }

        IndexExpression indexExpr = arrayAssignment.IndexExpression;

        LoggerService.Logger.Debug($"[ARRAY_ASSIGN] Assigning to array element");

        // Evaluate the array expression to get the target array
        // For arr[0] this evaluates 'arr'
        // For matrix[0][1] this evaluates 'matrix[0]'
        object arrayValue = _evaluateExpression(indexExpr.ArrayExpression);

        if (arrayValue is not object[] array)
        {
            throw new InvalidOperationException($"Expression does not evaluate to an array. Actual type: {arrayValue?.GetType().Name ?? "null"}");
        }

        object indexValue = _evaluateExpression(indexExpr.Index);
        int index = Convert.ToInt32(indexValue);

        if (index < 0 || index >= array.Length)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range. Array length: {array.Length}");
        }

        object newValue = _evaluateExpression(arrayAssignment.ValueExpression);

        array[index] = newValue;

        LoggerService.Logger.Debug($"[ARRAY_ASSIGN] Set array[{index}] = {newValue}");
    }
}

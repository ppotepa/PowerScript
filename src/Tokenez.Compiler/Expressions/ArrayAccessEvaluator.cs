using Tokenez.Common.Logging;
using Tokenez.Core.AST.Expressions;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Evaluates array element access.
/// Single Responsibility: Array indexing and bounds checking
/// </summary>
public class ArrayAccessEvaluator
{
    private readonly Func<Expression, object> _evaluateExpression;

    public ArrayAccessEvaluator(Func<Expression, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public object GetArrayElement(object arrayValue, Expression indexExpression)
    {
        if (arrayValue == null)
        {
            throw new InvalidOperationException("Cannot access element of null array");
        }

        if (indexExpression == null)
        {
            throw new ArgumentNullException(nameof(indexExpression));
        }

        object indexValue = _evaluateExpression(indexExpression);
        int index = ConvertToInteger(indexValue);

        if (arrayValue is not Array array)
        {
            throw new InvalidOperationException($"Cannot index into non-array type: {arrayValue.GetType().Name}");
        }

        ValidateArrayIndex(array, index);

        object element = array.GetValue(index)!;
        LoggerService.Logger.Debug($"[EXEC] Array access: array[{index}] = {element}");

        return element;
    }

    private static void ValidateArrayIndex(Array array, int index)
    {
        if (index < 0)
        {
            throw new IndexOutOfRangeException($"Array index cannot be negative: {index}");
        }

        if (index >= array.Length)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range. Array length is {array.Length}");
        }
    }

    private static int ConvertToInteger(object value)
    {
        if (value == null)
        {
            throw new InvalidOperationException("Array index cannot be null");
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (value is double doubleValue)
        {
            return Math.Abs(doubleValue - Math.Floor(doubleValue)) < double.Epsilon
                ? (int)doubleValue
                : throw new InvalidOperationException($"Array index must be an integer, got: {doubleValue}");
        }

        return value is long longValue
            ? longValue is > int.MaxValue or < int.MinValue
                ? throw new InvalidOperationException($"Array index {longValue} is too large for int")
                : (int)longValue
            : value is string stringValue
            ? int.TryParse(stringValue, out int parsed)
                ? parsed
                : throw new InvalidOperationException($"Cannot convert string '{stringValue}' to integer array index")
            : throw new InvalidOperationException($"Cannot convert type {value.GetType().Name} to integer array index");
    }
}

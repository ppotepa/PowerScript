using Tokenez.Common.Logging;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.Syntax.Tokens.Operators;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Evaluates comparison operations (==, !=, <, <=, >, >=).
/// Single Responsibility: Boolean comparison evaluation
/// </summary>
public class ComparisonEvaluator
{
    private readonly Func<Expression, object> _evaluateExpression;

    public ComparisonEvaluator(Func<Expression, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public bool EvaluateComparison(BinaryExpression expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        object leftValue = _evaluateExpression(expression.Left);
        object rightValue = _evaluateExpression(expression.Right);

        if (expression.Operator is EqualsToken)
        {
            return EvaluateEquals(leftValue, rightValue);
        }

        if (expression.Operator is NotEqualsToken)
        {
            return EvaluateNotEquals(leftValue, rightValue);
        }

        if (expression.Operator is LessThanToken)
        {
            return EvaluateLessThan(leftValue, rightValue);
        }

        return expression.Operator is LessThanOrEqualToken
            ? EvaluateLessThanOrEqual(leftValue, rightValue)
            : expression.Operator is GreaterThanToken
            ? EvaluateGreaterThan(leftValue, rightValue)
            : expression.Operator is GreaterThanOrEqualToken
            ? EvaluateGreaterThanOrEqual(leftValue, rightValue)
            : throw new InvalidOperationException($"Unknown comparison operator: {expression.Operator.GetType().Name}");
    }

    private static bool EvaluateEquals(object left, object right)
    {
        bool result = AreEqual(left, right);
        LoggerService.Logger.Debug($"[EXEC] {left} == {right} = {result}");
        return result;
    }

    private static bool EvaluateNotEquals(object left, object right)
    {
        bool result = !AreEqual(left, right);
        LoggerService.Logger.Debug($"[EXEC] {left} != {right} = {result}");
        return result;
    }

    private static bool EvaluateLessThan(object left, object right)
    {
        double leftNum = ConvertToNumber(left);
        double rightNum = ConvertToNumber(right);
        bool result = leftNum < rightNum;
        LoggerService.Logger.Debug($"[EXEC] {left} < {right} = {result}");
        return result;
    }

    private static bool EvaluateLessThanOrEqual(object left, object right)
    {
        double leftNum = ConvertToNumber(left);
        double rightNum = ConvertToNumber(right);
        bool result = leftNum <= rightNum;
        LoggerService.Logger.Debug($"[EXEC] {left} <= {right} = {result}");
        return result;
    }

    private static bool EvaluateGreaterThan(object left, object right)
    {
        double leftNum = ConvertToNumber(left);
        double rightNum = ConvertToNumber(right);
        bool result = leftNum > rightNum;
        LoggerService.Logger.Debug($"[EXEC] {left} > {right} = {result}");
        return result;
    }

    private static bool EvaluateGreaterThanOrEqual(object left, object right)
    {
        double leftNum = ConvertToNumber(left);
        double rightNum = ConvertToNumber(right);
        bool result = leftNum >= rightNum;
        LoggerService.Logger.Debug($"[EXEC] {left} >= {right} = {result}");
        return result;
    }

    private static bool AreEqual(object left, object right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left == null || right == null)
        {
            return false;
        }

        if (IsNumericType(left) && IsNumericType(right))
        {
            double leftNum = ConvertToNumber(left);
            double rightNum = ConvertToNumber(right);
            return Math.Abs(leftNum - rightNum) < double.Epsilon;
        }

        return left.Equals(right);
    }

    private static bool IsNumericType(object value)
    {
        if (value == null)
        {
            return false;
        }

        Type type = value.GetType();

        if (type == typeof(int))
        {
            return true;
        }

        return type == typeof(long) ? true : type == typeof(double) ? true : type == typeof(float);
    }

    private static double ConvertToNumber(object value)
    {
        if (value == null)
        {
            throw new InvalidOperationException("Cannot convert null to number");
        }

        if (value is double doubleValue)
        {
            return doubleValue;
        }

        if (value is int intValue)
        {
            return intValue;
        }

        if (value is long longValue)
        {
            return longValue;
        }

        if (value is float floatValue)
        {
            return floatValue;
        }

        if (value is string stringValue)
        {
            if (double.TryParse(stringValue, out double parsed))
            {
                return parsed;
            }
        }

        throw new InvalidOperationException($"Cannot convert type {value.GetType().Name} to number");
    }
}

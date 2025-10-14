using Tokenez.Common.Logging;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.Syntax.Tokens.Operators;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Evaluates binary arithmetic operations.
/// Single Responsibility: Binary arithmetic evaluation
/// </summary>
public class BinaryExpressionEvaluator
{
    private readonly Func<Expression, object> _evaluateExpression;

    public BinaryExpressionEvaluator(Func<Expression, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public double EvaluateBinaryExpression(BinaryExpression expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        double leftValue = ConvertToNumber(_evaluateExpression(expression.Left));
        double rightValue = ConvertToNumber(_evaluateExpression(expression.Right));

        if (expression.Operator is PlusToken)
        {
            return EvaluateAddition(leftValue, rightValue);
        }

        if (expression.Operator is MinusToken)
        {
            return EvaluateSubtraction(leftValue, rightValue);
        }

        return expression.Operator is MultiplyToken
            ? EvaluateMultiplication(leftValue, rightValue)
            : expression.Operator is DivideToken
            ? EvaluateDivision(leftValue, rightValue)
            : expression.Operator is ModuloToken
            ? EvaluateModulo(leftValue, rightValue)
            : throw new InvalidOperationException($"Unknown binary operator: {expression.Operator.GetType().Name}");
    }

    private static double EvaluateAddition(double left, double right)
    {
        double result = left + right;
        LoggerService.Logger.Debug($"[EXEC] {left} + {right} = {result}");
        return result;
    }

    private static double EvaluateSubtraction(double left, double right)
    {
        double result = left - right;
        LoggerService.Logger.Debug($"[EXEC] {left} - {right} = {result}");
        return result;
    }

    private static double EvaluateMultiplication(double left, double right)
    {
        double result = left * right;
        LoggerService.Logger.Debug($"[EXEC] {left} * {right} = {result}");
        return result;
    }

    private static double EvaluateDivision(double left, double right)
    {
        if (Math.Abs(right) < double.Epsilon)
        {
            throw new InvalidOperationException("Division by zero");
        }

        double result = left / right;
        LoggerService.Logger.Debug($"[EXEC] {left} / {right} = {result}");
        return result;
    }

    private static double EvaluateModulo(double left, double right)
    {
        if (Math.Abs(right) < double.Epsilon)
        {
            throw new InvalidOperationException("Modulo by zero");
        }

        double result = left % right;
        LoggerService.Logger.Debug($"[EXEC] {left} % {right} = {result}");
        return result;
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

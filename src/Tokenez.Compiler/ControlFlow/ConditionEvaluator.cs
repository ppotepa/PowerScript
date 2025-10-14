using Tokenez.Common.Logging;

namespace Tokenez.Compiler.ControlFlow;

/// <summary>
/// Evaluates conditional expressions and converts to boolean.
/// Single Responsibility: Condition evaluation
/// </summary>
public class ConditionEvaluator
{
    private readonly Func<object, object> _evaluateExpression;

    public ConditionEvaluator(Func<object, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public bool EvaluateCondition(object conditionExpression)
    {
        if (conditionExpression == null)
        {
            throw new ArgumentNullException(nameof(conditionExpression));
        }

        object result = _evaluateExpression(conditionExpression);
        bool boolResult = ConvertToBoolean(result);

        LoggerService.Logger.Debug($"[EVAL] Condition evaluated to: {boolResult}");

        return boolResult;
    }

    private static bool ConvertToBoolean(object value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        return value is double doubleValue
            ? Math.Abs(doubleValue) > double.Epsilon
            : value is string stringValue
            ? bool.TryParse(stringValue, out bool parsed) ? parsed : !string.IsNullOrWhiteSpace(stringValue)
            : true;
    }
}

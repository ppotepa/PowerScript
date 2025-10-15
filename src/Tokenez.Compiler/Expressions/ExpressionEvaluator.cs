using Tokenez.Common.Logging;
using Tokenez.Compiler.Core.Variables;
using Tokenez.Core.AST.Expressions;
using Tokenez.Core.Syntax.Tokens.Operators;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Main orchestrator for expression evaluation.
/// Single Responsibility: Expression routing and coordination
/// </summary>
public class ExpressionEvaluator
{
    private readonly VariableRegistry _variableRegistry;
    private readonly LiteralProcessor _literalProcessor;
    private readonly IdentifierProcessor _identifierProcessor;
    private readonly BinaryExpressionEvaluator _binaryEvaluator;
    private readonly ComparisonEvaluator _comparisonEvaluator;
    private readonly ArrayAccessEvaluator _arrayAccessEvaluator;
    private readonly Func<FunctionCallExpression, object> _functionCallEvaluator;

    public ExpressionEvaluator(
        VariableRegistry variableRegistry,
        Func<FunctionCallExpression, object> functionCallEvaluator)
    {
        if (variableRegistry == null)
        {
            throw new ArgumentNullException(nameof(variableRegistry));
        }

        _variableRegistry = variableRegistry;
        _functionCallEvaluator = functionCallEvaluator ?? throw new ArgumentNullException(nameof(functionCallEvaluator));

        _literalProcessor = new LiteralProcessor();
        _identifierProcessor = new IdentifierProcessor(variableRegistry, this);
        _binaryEvaluator = new BinaryExpressionEvaluator(Evaluate);
        _comparisonEvaluator = new ComparisonEvaluator(Evaluate);
        _arrayAccessEvaluator = new ArrayAccessEvaluator(Evaluate);
        _functionCallEvaluator = functionCallEvaluator;
    }

    public object Evaluate(Expression expression)
    {
        if (expression == null)
        {
            throw new ArgumentNullException(nameof(expression));
        }

        LoggerService.Logger.Debug($"[EVAL] Evaluating expression: {expression.GetType().Name}");

        if (expression is LiteralExpression literalExpression)
        {
            return _literalProcessor.ProcessLiteral(literalExpression);
        }

        if (expression is StringLiteralExpression stringLiteralExpression)
        {
            return EvaluateStringLiteral(stringLiteralExpression);
        }

        if (expression is IdentifierExpression identifierExpression)
        {
            return _identifierProcessor.GetIdentifierValue(identifierExpression);
        }

        if (expression is BinaryExpression binaryExpression)
        {
            return EvaluateBinaryExpression(binaryExpression);
        }

        if (expression is LogicalExpression logicalExpression)
        {
            return EvaluateLogicalExpression(logicalExpression);
        }

        if (expression is ArrayLiteralExpression arrayLiteralExpression)
        {
            return EvaluateArrayLiteral(arrayLiteralExpression);
        }

        if (expression is IndexExpression indexExpression)
        {
            return EvaluateIndexExpression(indexExpression);
        }

        if (expression is FunctionCallExpression functionCallExpression)
        {
            return _functionCallEvaluator(functionCallExpression);
        }

        if (expression is TemplateStringExpression templateStringExpression)
        {
            return EvaluateTemplateString(templateStringExpression);
        }

        throw new InvalidOperationException($"Unknown expression type: {expression.GetType().Name}");
    }

    private object EvaluateTemplateString(TemplateStringExpression templateExpression)
    {
        if (templateExpression == null || templateExpression.Template == null)
        {
            return string.Empty;
        }

        var result = new System.Text.StringBuilder();
        
        foreach (var part in templateExpression.Template.Parts)
        {
            if (!part.IsLiteral)
            {
                // This is a variable reference
                try
                {
                    var value = _variableRegistry.GetVariable(part.Text);
                    result.Append(value?.ToString() ?? string.Empty);
                }
                catch (InvalidOperationException ex)
                {
                    throw new InvalidOperationException($"Variable '@{part.Text}' not found in template string", ex);
                }
            }
            else
            {
                // Literal text part
                result.Append(part.Text);
            }
        }

        return result.ToString();
    }

    private object EvaluateStringLiteral(StringLiteralExpression stringExpression)
    {
        // Template strings not currently supported in AST
        return _literalProcessor.ProcessStringLiteral(stringExpression);
    }

    private object EvaluateBinaryExpression(BinaryExpression binaryExpression)
    {
        if (IsComparisonOperator(binaryExpression.Operator))
        {
            return _comparisonEvaluator.EvaluateComparison(binaryExpression);
        }

        if (IsArithmeticOperator(binaryExpression.Operator))
        {
            return _binaryEvaluator.EvaluateBinaryExpression(binaryExpression);
        }

        throw new InvalidOperationException($"Unknown binary operator type: {binaryExpression.Operator.GetType().Name}");
    }

    private static bool IsComparisonOperator(object operatorToken)
    {
        if (operatorToken is EqualsToken)
        {
            return true;
        }

        if (operatorToken is EqualsEqualsToken)
        {
            return true;
        }

        if (operatorToken is NotEqualsToken)
        {
            return true;
        }

        if (operatorToken is LessThanToken)
        {
            return true;
        }

        if (operatorToken is LessThanOrEqualToken)
        {
            return true;
        }

        if (operatorToken is GreaterThanToken)
        {
            return true;
        }

        if (operatorToken is GreaterThanOrEqualToken)
        {
            return true;
        }

        return false;
    }

    private static bool IsArithmeticOperator(object operatorToken)
    {
        if (operatorToken is PlusToken)
        {
            return true;
        }

        if (operatorToken is MinusToken)
        {
            return true;
        }

        if (operatorToken is MultiplyToken)
        {
            return true;
        }

        if (operatorToken is DivideToken)
        {
            return true;
        }

        if (operatorToken is ModuloToken)
        {
            return true;
        }

        return false;
    }

    private object EvaluateLogicalExpression(LogicalExpression logicalExpression)
    {
        object leftValue = Evaluate(logicalExpression.Left);
        object rightValue = Evaluate(logicalExpression.Right);

        bool leftBool = ConvertToBool(leftValue);
        bool rightBool = ConvertToBool(rightValue);

        string operatorText = logicalExpression.Operator.KeyWord;

        if (operatorText == "AND")
        {
            return leftBool && rightBool;
        }

        if (operatorText == "OR")
        {
            return leftBool || rightBool;
        }

        throw new InvalidOperationException($"Unknown logical operator: {operatorText}");
    }

    private object[] EvaluateArrayLiteral(ArrayLiteralExpression arrayLiteral)
    {
        if (arrayLiteral == null)
        {
            throw new ArgumentNullException(nameof(arrayLiteral));
        }

        if (arrayLiteral.Elements == null || arrayLiteral.Elements.Count == 0)
        {
            return Array.Empty<object>();
        }

        object[] result = new object[arrayLiteral.Elements.Count];

        for (int i = 0; i < arrayLiteral.Elements.Count; i++)
        {
            result[i] = Evaluate(arrayLiteral.Elements[i]);
        }

        return result;
    }

    private object EvaluateIndexExpression(IndexExpression indexExpression)
    {
        if (indexExpression == null)
        {
            throw new ArgumentNullException(nameof(indexExpression));
        }

        // Evaluate the array expression (could be IdentifierExpression or another IndexExpression for 2D arrays)
        object arrayValue = Evaluate(indexExpression.ArrayExpression);

        if (arrayValue is not object[] array)
        {
            throw new InvalidOperationException($"Expression does not evaluate to an array. Actual type: {arrayValue?.GetType().Name ?? "null"}");
        }

        object indexValue = Evaluate(indexExpression.Index);
        int index = Convert.ToInt32(indexValue);

        if (index < 0 || index >= array.Length)
        {
            throw new IndexOutOfRangeException($"Array index {index} is out of range. Array length: {array.Length}");
        }

        return array[index];
    }

    private static bool ConvertToBool(object value)
    {
        if (value is bool boolValue)
        {
            return boolValue;
        }

        if (value == null)
        {
            return false;
        }

        if (value is int intValue)
        {
            return intValue != 0;
        }

        if (value is double doubleValue)
        {
            return Math.Abs(doubleValue) > 0.0001;
        }

        if (value is string stringValue)
        {
            return !string.IsNullOrEmpty(stringValue);
        }

        return true;
    }

    public object GetArrayElement(object arrayValue, Expression indexExpression)
    {
        return _arrayAccessEvaluator.GetArrayElement(arrayValue, indexExpression);
    }
}

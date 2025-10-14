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
    private readonly LiteralProcessor _literalProcessor;
    private readonly IdentifierProcessor _identifierProcessor;
    private readonly BinaryExpressionEvaluator _binaryEvaluator;
    private readonly ComparisonEvaluator _comparisonEvaluator;
    private readonly ArrayAccessEvaluator _arrayAccessEvaluator;
    private readonly TemplateStringEvaluator _templateStringEvaluator;
    private readonly Func<FunctionCallExpression, object> _functionCallEvaluator;

    public ExpressionEvaluator(
        VariableRegistry variableRegistry,
        Func<FunctionCallExpression, object> functionCallEvaluator)
    {
        if (variableRegistry == null)
        {
            throw new ArgumentNullException(nameof(variableRegistry));
        }

        _functionCallEvaluator = functionCallEvaluator ?? throw new ArgumentNullException(nameof(functionCallEvaluator));

        _literalProcessor = new LiteralProcessor();
        _identifierProcessor = new IdentifierProcessor(variableRegistry, this);
        _binaryEvaluator = new BinaryExpressionEvaluator(Evaluate);
        _comparisonEvaluator = new ComparisonEvaluator(Evaluate);
        _arrayAccessEvaluator = new ArrayAccessEvaluator(Evaluate);
        _templateStringEvaluator = new TemplateStringEvaluator(Evaluate);
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

        return expression is IdentifierExpression identifierExpression
            ? _identifierProcessor.GetIdentifierValue(identifierExpression)
            : expression is BinaryExpression binaryExpression
            ? EvaluateBinaryExpression(binaryExpression)
            : expression is FunctionCallExpression functionCallExpression
            ? _functionCallEvaluator(functionCallExpression)
            : throw new InvalidOperationException($"Unknown expression type: {expression.GetType().Name}");
    }

    private object EvaluateStringLiteral(StringLiteralExpression stringExpression)
    {
        return stringExpression.TemplateSegments != null && stringExpression.TemplateSegments.Count > 0
            ? _templateStringEvaluator.EvaluateTemplateString(stringExpression)
            : _literalProcessor.ProcessStringLiteral(stringExpression);
    }

    private object EvaluateBinaryExpression(BinaryExpression binaryExpression)
    {
        return IsComparisonOperator(binaryExpression.Operator)
            ? _comparisonEvaluator.EvaluateComparison(binaryExpression)
            : IsArithmeticOperator(binaryExpression.Operator)
            ? (object)_binaryEvaluator.EvaluateBinaryExpression(binaryExpression)
            : throw new InvalidOperationException($"Unknown binary operator type: {binaryExpression.Operator.GetType().Name}");
    }

    private static bool IsComparisonOperator(object operatorToken)
    {
        if (operatorToken is EqualsToken)
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

        return operatorToken is LessThanOrEqualToken
            ? true
            : operatorToken is GreaterThanToken ? true : operatorToken is GreaterThanOrEqualToken;
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

        return operatorToken is MultiplyToken ? true : operatorToken is DivideToken ? true : operatorToken is ModuloToken;
    }

    public object GetArrayElement(object arrayValue, Expression indexExpression)
    {
        return _arrayAccessEvaluator.GetArrayElement(arrayValue, indexExpression);
    }
}

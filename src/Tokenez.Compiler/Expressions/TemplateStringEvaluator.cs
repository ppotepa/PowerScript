using System.Text;
using Tokenez.Common.Logging;
using Tokenez.Core.AST.Expressions;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Evaluates template strings with variable interpolation.
/// Single Responsibility: Template string processing
/// </summary>
public class TemplateStringEvaluator
{
    private readonly Func<Expression, object> _evaluateExpression;

    public TemplateStringEvaluator(Func<Expression, object> evaluateExpression)
    {
        _evaluateExpression = evaluateExpression ?? throw new ArgumentNullException(nameof(evaluateExpression));
    }

    public string EvaluateTemplateString(StringLiteralExpression templateExpression)
    {
        if (templateExpression == null)
        {
            throw new ArgumentNullException(nameof(templateExpression));
        }

        if (templateExpression.TemplateSegments == null || templateExpression.TemplateSegments.Count == 0)
        {
            return RemoveQuotes(templateExpression.Value);
        }

        StringBuilder result = new();

        foreach (var segment in templateExpression.TemplateSegments)
        {
            if (segment is StringLiteralExpression stringSegment)
            {
                string literalValue = RemoveQuotes(stringSegment.Value);
                result.Append(literalValue);
                continue;
            }

            if (segment is IdentifierExpression identifierSegment)
            {
                object variableValue = _evaluateExpression(identifierSegment);
                string stringValue = ConvertToString(variableValue);
                result.Append(stringValue);
                continue;
            }

            throw new InvalidOperationException($"Unsupported template segment type: {segment.GetType().Name}");
        }

        string finalResult = result.ToString();
        LoggerService.Logger.Debug($"[EXEC] Template string evaluated: \"{finalResult}\"");

        return finalResult;
    }

    private static string ConvertToString(object value)
    {
        return value == null ? string.Empty : value is string stringValue ? stringValue : value.ToString() ?? string.Empty;
    }

    private static string RemoveQuotes(string value)
    {
        return string.IsNullOrEmpty(value) ? value : value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;
    }
}

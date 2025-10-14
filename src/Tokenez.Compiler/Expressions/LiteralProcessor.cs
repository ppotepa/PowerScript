using Tokenez.Common.Logging;
using Tokenez.Core.AST.Expressions;

namespace Tokenez.Compiler.Expressions;

/// <summary>
/// Processes literal expressions (numbers, strings).
/// Single Responsibility: Literal value extraction
/// </summary>
public class LiteralProcessor
{
    public object ProcessLiteral(LiteralExpression literal)
    {
        if (literal == null)
        {
            throw new ArgumentNullException(nameof(literal));
        }

        if (literal.Value == null)
        {
            throw new InvalidOperationException("Literal value cannot be null.");
        }

        string valueText = literal.Value.RawToken?.Text ?? string.Empty;

        if (double.TryParse(valueText, out double numericValue))
        {
            return numericValue;
        }

        LoggerService.Logger.Warning($"Could not parse literal '{valueText}' as number, returning as string.");
        return valueText;
    }

    public object ProcessStringLiteral(StringLiteralExpression stringLiteral)
    {
        if (stringLiteral == null)
        {
            throw new ArgumentNullException(nameof(stringLiteral));
        }

        if (stringLiteral.Value == null)
        {
            throw new InvalidOperationException("String literal value cannot be null.");
        }

        string text = stringLiteral.Value.RawToken?.Text ?? string.Empty;

        // Remove quotes if present
        return text.StartsWith('"') && text.EndsWith('"') && text.Length >= 2 ? text[1..^1] : text;
    }
}

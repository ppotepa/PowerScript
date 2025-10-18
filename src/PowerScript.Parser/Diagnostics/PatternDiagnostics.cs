using System.Text;
using PowerScript.Common.Logging;
using PowerScript.Core.Syntax;
using PowerScript.Core.Syntax.Tokens.Base;

namespace PowerScript.Parser.Diagnostics;

/// <summary>
/// Provides diagnostic tools for debugging pattern matching issues.
/// </summary>
public static class PatternDiagnostics
{
    /// <summary>
    /// Diagnoses why a pattern failed to match against a token stream.
    /// </summary>
    public static string DiagnosePatternMatchFailure(Token startToken, SyntaxTransformation pattern)
    {
        var diagnostics = new StringBuilder();
        diagnostics.AppendLine($"=== Pattern Match Failure Diagnostics ===");
        diagnostics.AppendLine($"Pattern: {pattern.Pattern}");
        diagnostics.AppendLine($"Transformation: {pattern.Transformation}");
        diagnostics.AppendLine();
        diagnostics.AppendLine("Token Stream:");

        var current = startToken;
        int tokenIndex = 0;
        while (current != null && tokenIndex < 20)
        {
            var tokenType = current.GetType().Name;
            var tokenText = current.RawToken?.Text ?? "null";
            var originalText = current.RawToken?.OriginalText ?? "null";

            diagnostics.AppendLine($"  [{tokenIndex}] {tokenType,-25} Text='{tokenText}' Original='{originalText}'");

            current = current.Next;
            tokenIndex++;
        }

        diagnostics.AppendLine();
        diagnostics.AppendLine("Pattern Parts:");
        var patternParts = pattern.Pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < patternParts.Length; i++)
        {
            diagnostics.AppendLine($"  [{i}] {patternParts[i]}");
        }

        return diagnostics.ToString();
    }

    /// <summary>
    /// Finds the closest matching pattern for a given token sequence.
    /// </summary>
    public static SyntaxTransformation? FindClosestPatternMatch(Token startToken, IEnumerable<SyntaxTransformation> patterns)
    {
        var firstWord = startToken.RawToken?.Text?.ToUpperInvariant();
        if (string.IsNullOrEmpty(firstWord))
        {
            return null;
        }

        SyntaxTransformation? closestMatch = null;
        int bestScore = int.MaxValue;

        foreach (var pattern in patterns)
        {
            var patternFirstWord = GetFirstWord(pattern.Pattern).ToUpperInvariant();

            if (patternFirstWord == firstWord)
            {
                var score = CalculateMatchScore(startToken, pattern);
                if (score < bestScore)
                {
                    bestScore = score;
                    closestMatch = pattern;
                }
            }
        }

        return closestMatch;
    }

    /// <summary>
    /// Logs detailed information about registered patterns for debugging.
    /// </summary>
    public static void LogPatternRegistry(IEnumerable<SyntaxTransformation> patterns)
    {
        LoggerService.Logger.Info("=== Registered Pattern Summary ===");

        var groupedByFirstWord = patterns
            .GroupBy(p => GetFirstWord(p.Pattern).ToUpperInvariant())
            .OrderBy(g => g.Key);

        foreach (var group in groupedByFirstWord)
        {
            LoggerService.Logger.Info($"\nPatterns starting with '{group.Key}':");
            foreach (var pattern in group)
            {
                LoggerService.Logger.Info($"  - {pattern.Pattern} => {pattern.Transformation}");
            }
        }
    }

    /// <summary>
    /// Validates that token types match pattern expectations.
    /// </summary>
    public static List<string> ValidateTokenSequence(Token startToken, string expectedPattern)
    {
        var errors = new List<string>();
        var patternParts = expectedPattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = startToken;

        for (int i = 0; i < patternParts.Length && current != null; i++)
        {
            var part = patternParts[i];

            if (part.StartsWith("$"))
            {
                // Parameter - check if token is valid value/identifier
                if (!IsValidParameterToken(current))
                {
                    errors.Add($"Position {i}: Expected parameter but got {current.GetType().Name}");
                }
            }
            else
            {
                // Keyword - check exact match
                var expectedText = part.ToUpperInvariant();
                var actualText = current.RawToken?.Text?.ToUpperInvariant() ?? "";

                if (expectedText != actualText)
                {
                    errors.Add($"Position {i}: Expected '{expectedText}' but got '{actualText}'");
                }
            }

            current = current.Next;
        }

        return errors;
    }

    private static string GetFirstWord(string pattern)
    {
        var spaceIndex = pattern.IndexOf(' ');
        return spaceIndex > 0 ? pattern.Substring(0, spaceIndex) : pattern;
    }

    private static int CalculateMatchScore(Token startToken, SyntaxTransformation pattern)
    {
        // Simple scoring: count differences between tokens and pattern
        int score = 0;
        var patternParts = pattern.Pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var current = startToken;

        for (int i = 0; i < patternParts.Length && current != null; i++)
        {
            var part = patternParts[i];

            if (!part.StartsWith("$"))
            {
                var expectedText = part.ToUpperInvariant();
                var actualText = current.RawToken?.Text?.ToUpperInvariant() ?? "";

                if (expectedText != actualText)
                {
                    score++;
                }
            }

            current = current.Next;
        }

        return score;
    }

    private static bool IsValidParameterToken(Token token)
    {
        // Check if token can be used as a parameter
        var typeName = token.GetType().Name;
        return typeName.Contains("Identifier") ||
               typeName.Contains("Value") ||
               typeName.Contains("String") ||
               typeName.Contains("Decimal");
    }
}

using PowerScript.Common.Logging;
using PowerScript.Core.Syntax;

namespace PowerScript.Parser.Validators;

/// <summary>
/// Validates custom syntax patterns before registration to prevent conflicts and errors.
/// </summary>
public static class PatternValidator
{
    private static readonly HashSet<string> ReservedKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "FUNCTION", "RETURN", "IF", "ELSE", "WHILE", "CYCLE", "AS", "IN", "TO",
        "PRINT", "VAR", "FLEX", "INT", "STRING", "FLOAT", "BOOL", "ARRAY",
        "TRUE", "FALSE", "AND", "OR", "NOT", "LINK", "SYNTAX", "FROM", "OF",
        "ELEMENTS", "WHERE", "WITH"
    };

    /// <summary>
    /// Validates a pattern before registration.
    /// </summary>
    /// <param name="pattern">The pattern to validate</param>
    /// <param name="transformation">The transformation to validate</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public static List<string> ValidatePattern(string pattern, string transformation)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(pattern))
        {
            errors.Add("Pattern cannot be empty");
            return errors;
        }

        if (string.IsNullOrWhiteSpace(transformation))
        {
            errors.Add("Transformation cannot be empty");
            return errors;
        }

        // Check for keyword conflicts
        var firstWord = GetFirstWord(pattern);
        if (ReservedKeywords.Contains(firstWord))
        {
            errors.Add($"Pattern starts with reserved keyword '{firstWord}'");
        }

        // Check transformation is valid function call format
        if (!IsValidTransformation(transformation))
        {
            errors.Add($"Transformation '{transformation}' is not a valid function call format");
        }

        // Check parameter consistency
        var patternParams = ExtractParameters(pattern);
        var transformParams = ExtractParameters(transformation);

        foreach (var param in transformParams)
        {
            if (!patternParams.Contains(param))
            {
                errors.Add($"Transformation references parameter '{param}' not defined in pattern");
            }
        }

        return errors;
    }

    /// <summary>
    /// Checks if a pattern would conflict with existing patterns.
    /// </summary>
    public static List<string> CheckForConflicts(string pattern, IEnumerable<SyntaxTransformation> existingPatterns)
    {
        var conflicts = new List<string>();
        var normalizedPattern = NormalizePattern(pattern);

        foreach (var existing in existingPatterns)
        {
            var existingNormalized = NormalizePattern(existing.Pattern);

            if (existingNormalized == normalizedPattern)
            {
                conflicts.Add($"Pattern conflicts with existing: '{existing.Pattern}'");
            }
            else if (IsAmbiguous(pattern, existing.Pattern))
            {
                conflicts.Add($"Pattern may be ambiguous with: '{existing.Pattern}'");
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Validates pattern matches against type constraints.
    /// </summary>
    public static bool ValidateTypeConstraints(string pattern)
    {
        // Check if pattern has type annotations
        if (!pattern.Contains(":"))
        {
            return true; // No type constraints
        }

        // Extract type annotations and validate them
        var parts = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            if (part.Contains(":"))
            {
                var typePart = part.Split(':')[1].TrimEnd('?');
                if (!IsValidType(typePart))
                {
                    LoggerService.Logger.Warning($"Invalid type constraint: {typePart}");
                    return false;
                }
            }
        }

        return true;
    }

    private static string GetFirstWord(string pattern)
    {
        var spaceIndex = pattern.IndexOf(' ');
        return spaceIndex > 0 ? pattern.Substring(0, spaceIndex) : pattern;
    }

    private static bool IsValidTransformation(string transformation)
    {
        // Check basic function call format: FUNCTION_NAME(...)
        if (!transformation.Contains("(") || !transformation.Contains(")"))
        {
            return false;
        }

        var parenIndex = transformation.IndexOf('(');
        var functionName = transformation.Substring(0, parenIndex).Trim();

        // Function name should be uppercase identifier
        return !string.IsNullOrWhiteSpace(functionName) &&
               functionName.All(c => char.IsLetterOrDigit(c) || c == '_');
    }

    private static HashSet<string> ExtractParameters(string text)
    {
        var parameters = new HashSet<string>();
        var parts = text.Split(new[] { ' ', '(', ')', ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (part.StartsWith("$"))
            {
                // Remove type annotation if present
                var paramName = part.Contains(":") ? part.Split(':')[0] : part;
                parameters.Add(paramName);
            }
        }

        return parameters;
    }

    private static string NormalizePattern(string pattern)
    {
        // Normalize by removing type annotations and converting to uppercase
        var normalized = pattern.ToUpperInvariant();
        // Remove type constraints for comparison
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @":\w+\??", "");
        return normalized;
    }

    private static bool IsAmbiguous(string pattern1, string pattern2)
    {
        // Simple ambiguity check: same first word and similar structure
        var word1 = GetFirstWord(pattern1).ToUpperInvariant();
        var word2 = GetFirstWord(pattern2).ToUpperInvariant();

        if (word1 != word2)
        {
            return false;
        }

        // Check if parameter counts differ significantly
        var params1 = ExtractParameters(pattern1);
        var params2 = ExtractParameters(pattern2);

        return Math.Abs(params1.Count - params2.Count) <= 1;
    }

    private static bool IsValidType(string typeName)
    {
        var validTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "INT", "STRING", "FLOAT", "BOOL", "ARRAY", "FLEX", "OBJECT"
        };

        return validTypes.Contains(typeName);
    }
}

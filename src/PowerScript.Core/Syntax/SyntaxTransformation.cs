using PowerScript.Core.AST.Expressions;

namespace PowerScript.Core.Syntax;

/// <summary>
/// Represents a custom syntax transformation rule loaded from a .psx file.
/// </summary>
public class SyntaxTransformation
{
    /// <summary>
    /// The type of syntax transformation.
    /// </summary>
    public SyntaxType Type { get; set; }

    /// <summary>
    /// The pattern to match (e.g., "$target::Method($args)" or "FILTER $array WHERE $condition").
    /// </summary>
    public string Pattern { get; set; } = string.Empty;

    /// <summary>
    /// The transformation template (e.g., "FUNCTION_NAME($target, $args)").
    /// </summary>
    public string Transformation { get; set; } = string.Empty;

    /// <summary>
    /// The variables captured from the pattern (e.g., ["target", "args"]).
    /// </summary>
    public List<string> CapturedVariables { get; set; } = new();

    /// <summary>
    /// Type constraints for variables (e.g., { "count": "INT", "array": "ARRAY" }).
    /// Key is the variable name, value is the expected type.
    /// </summary>
    public Dictionary<string, string> TypeConstraints { get; set; } = new();

    /// <summary>
    /// Priority for pattern matching (higher priority patterns are tried first).
    /// Default is 0. Used to resolve ambiguous patterns.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// The source file where this transformation was defined.
    /// </summary>
    public string SourceFile { get; set; } = string.Empty;
}

/// <summary>
/// The type of syntax transformation.
/// </summary>
public enum SyntaxType
{
    /// <summary>
    /// Operator-based transformation using :: (e.g., array::Sort()).
    /// </summary>
    Operator,

    /// <summary>
    /// Pattern-based transformation (e.g., FILTER $array WHERE $condition).
    /// </summary>
    Pattern
}

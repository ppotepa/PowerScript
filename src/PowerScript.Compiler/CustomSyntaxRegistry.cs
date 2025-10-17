using PowerScript.Compiler.Models;

namespace PowerScript.Compiler;

/// <summary>
/// Registry for custom syntax transformations loaded from .psx files.
/// Manages syntax patterns and their transformations.
/// </summary>
public class CustomSyntaxRegistry
{
    private static readonly Lazy<CustomSyntaxRegistry> _instance = new(() => new CustomSyntaxRegistry());

    /// <summary>
    /// Gets the singleton instance of the registry.
    /// </summary>
    public static CustomSyntaxRegistry Instance => _instance.Value;

    private readonly Dictionary<string, SyntaxTransformation> _operatorTransformations = new();
    private readonly List<SyntaxTransformation> _patternTransformations = new();
    private readonly HashSet<string> _loadedFiles = new();

    private CustomSyntaxRegistry()
    {
    }

    /// <summary>
    /// Registers a syntax transformation.
    /// </summary>
    public void Register(SyntaxTransformation transformation)
    {
        if (transformation.Type == SyntaxType.Operator)
        {
            // For operator syntax, key is the method name
            var methodName = ExtractMethodName(transformation.Pattern);
            if (!string.IsNullOrEmpty(methodName))
            {
                _operatorTransformations[methodName.ToUpperInvariant()] = transformation;
            }
        }
        else
        {
            // Pattern transformations are checked in order
            _patternTransformations.Add(transformation);
        }
    }

    /// <summary>
    /// Tries to get an operator transformation by method name.
    /// </summary>
    public bool TryGetOperatorTransformation(string methodName, out SyntaxTransformation? transformation)
    {
        return _operatorTransformations.TryGetValue(methodName.ToUpperInvariant(), out transformation);
    }

    /// <summary>
    /// Gets all pattern transformations for matching.
    /// </summary>
    public IReadOnlyList<SyntaxTransformation> GetPatternTransformations()
    {
        return _patternTransformations.AsReadOnly();
    }

    /// <summary>
    /// Checks if a .psx file has already been loaded.
    /// </summary>
    public bool IsFileLoaded(string filePath)
    {
        return _loadedFiles.Contains(filePath.ToUpperInvariant());
    }

    /// <summary>
    /// Marks a .psx file as loaded.
    /// </summary>
    public void MarkFileLoaded(string filePath)
    {
        _loadedFiles.Add(filePath.ToUpperInvariant());
    }

    /// <summary>
    /// Clears all registered transformations (for testing).
    /// </summary>
    public void Clear()
    {
        _operatorTransformations.Clear();
        _patternTransformations.Clear();
        _loadedFiles.Clear();
    }

    /// <summary>
    /// Extracts the method name from an operator pattern like "$target::MethodName($args)".
    /// </summary>
    private string ExtractMethodName(string pattern)
    {
        // Find :: in the pattern
        var colonIndex = pattern.IndexOf("::");
        if (colonIndex < 0) return string.Empty;

        // Find the method name after ::
        var startIndex = colonIndex + 2;
        var endIndex = pattern.IndexOf('(', startIndex);
        if (endIndex < 0) endIndex = pattern.Length;

        return pattern.Substring(startIndex, endIndex - startIndex).Trim();
    }
}

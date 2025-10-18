namespace PowerScript.Core.Syntax;

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

    /// <summary>
    /// Map of keywords to pattern information they belong to.
    /// Key: keyword text (uppercase), Value: list of patterns containing this keyword
    /// </summary>
    private readonly Dictionary<string, List<PatternKeywordInfo>> _patternKeywords = new();

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

            // Extract and register keywords from the pattern
            RegisterPatternKeywords(transformation);
        }
    }

    /// <summary>
    /// Extracts keywords from a pattern and registers them.
    /// Pattern: "TAKE $count FROM $array" → registers TAKE at position 0, FROM at position 2
    /// Also extracts type constraints like $count:INT
    /// </summary>
    private void RegisterPatternKeywords(SyntaxTransformation transformation)
    {
        string pattern = transformation.Pattern;
        string patternId = GeneratePatternId(pattern);

        string[] words = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            if (word.StartsWith("$")) // Variable placeholder
            {
                // Extract type constraint if present: $variable:TYPE
                var parts = word.TrimStart('$').Split(':', 2);
                string varName = parts[0];

                if (parts.Length == 2)
                {
                    string typeName = parts[1].ToUpperInvariant();
                    transformation.TypeConstraints[varName] = typeName;
                }

                // Track captured variables
                if (!transformation.CapturedVariables.Contains(varName))
                {
                    transformation.CapturedVariables.Add(varName);
                }
            }
            else // Keyword
            {
                string keywordUpper = word.ToUpperInvariant();

                if (!_patternKeywords.ContainsKey(keywordUpper))
                {
                    _patternKeywords[keywordUpper] = new List<PatternKeywordInfo>();
                }

                _patternKeywords[keywordUpper].Add(new PatternKeywordInfo
                {
                    PatternId = patternId,
                    PatternText = pattern,
                    PositionInPattern = i,
                    Transformation = transformation
                });
            }
        }
    }

    /// <summary>
    /// Generates a unique ID for a pattern based on its keywords.
    /// </summary>
    private string GeneratePatternId(string pattern)
    {
        // Extract non-variable words and join them
        var keywords = pattern.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => !w.StartsWith("$"))
            .Select(w => w.ToUpperInvariant());

        return string.Join("_", keywords);
    }

    /// <summary>
    /// Checks if a word is a registered pattern keyword.
    /// </summary>
    public bool IsPatternKeyword(string word, out List<PatternKeywordInfo>? keywordInfo)
    {
        string wordUpper = word.ToUpperInvariant();
        return _patternKeywords.TryGetValue(wordUpper, out keywordInfo);
    }

    /// <summary>
    /// Checks if a word can start a pattern (is at position 0 of at least one pattern).
    /// </summary>
    public bool CanStartPattern(string word)
    {
        string wordUpper = word.ToUpperInvariant();
        if (_patternKeywords.TryGetValue(wordUpper, out var keywordInfoList))
        {
            // Check if any of the patterns have this keyword at position 0
            return keywordInfoList.Any(info => info.PositionInPattern == 0);
        }
        return false;
    }

    /// <summary>
    /// Gets pattern information by ID.
    /// </summary>
    public SyntaxTransformation? GetPatternById(string patternId)
    {
        // Search through pattern transformations for matching ID
        foreach (var pattern in _patternTransformations)
        {
            if (GeneratePatternId(pattern.Pattern) == patternId)
            {
                return pattern;
            }
        }
        return null;
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
        _patternKeywords.Clear();
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

/// <summary>
/// Information about a keyword in a pattern syntax.
/// </summary>
public class PatternKeywordInfo
{
    /// <summary>
    /// Unique ID for the pattern (e.g., "TAKE_FROM")
    /// </summary>
    public string PatternId { get; set; } = "";

    /// <summary>
    /// Full pattern text (e.g., "TAKE $count FROM $array")
    /// </summary>
    public string PatternText { get; set; } = "";

    /// <summary>
    /// Position of this keyword in the pattern (0-based)
    /// </summary>
    public int PositionInPattern { get; set; }

    /// <summary>
    /// The transformation this pattern maps to
    /// </summary>
    public SyntaxTransformation Transformation { get; set; } = null!;
}

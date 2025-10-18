using PowerScript.Core.Syntax;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace PowerScript.Compiler;

/// <summary>
/// Loads and parses .psx files containing custom syntax definitions.
/// Registers transformations with the CustomSyntaxRegistry.
/// </summary>
public class PsxFileLoader
{
    private static readonly Regex SyntaxPattern = new Regex(
        @"^\s*SYNTAX\s+(.+?)\s*=>\s*(.+?)\s*$",
        RegexOptions.Compiled | RegexOptions.Multiline
    );

    /// <summary>
    /// Loads a .psx file and registers all syntax transformations.
    /// </summary>
    public static void LoadFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Syntax file not found: {filePath}");
        }

        var registry = CustomSyntaxRegistry.Instance;

        // Check if already loaded
        if (registry.IsFileLoaded(filePath))
        {
            return;
        }

        var content = File.ReadAllText(filePath);
        var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Skip comments and empty lines
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith("//"))
            {
                continue;
            }

            // Parse SYNTAX statements
            var match = SyntaxPattern.Match(line);
            if (match.Success)
            {
                var pattern = match.Groups[1].Value.Trim();
                var transformation = match.Groups[2].Value.Trim();

                // Determine if it's operator or pattern syntax
                var syntaxType = pattern.Contains("::") ? SyntaxType.Operator : SyntaxType.Pattern;

                var syntaxTransformation = new SyntaxTransformation
                {
                    Type = syntaxType,
                    Pattern = pattern,
                    Transformation = transformation,
                    CapturedVariables = ExtractVariables(pattern),
                    SourceFile = filePath
                };

                registry.Register(syntaxTransformation);
            }
        }

        registry.MarkFileLoaded(filePath);
    }

    /// <summary>
    /// Loads all .psx files from a directory.
    /// </summary>
    public static void LoadDirectory(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return; // Silently skip if directory doesn't exist
        }

        var psxFiles = Directory.GetFiles(directoryPath, "*.psx", SearchOption.AllDirectories);
        foreach (var file in psxFiles)
        {
            try
            {
                LoadFile(file);
            }
            catch (Exception ex)
            {
                // Log but don't crash if a single file fails
                Console.WriteLine($"Warning: Failed to load syntax file {file}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Extracts variable names from a pattern (e.g., $array, $count, $item).
    /// </summary>
    private static List<string> ExtractVariables(string pattern)
    {
        var variables = new List<string>();
        var varPattern = new Regex(@"\$(\w+)");
        var matches = varPattern.Matches(pattern);

        foreach (Match match in matches)
        {
            var varName = match.Groups[1].Value;
            if (!variables.Contains(varName))
            {
                variables.Add(varName);
            }
        }

        return variables;
    }

    /// <summary>
    /// Loads the standard library syntax files from stdlib/syntax directory.
    /// </summary>
    public static void LoadStandardSyntax(string stdlibPath)
    {
        var syntaxPath = Path.Combine(stdlibPath, "syntax");
        LoadDirectory(syntaxPath);
    }
}

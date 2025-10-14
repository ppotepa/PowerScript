using Tokenez.Core.AST;
using Tokenez.Core.Syntax.Tokens.Base;
using Tokenez.Core.Syntax.Tokens.Identifiers;
using Tokenez.Core.Syntax.Tokens.Keywords;
using Tokenez.Core.Syntax.Tokens.Operators;

namespace Tokenez.Compiler.Diagnostics;

/// <summary>
///     Analyzes the token tree and scope structure to provide intelligent diagnostics.
///     Checks for missing LINK statements, unused imports, and other code quality issues.
/// </summary>
public class DiagnosticAnalyzer
{
    private readonly List<Diagnostic> _diagnostics = [];
    private readonly List<string> _linkedLibraries = [];

    /// <summary>
    ///     Analyzes the scope tree and returns a list of diagnostics.
    /// </summary>
    public List<Diagnostic> Analyze(Scope rootScope, Token[] allTokens)
    {
        _diagnostics.Clear();
        _linkedLibraries.Clear();

        // First pass: collect all LINK statements
        CollectLinkedLibraries(allTokens);

        // Second pass: analyze namespace/library usage
        AnalyzeLibraryUsage(allTokens);

        // Third pass: analyze scope-specific issues
        AnalyzeScope(rootScope);

        List<Diagnostic> list = [];
        foreach (Diagnostic diagnostic in _diagnostics)
        {
            list.Add(diagnostic);
        }

        return list;
    }

    /// <summary>
    ///     Collects all libraries that have been linked via LINK statements.
    /// </summary>
    private void CollectLinkedLibraries(Token[] tokens)
    {
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] is LinkKeywordToken && i + 1 < tokens.Length)
            {
                Token nextToken = tokens[i + 1];
                if (nextToken is IdentifierToken identifierToken)
                {
                    string libraryName = identifierToken.RawToken.Text.ToUpperInvariant();
                    _linkedLibraries.Add(libraryName);

                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Info,
                        "PS001",
                        $"Library '{libraryName}' has been linked",
                        $"LINK {libraryName}"
                    ));
                }
            }
        }
    }

    /// <summary>
    ///     Analyzes namespace operator usage and suggests missing LINK statements.
    /// </summary>
    private void AnalyzeLibraryUsage(Token[] tokens)
    {
        for (int i = 0; i < tokens.Length - 2; i++)
        {
            // Look for patterns like: Identifier :: Identifier (e.g., System :: Console)
            if (tokens[i] is IdentifierToken libToken &&
                tokens[i + 1] is NamespaceOperatorToken &&
                tokens[i + 2] is IdentifierToken)
            {
                string libraryName = libToken.RawToken.Text.ToUpperInvariant();
                string usage = $"{libToken.RawToken.Text}::{tokens[i + 2].RawToken.Text}";

                // Check if the library is linked
                if (!_linkedLibraries.Contains(libraryName))
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Warning,
                        "PS002",
                        $"Using '{usage}' but library '{libraryName}' is not linked",
                        usage,
                        $"Add 'LINK {libraryName}' at the top of your script"
                    ));
                }
                else
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Info,
                        "PS003",
                        $"Successfully using linked library: {usage}"
                    ));
                }
            }

            // Look for patterns that suggest full namespace paths (e.g., System.Console.WriteLine)
            if (tokens[i] is IdentifierToken nameToken &&
                tokens[i + 1] is DotToken &&
                tokens[i + 2] is IdentifierToken)
            {
                string fullPath = $"{nameToken.RawToken.Text}.{tokens[i + 2].RawToken.Text}";
                string rootNamespace = nameToken.RawToken.Text.ToUpperInvariant();

                // Check for well-known .NET namespaces
                if (IsWellKnownNamespace(rootNamespace) && !_linkedLibraries.Contains("SYSTEM"))
                {
                    _diagnostics.Add(new Diagnostic(
                        DiagnosticSeverity.Suggestion,
                        "PS004",
                        $"Using .NET namespace '{fullPath}'",
                        fullPath,
                        $"Consider adding 'LINK System' to enable full .NET integration, or use 'System::{tokens[i + 2].RawToken.Text}' syntax"
                    ));
                }
            }
        }
    }

    /// <summary>
    ///     Analyzes scope structure for potential issues.
    /// </summary>
    private void AnalyzeScope(Scope scope)
    {
        // Check for unused linked libraries
        CheckUnusedLibraries();

        // Analyze function declarations
        foreach (Declaration declaration in scope.Decarations.Values)
        {
            if (declaration is FunctionDeclaration funcDecl)
            {
                AnalyzeFunctionDeclaration(funcDecl);
            }
        }

        // Recursively analyze nested scopes (if any)
        // Note: In current implementation, we mainly have function scopes
    }

    /// <summary>
    ///     Analyzes individual function declarations.
    /// </summary>
    private void AnalyzeFunctionDeclaration(FunctionDeclaration declaration)
    {
        string functionName = declaration.Identifier.RawToken.Text;

        // Check if function has return type but no return statement
        if (declaration.ReturnType != null && declaration.Scope != null && !declaration.Scope.HasReturn)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Warning,
                "PS005",
                $"Function '{functionName}' declares return type '{declaration.ReturnType.RawToken.Text}' but has no RETURN statement",
                $"FUNCTION {functionName}",
                "Add a RETURN statement or remove the return type declaration"
            ));
        }

        // Check for functions with no return type but has return statement
        if (declaration.ReturnType == null && declaration.Scope != null && declaration.Scope.HasReturn)
        {
            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Suggestion,
                "PS006",
                $"Function '{functionName}' has RETURN statement but no declared return type",
                $"FUNCTION {functionName}",
                $"Consider adding a return type: FUNCTION {functionName}(...)[TYPE]"
            ));
        }
    }

    /// <summary>
    ///     Checks for linked libraries that are never used.
    /// </summary>
    private void CheckUnusedLibraries()
    {
        // This is a simple implementation - in a full system, we'd track actual usage
        foreach (string library in _linkedLibraries)
        {
            if (library is "SYSTEM" or "MATH" or "IO")
            {
                // Well-known libraries are commonly used, so we won't warn about them
                continue;
            }

            _diagnostics.Add(new Diagnostic(
                DiagnosticSeverity.Info,
                "PS007",
                $"Library '{library}' is linked and available for use"
            ));
        }
    }

    /// <summary>
    ///     Checks if a namespace is a well-known .NET namespace.
    /// </summary>
    private static bool IsWellKnownNamespace(string namespaceName)
    {
        string[] wellKnownNamespaces =
        [
            "SYSTEM", "CONSOLE", "MATH", "STRING", "DATETIME",
            "IO", "TEXT", "LINQ", "COLLECTIONS", "THREADING"
        ];

        return wellKnownNamespaces.Contains(namespaceName);
    }
}
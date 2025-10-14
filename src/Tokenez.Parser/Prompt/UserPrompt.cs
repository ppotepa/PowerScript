using System.Text;
using Tokenez.Core.Syntax.Tokens.Raw;

namespace Tokenez.Parser.Prompt;

/// <summary>
///     Represents user input code that will be tokenized.
///     User code defines functions directly at root scope, which serves as a standard library.
/// </summary>
public class UserPrompt(string prompt)
{
    private RawToken[]? _rawTokens;

    /// <summary>
    ///     The original user code
    /// </summary>
    public string Prompt { get; } = prompt;

    /// <summary>
    ///     The code to process (same as Prompt - no wrapping)
    /// </summary>
    public string WrappedPrompt { get; } = prompt; // No wrapping needed - root scope is the standard library

    /// <summary>
    ///     Lazily tokenizes the wrapped prompt into raw tokens.
    ///     Separates all operators, delimiters, and keywords into individual tokens.
    ///     Preserves string literals as single tokens.
    /// </summary>
    public RawToken[] RawTokens
    {
        get
        {
            // First, strip single-line comments (//)
            string text = StripComments(WrappedPrompt).Trim();
            List<string> tokens = [];
            int i = 0;

            while (i < text.Length)
            {
                // Handle template strings (backticks)
                if (text[i] == '`')
                {
                    int startQuote = i;
                    i++; // Skip opening backtick
                    while (i < text.Length && text[i] != '`')
                    {
                        i++;
                    }

                    if (i < text.Length)
                    {
                        i++; // Include closing backtick
                        tokens.Add(text[startQuote..i]);
                    }
                }
                // Handle string literals
                else if (text[i] == '"')
                {
                    int startQuote = i;
                    i++; // Skip opening quote
                    while (i < text.Length && text[i] != '"')
                    {
                        i++;
                    }

                    if (i < text.Length)
                    {
                        i++; // Include closing quote
                        tokens.Add(text[startQuote..i]);
                    }
                }
                // Handle other characters
                else if (char.IsWhiteSpace(text[i]))
                {
                    i++;
                }
                else
                {
                    int start = i;
                    while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '"')
                    {
                        i++;
                    }

                    tokens.Add(text[start..i]);
                }
            }

            // Now add spaces around delimiters and operators (but not inside string literals or template strings)
            List<string> processedTokens = [];
            foreach (string token in tokens)
            {
                if (token.StartsWith('"') || token.StartsWith('`'))
                {
                    // Keep string literals and template strings as-is
                    processedTokens.Add(token);
                }
                else
                {
                    // Add spaces around delimiters and operators
                    // Handle multi-character operators FIRST before single-character ones
                    string processed = token
                        .Replace("::", " :: ") // Namespace operator
                        .Replace("==", " == ") // Equality comparison
                        .Replace("!=", " != ") // Not equal comparison
                        .Replace(">=", " >= ") // Greater than or equal
                        .Replace("<=", " <= ") // Less than or equal
                        .Replace(".", " . ") // Dot operator
                        .Replace("#", " # ") // C# .NET shorthand
                        .Replace("{", " { ")
                        .Replace("}", " } ")
                        .Replace(")", " ) ")
                        .Replace("(", " ( ")
                        .Replace("[", " [ ")
                        .Replace("]", " ] ")
                        .Replace(",", " , ")
                        .Replace("+", " + ")
                        .Replace("-", " - ")
                        .Replace("*", " * ")
                        .Replace("/", " / ")
                        .Replace("%", " % "); // Modulo operator

                    // NOTE: Single "=" and "<", ">" are NOT replaced here
                    // because they would break multi-character operators like "==", "<=", ">="
                    // The tokenizer will handle them correctly without extra spaces
                    processedTokens.AddRange(processed.Split([' '], StringSplitOptions.RemoveEmptyEntries));
                }
            }

            // Create raw tokens only once (lazy initialization)
            _rawTokens ??= processedTokens.Select(RawToken.Create).ToArray();

            return _rawTokens;
        }
    }

    /// <summary>
    ///     Strips single-line comments (//) from the code while preserving string literals.
    /// </summary>
    private string StripComments(string code)
    {
        StringBuilder result = new();
        int i = 0;

        while (i < code.Length)
        {
            // Preserve string literals
            if (code[i] == '"')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '"')
                {
                    if (code[i] == '\\' && i + 1 < code.Length)
                    {
                        result.Append(code[i]); // Append escape character
                        i++;
                        result.Append(code[i]); // Append escaped character
                        i++;
                    }
                    else
                    {
                        result.Append(code[i]);
                        i++;
                    }
                }

                if (i < code.Length)
                {
                    result.Append(code[i]); // Append closing quote
                    i++;
                }
            }
            // Preserve template strings (backticks)
            else if (code[i] == '`')
            {
                result.Append(code[i]);
                i++;
                while (i < code.Length && code[i] != '`')
                {
                    result.Append(code[i]);
                    i++;
                }

                if (i < code.Length)
                {
                    result.Append(code[i]); // Append closing backtick
                    i++;
                }
            }
            // Strip single-line comments
            else if (i + 1 < code.Length && code[i] == '/' && code[i + 1] == '/')
            {
                // Skip until end of line
                while (i < code.Length && code[i] != '\n' && code[i] != '\r')
                {
                    i++;
                }

                // Keep the newline character
                if (i < code.Length && (code[i] == '\n' || code[i] == '\r'))
                {
                    result.Append(code[i]);
                    i++;
                    // Handle \r\n
                    if (i < code.Length && code[i - 1] == '\r' && code[i] == '\n')
                    {
                        result.Append(code[i]);
                        i++;
                    }
                }
            }
            else
            {
                result.Append(code[i]);
                i++;
            }
        }

        return result.ToString();
    }
}
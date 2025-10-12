using ppotepa.tokenez.DotNet;
using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Raw;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    ///     Represents the complete token tree built from user input.
    ///     Handles tokenization, linking, and scope building.
    /// </summary>
    public partial class TokenTree
    {
        /// <summary>
        ///     Static mapping of exact text strings to their corresponding token types.
        ///     This is checked first before the reflection-based TokenTypes dictionary.
        ///     Used for operators, delimiters, and core keywords that need exact matching.
        /// </summary>
        private static readonly Dictionary<string, Type> _map = new()
        {
            { "LINK", typeof(LinkKeywordToken) }, // Library/file import keyword
            { "FUNCTION", typeof(FunctionToken) }, // Function declaration keyword
            { "RETURN", typeof(ReturnKeywordToken) }, // Return statement keyword
            { "PRINT", typeof(PrintKeywordToken) }, // Print statement keyword
            { "EXECUTE", typeof(ExecuteKeywordToken) }, // Execute script file keyword
            { "NET", typeof(NetKeywordToken) }, // .NET access keyword
            { "#", typeof(NetKeywordToken) }, // .NET access shorthand (# for C#)
            { "VAR", typeof(VarKeywordToken) }, // Variable declaration keyword
            { "FLEX", typeof(FlexKeywordToken) }, // Dynamic variable declaration keyword
            { "CYCLE", typeof(CycleKeywordToken) }, // Loop keyword (foreach equivalent)
            { "IN", typeof(InKeywordToken) }, // In keyword for loops
            { "AS", typeof(AsKeywordToken) }, // As keyword for renaming
            { "IF", typeof(IfKeywordToken) }, // Conditional statement keyword
            { "ELSE", typeof(ElseKeywordToken) }, // Else block keyword
            { "AND", typeof(AndKeywordToken) }, // Logical AND operator
            { "OR", typeof(OrKeywordToken) }, // Logical OR operator
            { "INT", typeof(IntToken) }, // Integer type keyword
            { "PREC", typeof(PrecToken) }, // Precision/float type keyword
            { "CHAR", typeof(CharToken) }, // Character type keyword
            { "STRING", typeof(StringToken) }, // String type keyword (CHAR CHAIN)
            { "CHAIN", typeof(ChainToken) }, // Collection/array type modifier
            { "{", typeof(ScopeStartToken) }, // Scope/block start
            { "}", typeof(ScopeEndToken) }, // Scope/block end
            { "[", typeof(BracketOpen) }, // Return type bracket open
            { "]", typeof(BracketClosed) }, // Return type bracket close
            { ",", typeof(CommaToken) }, // Comma delimiter
            { "+", typeof(PlusToken) }, // Addition operator
            { "-", typeof(MinusToken) }, // Subtraction operator
            { "*", typeof(MultiplyToken) }, // Multiplication operator
            { "/", typeof(DivideToken) }, // Division operator
            { "=", typeof(EqualsToken) }, // Assignment operator
            { ">", typeof(GreaterThanToken) }, // Greater than comparison
            { "<", typeof(LessThanToken) }, // Less than comparison
            { ">=", typeof(GreaterThanOrEqualToken) }, // Greater than or equal
            { "<=", typeof(LessThanOrEqualToken) }, // Less than or equal
            { "==", typeof(EqualsEqualsToken) }, // Equality comparison
            { "!=", typeof(NotEqualsToken) }, // Not equal comparison
            { "::", typeof(NamespaceOperatorToken) }, // Namespace operator
            { ".", typeof(DotToken) } // Dot operator for member access
        };

        private Dictionary<string, Type>? _tokenTypes;

        /// <summary>The root scope containing all top-level declarations</summary>
        public Scope? RootScope { get; private set; }

        /// <summary>All tokens in the tree</summary>
        public Token[]? Tokens { get; private set; }

        /// <summary>The .NET linker for resolving types from linked namespaces</summary>
        public DotNetLinker DotNetLinker => _dotNetLinker;

        /// <summary>
        ///     Dynamically discovered token types through reflection.
        ///     Finds all Token subclasses and maps them by their name (without 'Token' suffix).
        ///     Excludes operator tokens to prevent conflicts with function names.
        ///     This allows adding new token types without modifying the core mapping logic.
        /// </summary>
        private Dictionary<string, Type> TokenTypes
        {
            get
            {
                // Use reflection to find all classes that inherit from Token
                _tokenTypes
                    ??= AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                        .Where(type => type.IsSubclassOf(typeof(Token)))
                        // Exclude operator and value tokens to prevent conflicts (they should be in _map instead)
                        .Where(type => type.Namespace?.Contains("Operators") != true)
                        .Where(type => type.Namespace?.Contains("Values") != true)
                        .ToDictionary(
                            // Map by class name without 'Token' suffix (e.g., 'FunctionToken' -> 'FUNCTION')
                            type => type.Name.ToUpperInvariant().Replace("TOKEN", ""),
                            type => type,
                            StringComparer.OrdinalIgnoreCase
                        );

                return _tokenTypes;
            }
        }

        /// <summary>
        ///     Creates the token tree from a user prompt.
        ///     Steps: 1) Convert raw tokens to typed tokens, 2) Link tokens bidirectionally, 3) Build scope hierarchy
        /// </summary>
        public TokenTree Create(UserPrompt prompt)
        {
            Console.WriteLine("\n=== Building Token Tree ===");
            Console.WriteLine($"Processing: '{prompt.WrappedPrompt}'\n");

            // Step 1: Convert raw text tokens into strongly-typed token objects
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            Console.WriteLine($"Tokens created: {tokens.Length}");

            // Step 2: Link all tokens with Previous/Next references for easy navigation
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Console.WriteLine("Tokens linked");

            // Step 3: Build the scope hierarchy starting from root scope
            var scope = CreateScope(tokens[0], new Scope("ROOT"));
            Console.WriteLine("Scope creation complete\n");

            // Store the results
            Tokens = tokens;
            RootScope = scope;

            return this;
        }

        /// <summary>
        ///     Visualizes the entire token tree structure.
        ///     Shows all scopes, declarations, statements, and their relationships.
        /// </summary>
        public void Visualize()
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║       TOKEN TREE VISUALIZATION         ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            Console.ResetColor();

            if (RootScope != null)
            {
                RootScope.Visualize();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No root scope found!");
                Console.ResetColor();
            }

            Console.WriteLine();
        }

        /// <summary>
        ///     Links tokens bidirectionally to form a doubly-linked list.
        ///     This allows processors to easily navigate forward (Next) and backward (Prev).
        /// </summary>
        private Token Link(Token token, int index, Token[] tokens)
        {
            // First token: only set Next
            if (index is 0)
            {
                token.Next = tokens[index + 1];
            }
            // Middle tokens: set both Prev and Next
            else if (index < tokens.Length - 1)
            {
                token.Next = tokens[index + 1];
                token.Prev = tokens[index - 1];
            }
            // Last token: only set Prev
            else if (tokens.Length - 1 == index)
            {
                token.Prev = tokens[index - 1];
            }

            return token;
        }

        /// <summary>
        ///     Converts a raw text token into a strongly-typed Token object.
        ///     Uses a three-tier lookup strategy:
        ///     1) Check static _map for exact matches (operators, keywords, delimiters)
        ///     2) Check dynamic TokenTypes for reflection-discovered types
        ///     3) Fall back to switch statement for special cases and identifiers
        /// </summary>
        private Token ToToken(RawToken rawToken, int index)
        {
            Type? targetType = null;

            // First priority: Check static map for exact string matches
            if (_map.ContainsKey(rawToken.Text))
                targetType = _map[rawToken.Text];
            // Second priority: Check dynamically discovered token types
            else if (TokenTypes.ContainsKey(rawToken.Text))
                targetType = TokenTypes[rawToken.Text];
            // Third priority: Handle special cases and default to identifier
            else
                switch (rawToken.Text.Trim())
                {
                    case "(":
                        targetType = typeof(ParenthesisOpen);
                        break;
                    case ")":
                        targetType = typeof(ParenthesisClosed);
                        break;
                    case ",":
                        targetType = typeof(CommaSeparatorToken);
                        break;
                    case "{":
                        targetType = typeof(ScopeStartToken);
                        break;
                    case "}":
                        targetType = typeof(ScopeEndToken);
                        break;
                    default:
                        // Check if it's a template string (starts and ends with backticks)
                        if (IsTemplateString(rawToken.Text))
                            targetType = typeof(TemplateStringToken);
                        // Check if it's a string literal (starts and ends with quotes)
                        else if (IsStringLiteral(rawToken.Text))
                            targetType = typeof(StringLiteralToken);
                        // Check if it's a numeric literal
                        else if (IsNumericLiteral(rawToken.Text))
                            targetType = typeof(ValueToken);
                        else
                            // If no match found, treat as identifier (variable/function name)
                            targetType = typeof(IdentifierToken);
                        break;
                }

            // Create instance of the appropriate token type
            var token = Activator.CreateInstance(targetType, rawToken) as Token;
            if (token == null)
            {
                throw new InvalidOperationException($"Failed to create token of type {targetType.Name}");
            }
            Console.WriteLine($"\t[{index}] {rawToken.Text} → {targetType.Name}");
            return token;
        }

        /// <summary>
        ///     Checks if a string represents a string literal (enclosed in quotes).
        /// </summary>
        private static bool IsStringLiteral(string text)
        {
            return text.StartsWith("\"") && text.EndsWith("\"") && text.Length >= 2;
        }

        /// <summary>
        ///     Checks if a string represents a template string (enclosed in backticks).
        /// </summary>
        private static bool IsTemplateString(string text)
        {
            return text.StartsWith("`") && text.EndsWith("`") && text.Length >= 2;
        }

        /// <summary>
        ///     Checks if a string represents a numeric literal (integer or decimal).
        /// </summary>
        private static bool IsNumericLiteral(string text)
        {
            return double.TryParse(text, out _);
        }
    }
}
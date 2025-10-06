using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Raw;
using System.Collections;
using System.Data;

namespace ppotepa.tokenez.Tree
{
    /// <summary>
    /// Represents the complete token tree built from user input.
    /// Handles tokenization, linking, and scope building.
    /// </summary>
    public partial class TokenTree
    {
        /// <summary>
        /// Static mapping of exact text strings to their corresponding token types.
        /// This is checked first before the reflection-based TokenTypes dictionary.
        /// Used for operators, delimiters, and core keywords that need exact matching.
        /// </summary>
        private static Dictionary<string, Type> _map = new()
        {
            { "FUNCTION", typeof(Tokens.Keywords.FunctionToken) },   // Function declaration keyword
            { "RETURN", typeof(Tokens.Keywords.ReturnKeywordToken) }, // Return statement keyword
            { "INT", typeof(IntToken) },                              // Integer type keyword
            { "{", typeof(Tokens.Scoping.ScopeStartToken) },         // Scope/block start
            { "}", typeof(Tokens.Scoping.ScopeEndToken) },           // Scope/block end
            { "+", typeof(PlusToken) },                              // Addition operator
            { "-", typeof(MinusToken) },                             // Subtraction operator
            { "*", typeof(MultiplyToken) },                          // Multiplication operator
            { "/", typeof(DivideToken) }                             // Division operator
        };

        private Dictionary<string, Type> _tokenTypes = default;

        /// <summary>
        /// Dynamically discovered token types through reflection.
        /// Finds all Token subclasses and maps them by their name (without 'Token' suffix).
        /// This allows adding new token types without modifying the core mapping logic.
        /// </summary>
        private Dictionary<string, Type> TokenTypes
        {
            get
            {
                if (_tokenTypes is null)
                {
                    // Use reflection to find all classes that inherit from Token
                    _tokenTypes
                        = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                            .Where(type => type.IsSubclassOf(typeof(Token)))
                            .ToDictionary(
                                    // Map by class name without 'Token' suffix (e.g., 'FunctionToken' -> 'FUNCTION')
                                    type => type.Name.ToUpperInvariant().Replace("TOKEN", ""),
                                    type => type,
                                    StringComparer.OrdinalIgnoreCase
                            );
                }

                return _tokenTypes;
            }
        }
        /// <summary>
        /// Creates the token tree from a user prompt.
        /// Steps: 1) Convert raw tokens to typed tokens, 2) Link tokens bidirectionally, 3) Build scope hierarchy
        /// </summary>
        public TokenTree Create(UserPrompt prompt)
        {
            Console.WriteLine($"\n=== Building Token Tree ===");
            Console.WriteLine($"Processing: '{prompt.WrappedPrompt}'\n");

            // Step 1: Convert raw text tokens into strongly-typed token objects
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            Console.WriteLine($"Tokens created: {tokens.Length}");

            // Step 2: Link all tokens with Previous/Next references for easy navigation
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Console.WriteLine($"Tokens linked");

            // Step 3: Build the scope hierarchy starting from root scope
            Scope scope = CreateScope(tokens[0], new Scope("ROOT"));
            Console.WriteLine($"Scope creation complete\n");

            return null; // TODO: Return the built tree
        }

        /// <summary>
        /// Links tokens bidirectionally to form a doubly-linked list.
        /// This allows processors to easily navigate forward (Next) and backward (Prev).
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
        /// Converts a raw text token into a strongly-typed Token object.
        /// Uses a three-tier lookup strategy:
        /// 1) Check static _map for exact matches (operators, keywords, delimiters)
        /// 2) Check dynamic TokenTypes for reflection-discovered types
        /// 3) Fall back to switch statement for special cases and identifiers
        /// </summary>
        private Token ToToken(RawToken rawToken, int index)
        {
            Type targetType = default;

            // First priority: Check static map for exact string matches
            if (_map.ContainsKey(rawToken.Text))
            {
                targetType = _map[rawToken.Text];
            }
            // Second priority: Check dynamically discovered token types
            else if (TokenTypes.ContainsKey(rawToken.Text))
            {
                targetType = TokenTypes[rawToken.Text];
            }
            // Third priority: Handle special cases and default to identifier
            else
            {
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
                    default:
                        // If no match found, treat as identifier (variable/function name)
                        targetType = typeof(IdentifierToken);
                        break;
                }
            }

            // Create instance of the appropriate token type
            var token = Activator.CreateInstance(targetType, [rawToken]) as Token;
            Console.WriteLine($"\t[{index}] {rawToken.Text} → {targetType.Name}");
            return token;
        }
    }
}
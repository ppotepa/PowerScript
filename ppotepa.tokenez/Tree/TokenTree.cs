using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Raw;
using System.Collections;

namespace ppotepa.tokenez.Tree
{

    public partial class TokenTree
    {
        private static Dictionary<string, Type> _map = new()
        {
            {
                "INT", typeof(IntToken)
            }
        };

        private Dictionary<string, Type> _tokenTypes = default;

        private Dictionary<string, Type> TokenTypes
        {
            get
            {
                if (_tokenTypes is null)
                {
                    _tokenTypes
                        = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
                            .Where(type => type.IsSubclassOf(typeof(Token)))
                            .ToDictionary(
                                    type => type.Name.ToUpperInvariant().Replace("TOKEN", ""),
                                    type => type,
                                    StringComparer.OrdinalIgnoreCase
                            );
                }

                return _tokenTypes;
            }
        }
        public TokenTree Create(UserPrompt prompt)
        {
            Console.WriteLine("\n" + new string('=', 80));
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("╔═══════════════════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                       TOKENIZATION PROCESS STARTED                           ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
            Console.WriteLine($"Prompt: \"{prompt.Prompt}\"");
            Console.WriteLine($"Raw Tokens Count: {prompt.RawTokens.Length}");
            Console.WriteLine(new string('-', 80));
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[PHASE 1] Converting Raw Tokens to Typed Tokens...");
            Console.ResetColor();
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Created {tokens.Length} typed tokens");
            Console.ResetColor();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[PHASE 2] Linking Tokens (Prev/Next references)...");
            Console.ResetColor();
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Linked {tokens.Length} tokens");
            Console.ResetColor();
            
            Console.WriteLine("\n[Token Chain]");
            for (int i = 0; i < tokens.Length; i++)
            {
                Console.Write($"  [{i}] {tokens[i].GetType().Name}");
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($" ('{tokens[i].RawToken?.Text}')");
                Console.ResetColor();
                if (i < tokens.Length - 1) Console.Write(" → ");
                else Console.WriteLine();
                if ((i + 1) % 3 == 0 && i < tokens.Length - 1) Console.Write("\n      ");
            }
            Console.WriteLine();
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n[PHASE 3] Creating Scope Tree...");
            Console.ResetColor();
            Scope scope = CreateScope(tokens[0], new Scope("ROOT"));
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Scope tree created");
            Console.ResetColor();

            Console.WriteLine(new string('=', 80) + "\n");
            return null;
        }

        private Token Link(Token token, int index, Token[] tokens)
        {
            string linkInfo = "";
            
            if (index is 0)
            {
                token.Next = tokens[index + 1];
                linkInfo = $"First token: Next → {token.Next.GetType().Name}";
            }
            else if (index < tokens.Length - 1)
            {
                token.Next = tokens[index + 1];
                token.Prev = tokens[index - 1];
                linkInfo = $"Middle token: {token.Prev.GetType().Name} ← Current → {token.Next.GetType().Name}";
            }
            else if (tokens.Length - 1 == index)
            {
                token.Prev = tokens[index - 1];
                linkInfo = $"Last token: Prev ← {token.Prev.GetType().Name}";
            }
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"  Linking [{index}] {token.GetType().Name}: {linkInfo}");
            Console.ResetColor();

            return token;
        }
        private Token ToToken(RawToken rawToken, int index)
        {
            Type targetType = default;
            string resolutionMethod = "";

            if (TokenTypes.ContainsKey(rawToken.Text))
            {
                targetType = TokenTypes[rawToken.Text];
                resolutionMethod = "Token Dictionary";
            }
            else
            {
                switch(rawToken.Text.Trim())
                {
                    case "(":
                        targetType = typeof(ParenthesisOpen);
                        resolutionMethod = "Switch Case (Delimiter)";
                        break;
                    case ")":
                        targetType = typeof(ParenthesisClosed);
                        resolutionMethod = "Switch Case (Delimiter)";
                        break;
                }

                if (targetType == null)
                {
                    //keywords                
                    targetType = typeof(IdentifierToken);
                    resolutionMethod = "Default (Identifier)";
                }
            }

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"  [{index}] '{rawToken.Text}'");
            Console.ResetColor();
            Console.Write($" → {targetType.Name}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($" ({resolutionMethod})");
            Console.ResetColor();

            return Activator.CreateInstance(targetType, [rawToken]) as Token;
        }
    }
}
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
            Console.WriteLine($"Creating TokenTree from prompt: '{prompt.Prompt}'");
            
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            Console.WriteLine($"Tokens created: {tokens.Length}");
            
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Console.WriteLine($"Tokens linked");
            
            Scope scope = CreateScope(tokens[0], new Scope("ROOT"));
            Console.WriteLine($"Scope creation complete\n");

            return null;
        }

        private Token Link(Token token, int index, Token[] tokens)
        {
            if (index is 0)
            {
                token.Next = tokens[index + 1];
            }
            else if (index < tokens.Length - 1)
            {
                token.Next = tokens[index + 1];
                token.Prev = tokens[index - 1];
            }
            else if (tokens.Length - 1 == index)
            {
                token.Prev = tokens[index - 1];
            }

            return token;
        }
        private Token ToToken(RawToken rawToken, int index)
        {
            Type targetType = default;

            if (TokenTypes.ContainsKey(rawToken.Text))
            {
                targetType = TokenTypes[rawToken.Text];
            }
            else
            {
                switch(rawToken.Text.Trim())
                {
                    case "(":
                        targetType = typeof(ParenthesisOpen);
                        break;
                    case ")":
                        targetType = typeof(ParenthesisClosed);
                        break;
                }

                //keywords                
                targetType = typeof(IdentifierToken);
            }

            var token = Activator.CreateInstance(targetType, [rawToken]) as Token;
            Console.WriteLine($"\t[{index}] {rawToken.Text} → {targetType.Name}");
            return token;
        }
    }
}
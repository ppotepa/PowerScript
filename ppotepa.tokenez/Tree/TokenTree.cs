using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Raw;
using System.Collections;

namespace ppotepa.tokenez.Tree
{

    public partial class TokenTree
    {
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

        private static Dictionary<string, Type> _map = new()
        {
            { 
                "INT", typeof(IntToken) 
            }
        };

        public TokenTree Create(UserPrompt prompt)
        {
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Scope scope = CreateScope(tokens[0], new Scope("ROOT"));

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
                //keywords
                
                targetType = typeof(IdentifierToken);
            }


            return Activator.CreateInstance(targetType, [rawToken]) as Token;
        }
    }
}
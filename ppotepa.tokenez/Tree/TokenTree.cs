using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens;
using ppotepa.tokenez.Tree.Tokens.Raw;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.WebSockets;
using System.Security.Cryptography;

namespace ppotepa.tokenez.Tree
{
    public class Scope
    {
        public List<string> Decarations = new();

        private Scope _outerScope;

        public IEnumerator Enumerator { get; set; }

        public Scope InnerScope { get; set; }

        public string Name { get; }

        public Scope OuterScope
        {
            get => _outerScope;
            set => _outerScope = value;
        }
        public Token Token { get; set; }
        // Add a method to get a ref to the backing field for OuterScope
        public ref Scope GetOuterScopeRef()
        {
            return ref _outerScope;
        }
    }

    public class TokenTree
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

        public TokenTree Create(UserPrompt prompt)
        {
            Token[] tokens = [.. new RawTokenCollection(prompt.RawTokens).Select(ToToken)];
            tokens = [.. tokens.Select((element, index) => Link(element, index, tokens)).ToArray()];
            Scope scope = CreateScope(tokens[0], new Scope());

            return null;
        }

        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0)
        {
            do
            {
                bool hasExpectations = currentToken.Expectations.Any();

                if (hasExpectations)
                {
                    if (currentToken is FunctionToken)
                    {
                        Type targetExpectation = null;
                        foreach (Type expectation in currentToken.Expectations)
                        {
                            if (currentToken.Next.GetType() == expectation)
                            {
                                targetExpectation = expectation;
                                currentToken = currentToken.Next;                               
                            }
                            else
                            {
                                throw new InvalidOperationException($"Invalid operation. {expectation.Name} expected");
                            }
                        }
                        //if (targetExpectation is null)
                        //{
                        //    var expected = string.Join('|', currentToken.Expectations.Select(type => type.Name));
                        //    throw new InvalidOperationException($"Invalid operation. {expected} expected");
                        //}
                    }
                }
                else
                {

                }

                currentToken = currentToken.Next;
            }
            while (currentToken is not null);

            return default;
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
                targetType = typeof(IdentifierToken);
            }


            return Activator.CreateInstance(targetType, [rawToken]) as Token;
        }
    }
}
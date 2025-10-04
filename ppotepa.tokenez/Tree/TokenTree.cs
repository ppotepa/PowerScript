using ppotepa.tokenez.Prompt;
using ppotepa.tokenez.Tree.Tokens;
using ppotepa.tokenez.Tree.Tokens.Raw;
using System.Collections;

namespace ppotepa.tokenez.Tree
{
    public class Scope
    {
        public List<string> Decarations = new();
        public Scope InnerScope { get; set; }
        public string Name { get; }
        public Scope OuterScope { get; set; }
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
            var iterator = tokens.GetEnumerator();
            Scope scope = CreateScope(new Scope(), iterator, 0);

            return null;
        }

        public Scope CreateScope(Scope scope, IEnumerator enumerator, int depth = 0)
        {
            Console.WriteLine($"Creating scope:DEPTH{depth}");

            while (enumerator.MoveNext())
            {
                Console.WriteLine($"Current Expects Token {enumerator.Current}");
                Token current = enumerator.Current as Token;

                if (current.Expects.Length != 0)
                {    
                    Console.WriteLine($"Current Expects Token {current.Expects[0]}");
                    var innerScope = new Scope();
                    innerScope.OuterScope = scope;
                    return CreateScope(innerScope, current.Expects.GetEnumerator(), depth + 1);
                }
                else
                {
                    if (depth is 0)
                    {
                        return scope;
                    }

                    if (depth > 0)
                    {
                        return CreateScope(scope.OuterScope, enumerator, depth - 1);
                    }

                }
            }

            return scope;
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
                targetType = typeof(StringValueToken);
            }


            return Activator.CreateInstance(targetType, [rawToken]) as Token;
        }
    }
}
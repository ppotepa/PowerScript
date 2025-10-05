using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree
{

    public partial class TokenTree
    {

        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0)
        {
            Console.WriteLine($"\n{new string(' ', depth * 2)}┌─ CreateScope called");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"{new string(' ', depth * 2)}│  Scope: {scope.ScopeName}");
            Console.WriteLine($"{new string(' ', depth * 2)}│  Depth: {depth}, Iteration: {iteration}");
            Console.WriteLine($"{new string(' ', depth * 2)}│  Starting Token: {currentToken.GetType().Name} ('{currentToken.RawToken?.Text}')");
            Console.ResetColor();
            
            int tokenCount = 0;
            do
            {
                tokenCount++;
                string indent = new string(' ', depth * 2);
                
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{indent}├─ Processing Token #{tokenCount}");
                Console.ResetColor();
                Console.WriteLine($"{indent}│  Type: {currentToken.GetType().Name}");
                Console.WriteLine($"{indent}│  Text: '{currentToken.RawToken?.Text}'");
                
                bool hasExpectations = currentToken.Expectations.Length != 0;
                Console.Write($"{indent}│  Expectations: ");
                if (hasExpectations)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{currentToken.Expectations.Length} type(s) - [{string.Join(", ", currentToken.Expectations.Select(t => t.Name))}]");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine("None");
                    Console.ResetColor();
                }

                if (hasExpectations)
                {
                    if (currentToken is FunctionToken)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"{indent}│  ► FUNCTION TOKEN DETECTED");
                        Console.ResetColor();
                        
                        Scope functionScope = new() { OuterScope = scope };
                        Console.WriteLine($"{indent}│    Created function scope with outer scope: {scope.ScopeName}");

                        if (currentToken.Next is IdentifierToken)
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine($"{indent}│    ► Function name found: '{currentToken.Next.RawToken.Text}'");
                            Console.ResetColor();
                            
                            Declaration declaration = new FunctionDeclaration($"{scope}.fn_{currentToken.Next.RawToken.Text}");
                            scope.Decarations.Add(currentToken.Next.RawToken.Text, declaration);
                            Console.WriteLine($"{indent}│      Added declaration: {declaration.Identifier}");
                            
                            currentToken = currentToken.Next;
                            Console.WriteLine($"{indent}│      Advanced to next token: {currentToken.GetType().Name}");

                            if (currentToken.Next is ParenthesisOpen)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"{indent}│    ► Opening parenthesis detected - parameters expected");
                                Console.ResetColor();
                                //currentToken = currentToken.RawToken;
                                //if (currentToken.Next is ScopeStart)
                                //{
                                //    currentToken = currentToken.Next;
                                //    functionScope.Token = currentToken;
                                //    currentToken = CreateScope(currentToken, functionScope, depth + 1, iteration + 1).Next;
                                //}
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"{indent}│    ✗ Expected IdentifierToken after FunctionToken, got: {currentToken.Next?.GetType().Name ?? "null"}");
                            Console.ResetColor();
                        }
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"{indent}│  No expectations - terminal token");
                    Console.ResetColor();
                }

                if (currentToken.Next != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"{indent}│  → Moving to next: {currentToken.Next.GetType().Name} ('{currentToken.Next.RawToken?.Text}')");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"{indent}│  ⚠ Next token is null - end of chain");
                    Console.ResetColor();
                }
                
                currentToken = currentToken.Next;
            }
            while (currentToken is not null);

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n{new string(' ', depth * 2)}└─ CreateScope completed");
            Console.WriteLine($"{new string(' ', depth * 2)}   Processed {tokenCount} token(s) in scope '{scope.ScopeName}'");
            Console.ResetColor();
            
            return default;
        }
    }
}
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;

namespace ppotepa.tokenez.Tree
{

    public partial class TokenTree
    {

        public Scope CreateScope(Token currentToken, Scope scope, int depth = 0, int iteration = 0, int parenthesisDepth = 0)
        {
            string indent = new string('\t', depth + 1);
            Console.WriteLine($"{indent}CreateScope: {scope.ScopeName} (depth={depth})");

            do
            {
                bool hasExpectations = currentToken.Expectations.Length != 0;
                Console.WriteLine($"{indent}\tProcessing: {currentToken.GetType().Name} '{currentToken.RawToken?.Text}'");

                if (hasExpectations)
                {
                    if (currentToken is FunctionToken)
                    {
                        Console.WriteLine($"{indent}\t\tFound Function");
                        Scope functionScope = new() { OuterScope = scope };

                        if (currentToken.Next is IdentifierToken)
                        {
                            string functionName = currentToken.Next.RawToken.Text;
                            Console.WriteLine($"{indent}\t\t\tFunction name: {functionName}");
                            Declaration declaration = new FunctionDeclaration($"{scope}.fn_{functionName}");
                            scope.Decarations.Add(functionName, declaration);
                            currentToken = currentToken.Next;

                            if (currentToken.Next is ParenthesisOpen)
                            {
                                parenthesisDepth++;
                                Console.WriteLine($"{indent}\t\t\tParameters starting");
                                //currentToken = currentToken.RawToken;
                                //if (currentToken.Next is ScopeStart)
                                //{
                                //    currentToken = currentToken.Next;
                                //    functionScope.Token = currentToken;
                                //    currentToken = CreateScope(currentToken, functionScope, depth + 1, iteration + 1).Next;
                                //}
                            }
                            if(parenthesisDepth > 0)
                            {
                                throw new InvalidOperationException("Parenthesis not closed.");
                            }
                        }
                    }
                }
                else
                {

                }

                currentToken = currentToken.Next;
            }
            while (currentToken is not null);

            if(depth == 0 && parenthesisDepth > 0)
                throw new InvalidProgramException("Unmatched parenthesis detected.");

            Console.WriteLine($"{indent}Scope complete: {scope.ScopeName}");
            return default;
        }
    }
}
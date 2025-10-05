using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;
using System.Runtime.CompilerServices;

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
                            Declaration declaration = new FunctionDeclaration(currentToken.Next);
                            scope.Decarations.Add(functionName, declaration);
                            currentToken = currentToken.Next;

                            if (currentToken.Next is ParenthesisOpen)
                            {
                                currentToken = currentToken.Next;
                                parenthesisDepth++;
                                Console.WriteLine($"{indent}\t\t\tParameters starting");
                                var parameters = ProcessFunctionParameters(currentToken.Next);
                            }

                            parenthesisDepth--;

                            if (parenthesisDepth > 0)
                            {
                                throw new UnexpectedTokenException(currentToken, "Parenthesis not closed", typeof(ParenthesisClosed));
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

            if (depth == 0 && parenthesisDepth > 0)
                throw new UnexpectedTokenException(currentToken, "Unmatched parenthesis detected", typeof(ParenthesisClosed));

            Console.WriteLine($"{indent}Scope complete: {scope.ScopeName}");
            return default;
        }

        private (FunctionParametersToken, Token) ProcessFunctionParameters(Token token, FunctionParametersToken parameters = default, int index = 0)
        {
            if (parameters is null)
            {
                parameters = new();
            }

            while (token is not ParenthesisClosed)
            {
                if (token is ITypeToken)
                {
                    var type = token;
                    var expectedIdentifier = token.Next;

                    if (expectedIdentifier is not IdentifierToken)
                    {
                        throw new UnexpectedTokenException(expectedIdentifier, typeof(IdentifierToken));
                    }

                    parameters.Declarations.Add(new ParameterDeclaration(type, expectedIdentifier));
                    token = token.Next;
                    continue;
                }

                if (token.Next is CommaSeparatorToken)
                {
                    token = token.Next.Next;
                    continue;
                }

                if (token.Next is ParenthesisClosed)
                {
                    return (parameters, token.Next.Next);
                }

                throw new UnexpectedTokenException(token, "Unexpected token in parameters list", typeof(ITypeToken), typeof(CommaSeparatorToken), typeof(ParenthesisClosed));
            }

            throw new UnexpectedTokenException(token, "Unexpected token in parameters list", typeof(ITypeToken), typeof(CommaSeparatorToken), typeof(ParenthesisClosed));
        }
    }
}
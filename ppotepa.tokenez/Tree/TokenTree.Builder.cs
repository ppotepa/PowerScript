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
            do
            {
                bool hasExpectations = currentToken.Expectations.Length != 0;

                if (hasExpectations)
                {
                    if (currentToken is FunctionToken)
                    {
                        Scope functionScope = new() { OuterScope = scope };

                        if (currentToken.Next is IdentifierToken)
                        {
                            Declaration declaration = new FunctionDeclaration($"{scope}.fn_{currentToken.Next.RawToken.Text}");
                            scope.Decarations.Add(currentToken.Next.RawToken.Text, declaration);
                        }

                        currentToken = currentToken.Next;
                        bool parametersStarted = false;
                        if (currentToken.Next is ParenthesisOpen)
                        {
                            parametersStarted = true;
                            var parameters = ParseParameters(currentToken.Next);

                        }
                        else
                        {
                            throw new Exception("Function must have parameters");
                        }
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

        private Token ParseParameters(Token next)
        {
            Token tmpNext = next;
            while (tmpNext.Next is not ParenthesisClosed)
            {
                tmpNext = tmpNext.Next;

                if (tmpNext is ParenthesisClosed)
                {
                    break;
                }
            }
            throw new InvalidOperationException("Closing parenthesis not found");
        }
    }
}
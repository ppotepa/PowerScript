using ppotepa.tokenez.Tree;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Delimiters;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Interfaces;
using ppotepa.tokenez.Tree.Tokens.Keywords.Types;
using ppotepa.tokenez.Tree.Tokens.Operators;

namespace ppotepa.tokenez.Tree.Builders
{
    /// <summary>
    /// Responsible for parsing and validating function parameter lists
    /// </summary>
    internal class ParameterProcessor
    {
        public (FunctionParametersToken Parameters, Token NextToken) ProcessParameters(Token startToken)
        {
            var parameters = new FunctionParametersToken();
            var currentToken = startToken;

            while (currentToken is not ParenthesisClosed)
            {
                if (TryProcessTypeAndIdentifier(currentToken, parameters, out var nextToken))
                {
                    currentToken = nextToken;
                    continue;
                }

                currentToken = currentToken.Next;

                if (currentToken is CommaSeparatorToken)
                {
                    currentToken = currentToken.Next;
                    continue;
                }

                if (currentToken is ParenthesisClosed)
                {
                    return (parameters, currentToken.Next?.Next);
                }

                ThrowUnexpectedTokenException(currentToken);
            }

            ThrowUnexpectedTokenException(currentToken);
            return default; // Unreachable
        }

        private bool TryProcessTypeAndIdentifier(Token token, FunctionParametersToken parameters, out Token nextToken)
        {
            if (token is not ITypeToken)
            {
                nextToken = null;
                return false;
            }

            var typeToken = token;
            var identifierToken = token.Next;

            if (identifierToken is not IdentifierToken)
            {
                throw new UnexpectedTokenException(identifierToken, typeof(IdentifierToken));
            }

            parameters.Declarations.Add(new ParameterDeclaration(typeToken, identifierToken));
            nextToken = identifierToken.Next;
            return true;
        }

        private static void ThrowUnexpectedTokenException(Token token)
        {
            throw new UnexpectedTokenException(
                token,
                "Unexpected token in parameters list",
                typeof(ITypeToken),
                typeof(CommaSeparatorToken),
                typeof(ParenthesisClosed));
        }
    }
}

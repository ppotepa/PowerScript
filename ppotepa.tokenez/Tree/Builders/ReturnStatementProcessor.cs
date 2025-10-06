using ppotepa.tokenez.Tree.Builders.Interfaces;
using ppotepa.tokenez.Tree.Exceptions;
using ppotepa.tokenez.Tree.Expressions;
using ppotepa.tokenez.Tree.Statements;
using ppotepa.tokenez.Tree.Tokens.Base;
using ppotepa.tokenez.Tree.Tokens.Identifiers;
using ppotepa.tokenez.Tree.Tokens.Keywords;
using ppotepa.tokenez.Tree.Tokens.Operators;
using ppotepa.tokenez.Tree.Tokens.Scoping;
using ppotepa.tokenez.Tree.Tokens.Values;

namespace ppotepa.tokenez.Tree.Builders
{
    internal class ReturnStatementProcessor : ITokenProcessor
    {
        private readonly ExpectationValidator _validator;

        public ReturnStatementProcessor(ExpectationValidator validator)
        {
            _validator = validator;
        }

        public bool CanProcess(Token token)
        {
            return token is ReturnKeywordToken;
        }

        public TokenProcessingResult Process(Token token, ProcessingContext context)
        {
            if (!context.IsInsideFunction)
            {
                throw new InvalidReturnStatementException(token);
            }

            var expression = ParseExpression(token.Next);

            var returnStatement = new ReturnStatement(token, expression);
            context.CurrentScope.Statements.Add(returnStatement);
            context.CurrentScope.HasReturn = true;

            var nextToken = FindNextToken(token.Next);

            return TokenProcessingResult.Continue(nextToken);
        }

        private Expression ParseExpression(Token startToken)
        {
            if (startToken == null)
                throw new UnexpectedTokenException(startToken, "Expected expression after RETURN");

            if (startToken is IdentifierToken identToken)
            {
                if (startToken.Next is PlusToken or MinusToken or MultiplyToken or DivideToken)
                {
                    var left = new IdentifierExpression(identToken);
                    var op = startToken.Next;
                    var right = ParseSimpleExpression(op.Next);
                    return new BinaryExpression(left, op, right);
                }

                return new IdentifierExpression(identToken);
            }

            if (startToken is ValueToken valueToken)
            {
                return new LiteralExpression(valueToken);
            }

            throw new UnexpectedTokenException(startToken, typeof(IdentifierToken), typeof(ValueToken));
        }

        private Expression ParseSimpleExpression(Token token)
        {
            if (token is IdentifierToken identToken)
                return new IdentifierExpression(identToken);

            if (token is ValueToken valueToken)
                return new LiteralExpression(valueToken);

            throw new UnexpectedTokenException(token, typeof(IdentifierToken), typeof(ValueToken));
        }

        private Token FindNextToken(Token expressionStart)
        {
            var current = expressionStart;

            while (current != null && current is not ScopeEndToken)
            {
                if (current is IdentifierToken or ValueToken)
                {
                    if (current.Next is PlusToken or MinusToken or MultiplyToken or DivideToken)
                    {
                        current = current.Next?.Next;
                    }
                    else
                    {
                        current = current.Next;
                    }
                }
                else
                {
                    current = current.Next;
                }

                if (current is ScopeEndToken)
                    return current;
            }

            return current;
        }
    }
}

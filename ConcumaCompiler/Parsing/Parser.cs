using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Parsing
{
    public class ParseException : Exception
    {
        public Token Token { get; }

        public ParseException(Token token, string message) : base(message)
        {
            Token = token;
        }
    }

    public sealed class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Statement> Parse()
        {
            List<Statement> statements = new();

            while (!IsEnd())
            {
                try
                {
                    statements.Add(ParseStatement());
                }
                catch (ParseException p)
                {
                    ErrorHandling.Parsing(p.Token, p.Message);
                    return statements;
                }
            }

            return statements;
        }

        private Statement ParseStatement()
        {
            switch (Advance().Type)
            {
                case TokenType.Print:
                    Statement s = new Statement.PrintStmt(ParseExpression());
                    Consume(TokenType.Semicolon, "Expected semicolon after expression.");
                    return s;
                case TokenType.If:
                    return IfStatement();
                case TokenType.LeftBrace:
                    return BlockStatement();
                case TokenType.Const:
                    return DeclarationStatement(true);
                case TokenType.Var:
                    return DeclarationStatement(false);
                case TokenType.For:
                    return ForStatement();
                case TokenType.Function:
                    return Function();
                case TokenType.Break:
                    Consume(TokenType.Semicolon, "Expected ';' after 'break'.");
                    return new Statement.Break();
                case TokenType.Return:
                    Expression? e = null;
                    if (!Match(TokenType.Semicolon))
                    {
                        e = ParseExpression();
                    }
                    Consume(TokenType.Semicolon, "Expected ';' after 'return'.");
                    return new Statement.ReturnStmt(e);
                case TokenType.Identifier:
                    return Identifier();
            }

            throw new ParseException(Previous(), "Unknown statement.");
        }

        private Statement Function()
        {
            Token name = Advance();
            Consume(TokenType.LeftParen, "Expected '(' after method name.");

            List<Token> parameters = new();

            while (true)
            {
                parameters.Add(Advance());
                if (!Match(TokenType.Comma))
                {
                    break;
                }
            }

            Consume(TokenType.RightParen, "Expected ')' after function parameters.");

            Statement action = ParseStatement();

            return new Statement.Function(name, parameters, action);
        }

        private Statement ForStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'for'.");
            Statement? initializer = null;
            if (Peek().Type != TokenType.Semicolon)
            {
                initializer = DeclarationStatement(false);
            }
            Expression? condition = new Expression.Literal(true);
            if (Peek().Type != TokenType.Semicolon)
            {
                condition = ParseExpression();
                Consume(TokenType.Semicolon, "Expected semicolon after for condition.");
            }
            Statement? accumulator = null;
            if (Peek().Type != TokenType.RightParen)
            {
                Advance();
                accumulator = Identifier(false);
            }
            Consume(TokenType.RightParen, "Expected ')' after for arguments.");

            Statement action = ParseStatement();

            return new Statement.ForStmt(initializer, condition, accumulator, action);
        }

        private Statement Identifier(bool requireSemicolon = true)
        {
            Token name = Previous();

            if (Match(TokenType.Equal))
            {
                Expression initializer = ParseExpression();
                if (requireSemicolon) Consume(TokenType.Semicolon, "Expected ';' after redefinition.");
                return new Statement.DefinitionStmt(name, initializer);
            }
            else if (Match(TokenType.LeftParen))
            {
                List<Expression> parameters = new();

                while (true)
                {
                    parameters.Add(ParseExpression());
                    if (!Match(TokenType.Comma))
                    {
                        break;
                    }
                }

                Consume(TokenType.RightParen, "Expected ')' after function parameters.");
                Consume(TokenType.Semicolon, "Expected ';' after function call.");
                return new Statement.CallStmt(name, parameters);
            }

            throw new ParseException(name, "Floating identifier.");
        }

        private Statement DeclarationStatement(bool isConst)
        {
            Token name = Advance();
            Expression? initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = ParseExpression();
            }

            Consume(TokenType.Semicolon, "Expected ';' after declaration.");

            return new Statement.DeclarationStmt(name, initializer, isConst);
        }

        private Statement IfStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'if'.");

            Expression condition = ParseExpression();

            Consume(TokenType.RightParen, "Expected ')' after if condition.");

            Statement ifBranch = ParseStatement();
            Statement? elseBranch = null;

            if (Match(TokenType.Else))
            {
                elseBranch = ParseStatement();
            }

            return new Statement.IfStmt(condition, ifBranch, elseBranch);
        }

        private Statement BlockStatement()
        {
            List<Statement> statements = new();

            while (!Match(TokenType.RightBrace))
            {
                statements.Add(ParseStatement());
            }

            return new Statement.BlockStmt(statements);
        }

        private Expression ParseExpression()
        {
            return Equality();
        }

        private Expression Equality()
        {
            Expression e = Comparison();

            while (Match(TokenType.BangEqual, TokenType.EqualEqual))
            {
                Token op = Previous();
                Expression right = Comparison();
                e = new Expression.Binary(e, right, op);
            }

            return e;
        }

        private Expression Comparison()
        {
            Expression e = Term();

            while (Match(TokenType.Less, TokenType.LessEqual, TokenType.Greater, TokenType.GreaterEqual))
            {
                Token op = Previous();
                Expression right = Term();
                e = new Expression.Binary(e, right, op);
            }

            return e;
        }

        private Expression Term()
        {
            Expression e = Factor();

            while (Match(TokenType.Plus, TokenType.Minus))
            {
                Token op = Previous();
                Expression right = Factor();
                e = new Expression.Binary(e, right, op);
            }

            return e;
        }

        private Expression Factor()
        {
            Expression e = Unary();

            while (Match(TokenType.Star, TokenType.Slash))
            {
                Token op = Previous();
                Expression right = Unary();
                e = new Expression.Binary(e, right, op);
            }

            return e;
        }

        private Expression Unary()
        {
            while (Match(TokenType.Bang, TokenType.Minus))
            {
                Token op = Previous();
                Expression right = Unary();
                return new Expression.Unary(right, op);
            }

            return Literal();
        }

        private Expression IdentifierExpr()
        {
            Token name = Previous();

            if (Match(TokenType.LeftParen))
            {
                List<Expression> parameters = new();

                while (true)
                {
                    parameters.Add(ParseExpression());
                    if (!Match(TokenType.Comma))
                    {
                        break;
                    }
                }

                Consume(TokenType.RightParen, "Expected ')' after function parameters.");
                return new Expression.Call(name, parameters);
            }
            else
            {
                return new Expression.Var(name);
            }
        }

        private Expression Literal()
        {
            if (Match(TokenType.False)) return new Expression.Literal(false);
            else if (Match(TokenType.True)) return new Expression.Literal(true);
            else if (Match(TokenType.Null)) return new Expression.Literal(null);

            if (Match(TokenType.Integer, TokenType.Double, TokenType.String))
                return new Expression.Literal(Previous().Value);

            if (Match(TokenType.Identifier))
                return IdentifierExpr();

            if (Match(TokenType.LeftParen))
            {
                Expression e = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return new Expression.Grouping(e);
            }

            throw new ParseException(Peek(), "Uncaught Token.");
        }

        private bool IsEnd() => Peek().Type == TokenType.EoF;

        private Token Peek() => _tokens[_current];
        private Token Previous() => _tokens[_current - 1];
        private Token Advance()
        {
            if (!IsEnd()) _current++;
            return Previous();
        }

        private bool Match(params TokenType[] types)
        {
            foreach (TokenType type in types)
            {
                if (Peek().Type == type)
                {
                    Advance();
                    return true;
                }
            }
            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Peek().Type == type) return Advance();

            throw new ParseException(Peek(), message);
        }
    }
}

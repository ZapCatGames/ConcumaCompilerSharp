using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Parsing
{
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
                statements.Add(ParseStatement());
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
                case TokenType.Identifier:
                    return Identifier();
            }

            Synchronize();
            return ParseStatement();
        }

        private Statement ForStatement()
        {
            Consume(TokenType.LeftParen, "Expected '(' after 'for'.");
            Statement? initializer = null;
            if (Peek().Type != TokenType.Semicolon)
            {
                Advance();
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

            ErrorHandling.Parsing(name, "Floating identifier.");
            throw new Exception();
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

        private Expression Literal()
        {
            if (Match(TokenType.False)) return new Expression.Literal(false);
            else if (Match(TokenType.True)) return new Expression.Literal(true);
            else if (Match(TokenType.Null)) return new Expression.Literal(null);

            if (Match(TokenType.Integer, TokenType.Double, TokenType.String))
                return new Expression.Literal(Previous().Value);

            if (Match(TokenType.Identifier))
                return new Expression.Var(Previous());

            if (Match(TokenType.LeftParen))
            {
                Expression e = ParseExpression();
                Consume(TokenType.RightParen, "Expected ')' after expression.");
                return new Expression.Grouping(e);
            }

            ErrorHandling.Parsing(Peek(), "Uncaught Token!");
            throw new Exception("Uncaught Token.");
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

            ErrorHandling.Parsing(Peek(), message);
            Synchronize();
            return Peek();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsEnd())
            {
                if (Previous().Type == TokenType.Semicolon) return;

                switch (Peek().Type)
                {
                    case TokenType.Print:
                        return;
                }
            }

            Advance();
        }
    }
}

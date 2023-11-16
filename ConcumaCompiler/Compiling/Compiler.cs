using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaCompiler.Compiling
{
    public sealed class Compiler
    {
        private readonly List<Statement> _statements = new();
        private readonly List<byte> _bytecode = new();

        public Compiler(List<Statement> statements)
        {
            _statements = statements;
        }

        public List<byte> Compile()
        {
            foreach (Statement stmt in _statements)
            {
                EvaluateStatement(stmt);
            }

            return _bytecode;
        }

        private void EvaluateStatement(Statement stmt)
        {
            switch (stmt)
            {
                case Statement.PrintStmt p:
                    {
                        _bytecode.Add(0x01);
                        ParseExpression(p.Expression);
                        break;
                    }
            }
        }

        private void ParseExpression(Expression expression)
        {
            switch (expression)
            {
                case Expression.Unary u:
                    _bytecode.Add(0x01);
                    Unary(u);
                    break;
                case Expression.Binary b:
                    _bytecode.Add(0x02);
                    Binary(b);
                    break;
                case Expression.Grouping g:
                    _bytecode.Add(0x03);
                    ParseExpression(g.Expression);
                    break;
                case Expression.Literal l:
                    _bytecode.Add(0x04);
                    Literal(l);
                    break;
            }
        }

        private void Unary(Expression.Unary u)
        {
            switch (u.Operator.Type)
            {
                case Lexing.TokenType.Bang:
                    {
                        _bytecode.Add(0x01);
                        break;
                    }
                case Lexing.TokenType.Minus:
                    {
                        _bytecode.Add(0x02);
                        break;
                    }
            }

            ParseExpression(u.Right);
        }

        private void Binary(Expression.Binary b)
        {
            switch (b.Operator.Type)
            {
                case TokenType.Plus:
                    {
                        _bytecode.Add(0x01);
                        break;
                    }
                case TokenType.Minus:
                    {
                        _bytecode.Add(0x02);
                        break;
                    }
                case TokenType.Star:
                    {
                        _bytecode.Add(0x03);
                        break;
                    }
                case TokenType.Slash:
                    {
                        _bytecode.Add(0x04);
                        break;
                    }
                case TokenType.EqualEqual:
                    {
                        _bytecode.Add(0x05);
                        break;
                    }
                case TokenType.BangEqual:
                    {
                        _bytecode.Add(0x06);
                        break;
                    }
                case TokenType.Less:
                    {
                        _bytecode.Add(0x07);
                        break;
                    }
                case TokenType.LessEqual:
                    {
                        _bytecode.Add(0x08);
                        break;
                    }
                case TokenType.Greater:
                    {
                        _bytecode.Add(0x09);
                        break;
                    }
                case TokenType.GreaterEqual:
                    {
                        _bytecode.Add(0x0A);
                        break;
                    }
            }

            ParseExpression(b.Left);
            ParseExpression(b.Right);
        }

        private void Literal(Expression.Literal l)
        {
            switch (l.Value)
            {
                case null:
                    _bytecode.Add(0x00);
                    break;
                case true:
                    _bytecode.Add(0x01);
                    _bytecode.Add(0x01);
                    break;
                case false:
                    _bytecode.Add(0x01);
                    _bytecode.Add(0x00);
                    break;
                case int i:
                    _bytecode.Add(0x02);
                    _bytecode.AddRange(BitConverter.GetBytes(i));
                    break;
                case double d:
                    _bytecode.Add(0x03);
                    _bytecode.AddRange(BitConverter.GetBytes(d));
                    break;
                case string s:
                    _bytecode.Add(0x04);
                    _bytecode.Add((byte)s.Length);
                    foreach (char c in s)
                    {
                        _bytecode.Add((byte)c);
                    }
                    break;
            }
        }
    }
}

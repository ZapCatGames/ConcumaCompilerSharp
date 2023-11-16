using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaCompiler.Compiling
{
    public sealed class Compiler
    {
        private readonly List<Statement> _statements = new();
        private readonly List<byte> _bytecode = new();

        private readonly Dictionary<string, int> _definedSymbols = new();
        private int _symbolIndex = 0;

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
                        EvaluateExpression(p.Expression);
                        break;
                    }
                case Statement.IfStmt i:
                    {
                        _bytecode.Add(0x02);
                        EvaluateExpression(i.Condition);
                        EvaluateStatement(i.If);
                        if (i.Else is not null)
                        {
                            EvaluateStatement(i.Else);
                        }
                        else
                        {
                            _bytecode.Add(0x00);
                        }
                        break;
                    }
                case Statement.BlockStmt b:
                    {
                        _bytecode.Add(0x03);
                        _bytecode.AddRange(BitConverter.GetBytes(b.Statements.Count));
                        for (int i = 0; i < b.Statements.Count; i++)
                        {
                            EvaluateStatement(b.Statements[i]);
                        }
                        break;
                    }
                case Statement.DeclarationStmt decl:
                    {
                        _bytecode.Add(0x04);
                        _bytecode.Add(decl.IsConst ? (byte)0x01 : (byte)0x00);
                        _bytecode.AddRange(BitConverter.GetBytes(_symbolIndex));
                        if (decl.Initializer is null)
                        {
                            _bytecode.Add(0x00);
                        }
                        else
                        {
                            EvaluateExpression(decl.Initializer);
                        }
                        _definedSymbols.Add(decl.Name.Lexeme, _symbolIndex++);
                        break;
                    }
                case Statement.DefinitionStmt def:
                    {
                        _bytecode.Add(0x05);
                        _bytecode.AddRange(BitConverter.GetBytes(_definedSymbols[def.Name.Lexeme]));
                        EvaluateExpression(def.Value);
                        break;
                    }
                case Statement.ForStmt f:
                    {
                        _bytecode.Add(0x06);
                        if (f.Initializer != null)
                        {
                            EvaluateStatement(f.Initializer);
                        }
                        else
                        {
                            _bytecode.Add(0x00);
                        }

                        EvaluateExpression(f.Condition);

                        EvaluateStatement(f.Action);

                        if (f.Accumulator != null)
                        {
                            EvaluateStatement(f.Accumulator);
                        }
                        else
                        {
                            _bytecode.Add(0x00);
                        }
                        break;
                    }
            }
        }

        private void EvaluateExpression(Expression expression)
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
                    EvaluateExpression(g.Expression);
                    break;
                case Expression.Literal l:
                    _bytecode.Add(0x04);
                    Literal(l);
                    break;
                case Expression.Var v:
                    _bytecode.Add(0x05);
                    Var(v);
                    break;
            }
        }

        private void Var(Expression.Var v)
        {
            _bytecode.AddRange(BitConverter.GetBytes(_definedSymbols[v.Name.Lexeme]));
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

            EvaluateExpression(u.Right);
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

            EvaluateExpression(b.Left);
            EvaluateExpression(b.Right);
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

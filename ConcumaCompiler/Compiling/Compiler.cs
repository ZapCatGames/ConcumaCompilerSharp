using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaCompiler.Compiling
{
    public class CompilerException : Exception
    {
        public int Line { get; }

        public CompilerException(int line, string message) : base(message)
        {
            Line = line;
        }
    }

    public sealed class Compiler
    {
        private readonly List<Statement> _statements = new();
        private readonly List<byte> _bytecode = new();

        private ConcumaEnvironment _currentEnv = new(null);
        private int _symbolIndex = 1;

        public Compiler(List<Statement> statements)
        {
            _statements = statements;
        }

        public List<byte> Compile()
        {
            foreach (Statement stmt in _statements)
            {
                try
                {
                    EvaluateStatement(stmt);
                }
                catch (CompilerException c)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\"{c.Message}\" on line {c.Line}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
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
                        _currentEnv.Add(decl.Name.Lexeme, _symbolIndex++);
                        break;
                    }
                case Statement.DefinitionStmt def:
                    {
                        _bytecode.Add(0x05);
                        _bytecode.AddRange(BitConverter.GetBytes(_currentEnv.Find(def.Name)));
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
                case Statement.Break:
                    {
                        _bytecode.Add(0x07);
                        break;
                    }
                case Statement.Function fn:
                    {
                        _bytecode.Add(0x08);
                        _bytecode.AddRange(BitConverter.GetBytes(_symbolIndex));
                        _currentEnv.Add(fn.Name.Lexeme, _symbolIndex++);
                        _currentEnv = new ConcumaEnvironment(_currentEnv);
                        _bytecode.AddRange(BitConverter.GetBytes(fn.Parameters.Count));
                        for (int i = 0; i < fn.Parameters.Count; i++)
                        {
                            _bytecode.AddRange(BitConverter.GetBytes(_symbolIndex));
                            _currentEnv.Add(fn.Parameters[i].Lexeme, _symbolIndex++);
                        }
                        EvaluateStatement(fn.Action);
                        _currentEnv = _currentEnv.Exit()!;
                        break;
                    }
                case Statement.CallStmt call:
                    {
                        _bytecode.Add(0x09);
                        _bytecode.AddRange(BitConverter.GetBytes(_currentEnv.Find(call.Name)));
                        _bytecode.AddRange(BitConverter.GetBytes(call.Parameters.Count));
                        for (int i = 0; i < call.Parameters.Count; i++)
                        {
                            EvaluateExpression(call.Parameters[i]);
                        }
                        break;
                    }
                case Statement.ReturnStmt ret:
                    {
                        _bytecode.Add(0x0A);
                        if (ret.Value is not null)
                        {
                            EvaluateExpression(ret.Value);
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
                case Expression.Call c:
                    _bytecode.Add(0x06);
                    Call(c);
                    break;

            }
        }

        private void Call(Expression.Call c)
        {
            _bytecode.AddRange(BitConverter.GetBytes(_currentEnv.Find(c.Name)));
            _bytecode.AddRange(BitConverter.GetBytes(c.Parameters.Count));
            for (int i = 0; i < c.Parameters.Count; i++)
            {
                EvaluateExpression(c.Parameters[i]);
            }
        }

        private void Var(Expression.Var v)
        {
            _bytecode.AddRange(BitConverter.GetBytes(_currentEnv.Find(v.Name)));
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

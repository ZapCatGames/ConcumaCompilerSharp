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

        private ConcumaEnvironment _currentEnv = new(0, null);
        private readonly Dictionary<string, int> _allSymbols = new();
        private int _symbolIndex = 1;

        public Compiler(List<Statement> statements)
        {
            _statements = statements;
        }

        public List<byte> Compile()
        {
            _bytecode.AddRange(BitConverter.GetBytes(0));

            foreach (Statement stmt in _statements)
            {
                try
                {
                    EvaluateStatement(stmt);
                }
                catch (CompilerException c)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"CompilationError: \"{c.Message}\" on line {c.Line}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            byte[] stLoc = BitConverter.GetBytes(_bytecode.Count);
            _bytecode[0] = stLoc[0];
            _bytecode[1] = stLoc[1];
            _bytecode[2] = stLoc[2];
            _bytecode[3] = stLoc[3];

            foreach (KeyValuePair<string, int> symbol in _allSymbols)
            {
                _bytecode.AddRange(BitConverter.GetBytes(symbol.Key.Length));
                foreach (char c in symbol.Key)
                {
                    _bytecode.Add((byte)c);
                }
                _bytecode.AddRange(BitConverter.GetBytes(symbol.Value));
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
                        int symbolIndex = GetSymbolIndex(decl.Name.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        if (decl.Initializer is null)
                        {
                            _bytecode.Add(0x00);
                        }
                        else
                        {
                            EvaluateExpression(decl.Initializer);
                        }
                        _currentEnv.Add(decl.Name.Lexeme, symbolIndex);
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
                        int symbolIndex = GetSymbolIndex(fn.Name.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        _currentEnv.Add(fn.Name.Lexeme, symbolIndex);
                        _currentEnv = new ConcumaEnvironment(symbolIndex, _currentEnv);
                        _bytecode.AddRange(BitConverter.GetBytes(fn.Parameters.Count));
                        for (int i = 0; i < fn.Parameters.Count; i++)
                        {
                            int pSymbolIndex = GetSymbolIndex(fn.Parameters[i].Lexeme);
                            _bytecode.AddRange(BitConverter.GetBytes(pSymbolIndex));
                            _currentEnv.Add(fn.Parameters[i].Lexeme, pSymbolIndex);
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
                case Statement.ClassStmt cls:
                    {
                        _bytecode.Add(0x0B);
                        int symbolIndex = GetSymbolIndex(cls.Name.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        _currentEnv.Add(cls.Name.Lexeme, symbolIndex);
                        _currentEnv = new ConcumaEnvironment(symbolIndex, _currentEnv);
                        _bytecode.AddRange(BitConverter.GetBytes(cls.Variables.Count));
                        for (int i = 0; i < cls.Variables.Count; i++)
                        {
                            EvaluateStatement(cls.Variables[i]);
                        }
                        _bytecode.AddRange(BitConverter.GetBytes(cls.Methods.Count));
                        for (int i = 0; i < cls.Methods.Count; i++)
                        {
                            EvaluateStatement(cls.Methods[i]);
                        }
                        _currentEnv = _currentEnv.Exit()!;
                        break;
                    }
                case Statement.ModuleStmt mod:
                    {
                        _bytecode.Add(0x0C);
                        int symbolIndex = GetSymbolIndex(mod.Name.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        _currentEnv.Add(mod.Name.Lexeme, symbolIndex);
                        _currentEnv = new ConcumaEnvironment(symbolIndex, _currentEnv);
                        _bytecode.AddRange(BitConverter.GetBytes(mod.Variables.Count));
                        for (int i = 0; i < mod.Variables.Count; i++)
                        {
                            EvaluateStatement(mod.Variables[i]);
                        }
                        _bytecode.AddRange(BitConverter.GetBytes(mod.Methods.Count));
                        for (int i = 0; i < mod.Methods.Count; i++)
                        {
                            EvaluateStatement(mod.Methods[i]);
                        }
                        _currentEnv = _currentEnv.Exit()!;
                        break;
                    }
                case Statement.ImportStmt imp:
                    {
                        _bytecode.Add(0x0D);
                        int symbolIndex = GetSymbolIndex(imp.Identifier.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        _currentEnv.Add(imp.Identifier.Lexeme, symbolIndex);
                        if (imp.Alias is Token alias)
                        {
                            int aliasIndex = GetSymbolIndex(alias.Lexeme);
                            _bytecode.AddRange(BitConverter.GetBytes(aliasIndex));
                            _currentEnv.Add(alias.Lexeme, aliasIndex);
                        }
                        else
                        {
                            _bytecode.Add(0x00);
                        }
                        break;
                    }
                case Statement.BinaryStmt bin:
                    {
                        _bytecode.Add(0x0E);
                        int symbolIndex = GetSymbolIndex(bin.Func.Name.Lexeme);
                        _bytecode.AddRange(BitConverter.GetBytes(symbolIndex));
                        _currentEnv.Add(bin.Func.Name.Lexeme, symbolIndex);
                        _currentEnv = new ConcumaEnvironment(symbolIndex, _currentEnv);
                        for (int i = 0; i < 2; i++)
                        {
                            int pSymbolIndex = GetSymbolIndex(bin.Func.Parameters[i].Lexeme);
                            _bytecode.AddRange(BitConverter.GetBytes(pSymbolIndex));
                            _currentEnv.Add(bin.Func.Parameters[i].Lexeme, pSymbolIndex);
                        }
                        EvaluateStatement(bin.Func.Action);
                        _currentEnv = _currentEnv.Exit()!;
                        break;
                    }
            }
        }

        private int GetSymbolIndex(string key)
        {
            if (_allSymbols.TryGetValue(key, out int symbol)) return symbol;
            _allSymbols.Add(key, _symbolIndex++);
            return _symbolIndex - 1;
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
                case Expression.Accessor a:
                    _bytecode.Add(0x07);
                    Accessor(a);
                    break;
            }
        }

        private void Accessor(Expression.Accessor a)
        {
            EvaluateExpression(a.Left);
            if (a.Left is Expression.Var v)
            {
                int es = _currentEnv.Find(v.Name);
                _currentEnv = _currentEnv.GetChild(es);
            }
            else
            {
                throw new CompilerException(a.Operator.Line, "Left side of accessor must be an identifier.");
            }

            EvaluateExpression(a.Right);

            _currentEnv = _currentEnv.Exit()!;
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
            if (b.Operator is Expression.Var v)
            {
                if (v.Name.Type == TokenType.Identifier)
                {
                    _bytecode.Add(0x01);
                    EvaluateExpression(b.Operator);
                }
                else
                {
                    _bytecode.Add(0x00);

                    switch (v.Name.Type)
                    {
                        case TokenType.Plus:
                        case TokenType.PlusEqual:
                        case TokenType.PlusPlus:
                            {
                                _bytecode.Add(0x01);
                                break;
                            }
                        case TokenType.Minus:
                        case TokenType.MinusEqual:
                        case TokenType.MinusMinus:
                            {
                                _bytecode.Add(0x02);
                                break;
                            }
                        case TokenType.Star:
                        case TokenType.StarEqual:
                            {
                                _bytecode.Add(0x03);
                                break;
                            }
                        case TokenType.Slash:
                        case TokenType.SlashEqual:
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
                        default:
                            throw new CompilerException(0, "Unexpected operator type.");
                    }
                }
            }
            else
            {
                throw new CompilerException(0, "Unexpected operator type.");
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
                    _bytecode.AddRange(BitConverter.GetBytes(s.Length));
                    foreach (char c in s)
                    {
                        _bytecode.Add((byte)c);
                    }
                    break;
            }
        }
    }
}

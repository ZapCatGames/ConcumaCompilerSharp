using System.Globalization;

namespace ConcumaCompiler.Lexing
{
    public sealed class Tokenizer
    {
        private static readonly Dictionary<string, TokenType> _keywords = new()
        {
            { "true", TokenType.True },
            { "false", TokenType.False },
            { "null", TokenType.Null },
            { "print", TokenType.Print }
        };

        private readonly string _text;
        private int _current = 0;
        private int _line = 1;

        public Tokenizer(string text)
        {
            _text = text;
        }

        public List<Token> Lex()
        {
            List<Token> tokens = new();

            while (!IsEnd())
            {
                switch (Advance())
                {
                    case ' ':
                    case '\r':
                    case '\t':
                        {
                            break;
                        }
                    case '\n':
                        {
                            _line++;
                            break;
                        }
                    case '+':
                        {
                            tokens.Add(Token(TokenType.Plus, "+"));
                            break;
                        }
                    case '-':
                        {
                            tokens.Add(Token(TokenType.Minus, "-"));
                            break;
                        }
                    case '*':
                        {
                            tokens.Add(Token(TokenType.Star, "*"));
                            break;
                        }
                    case '/':
                        {
                            if (Peek() == '/')
                            {
                                while (Advance() != '\n') ;
                                break;
                            }

                            tokens.Add(Token(TokenType.Slash, "/"));
                            break;
                        }
                    case '(':
                        {
                            tokens.Add(Token(TokenType.LeftParen, "("));
                            break;
                        }
                    case ')':
                        {
                            tokens.Add(Token(TokenType.RightParen, ")"));
                            break;
                        }
                    case '=':
                        {
                            if (Peek() == '=')
                            {
                                Advance();
                                tokens.Add(Token(TokenType.EqualEqual, "=="));
                                break;
                            }

                            break;
                        }
                    case '!':
                        {
                            if (Peek() == '=')
                            {
                                Advance();
                                tokens.Add(Token(TokenType.BangEqual, "!="));
                                break;
                            }

                            tokens.Add(Token(TokenType.Bang, "!"));
                            break;
                        }
                    case '<':
                        {
                            if (Peek() == '=')
                            {
                                Advance();
                                tokens.Add(Token(TokenType.LessEqual, "<="));
                                break;
                            }

                            tokens.Add(Token(TokenType.Less, "<"));
                            break;
                        }
                    case '>':
                        {
                            if (Peek() == '=')
                            {
                                Advance();
                                tokens.Add(Token(TokenType.GreaterEqual, ">="));
                                break;
                            }

                            tokens.Add(Token(TokenType.Greater, ">"));
                            break;
                        }
                    case ';':
                        {
                            tokens.Add(Token(TokenType.Semicolon, ";"));
                            break;
                        }
                    default:
                        {
                            if (char.IsDigit(Previous()))
                            {
                                tokens.Add(Number());
                                break;
                            }
                            else if (IsAlpha(Previous()))
                            {
                                tokens.Add(Identifier());
                                break;
                            }
                            else if (Previous() == '"')
                            {
                                tokens.Add(String());
                                break;
                            }

                            break;
                        }
                }
            }

            tokens.Add(Token(TokenType.EoF, "<EoF>"));

            return tokens;
        }

        private Token Number()
        {
            string value = "";
            value += Previous();

            bool isDouble = false;

            while (char.IsDigit(Peek()))
            {
                value += Advance();
            }

            if (Peek() == '.')
            {
                isDouble = true;

                value += Advance();
                while (char.IsDigit(Peek()))
                {
                    value += Advance();
                }
            }

            return isDouble ? Token(TokenType.Double, double.Parse(value, CultureInfo.InvariantCulture), value) : Token(TokenType.Integer, int.Parse(value, CultureInfo.InvariantCulture), value);
        }

        private Token Identifier()
        {
            string value = "";
            value += Previous();

            while (IsAlpha(Peek()))
            {
                value += Advance();
            }

            if (_keywords.TryGetValue(value, out TokenType type))
            {
                return Token(type, value);
            }

            return Token(TokenType.Identifier, value, value);
        }

        private Token String()
        {
            string value = "";

            while (Peek() != '"')
            {
                value += Advance();
            }

            Advance();

            return Token(TokenType.String, value, value);
        }

        private bool IsEnd() => _current >= _text.Length;
        private static bool IsAlpha(char c) => char.IsLetter(c) || c == '_';

        private char Peek()
        {
            if (IsEnd())
            {
                ErrorHandling.Lexing('\0', _line, "Unexpected early end of file.");
                return '\0';
            }

            return _text[_current];
        }

        private char Advance()
        {
            if (IsEnd())
            {
                ErrorHandling.Lexing('\0', _line, "Unexpected early end of file.");
                return '\0';
            }

            return _text[_current++];
        }

        private char Previous()
        {
            return _text[_current - 1];
        }

        private Token Token(TokenType type) => new(type, null, "", _line);
        private Token Token(TokenType type, string lexeme) => new(type, null, lexeme, _line);
        private Token Token(TokenType type, object? value) => new(type, value, value?.ToString() ?? "", _line);
        private Token Token(TokenType type, object? value, string lexeme) => new(type, value, lexeme, _line);
    }
}

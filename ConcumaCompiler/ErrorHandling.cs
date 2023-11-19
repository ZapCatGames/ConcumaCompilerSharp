using ConcumaCompiler.Lexing;

namespace ConcumaCompiler
{
    public static class ErrorHandling
    {
        private static readonly List<LexingException> _lexingExceptions = new();
        private static readonly List<ParsingException> _parsingExceptions = new();

        public static void Lexing(char symbol, int line, string message)
        {
            _lexingExceptions.Add(new LexingException(symbol, line, message));
        }

        public static bool ThrowLexingExceptions()
        {
            if (!_lexingExceptions.Any()) return false;

            Console.ForegroundColor = ConsoleColor.Red;

            foreach (LexingException l in _lexingExceptions)
            {
                Console.WriteLine($"LexingError: \"{l.Message}\" at symbol \"{l.Symbol}\" on line {l.Line}.");
            }

            Console.ForegroundColor = ConsoleColor.White;

            return true;
        }

        public static void Parsing(Token token, string message)
        {
            _parsingExceptions.Add(new ParsingException(token, message));
        }

        public static bool ThrowParsingExceptions()
        {
            if (!_parsingExceptions.Any()) return false;

            Console.ForegroundColor = ConsoleColor.Red;

            foreach (ParsingException p in _parsingExceptions)
            {
                Console.WriteLine($"ParsingError: \"{p.Message}\" at symbol \"{p.Token.Lexeme}\" on line {p.Token.Line}.");
            }

            Console.ForegroundColor = ConsoleColor.White;

            return true;
        }

        private record class LexingException(char Symbol, int Line, string Message);
        private record class ParsingException(Token Token, string Message);
    }
}

using ConcumaCompilerFramework.Lexing;

namespace ConcumaCompilerFramework
{
    public static class ErrorHandling
    {
        private static readonly List<LexingException> _lexingExceptions = new();
        private static readonly List<ParsingException> _parsingExceptions = new();

        public static void Lexing(char symbol, int line, string message)
        {
            _lexingExceptions.Add(new LexingException(symbol, line, message));
        }

        public static void Parsing(Token token, string message)
        {
            _parsingExceptions.Add(new ParsingException(token, message));
        }

        private record class LexingException(char Symbol, int Line, string Message);
        private record class ParsingException(Token Token, string Message);
    }
}

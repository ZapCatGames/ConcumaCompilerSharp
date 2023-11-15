namespace ConcumaCompilerFramework
{
    public static class ErrorHandling
    {
        private static List<LexingException> _lexingExceptions = new();

        public static void Lexing(char symbol, int line, string message)
        {
            _lexingExceptions.Add(new LexingException(symbol, line, message));
        }

        private record class LexingException(char Symbol, int Line, string Message);
    }
}

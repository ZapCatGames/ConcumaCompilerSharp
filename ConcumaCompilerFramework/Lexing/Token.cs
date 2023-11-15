namespace ConcumaCompilerFramework.Lexing
{
    public enum TokenType
    {
        // Control Flow
        EoF, LeftParen, RightParen, Identifier, Semicolon,

        // Types - Literals
        Integer, Double, String, True, False, Null,

        // Operations
        Plus, Minus, Star, Slash, Bang,

        // Comparisons
        EqualEqual, BangEqual, GreaterEqual, Greater, LessEqual, Less,

        // Statements
        Print
    }

    public readonly record struct Token(TokenType Type, object? Value, string Lexeme, int Line);
}

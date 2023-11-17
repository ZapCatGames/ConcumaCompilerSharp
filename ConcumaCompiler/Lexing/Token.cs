namespace ConcumaCompiler.Lexing
{
    public enum TokenType
    {
        // Control Flow
        EoF, LeftParen, RightParen, LeftBrace, RightBrace, Identifier, Semicolon, Comma,

        // Types - Literals
        Integer, Double, String, True, False, Null,

        // Operations
        Plus, Minus, Star, Slash, Bang, Equal,

        // Comparisons
        EqualEqual, BangEqual, GreaterEqual, Greater, LessEqual, Less,

        // Statements
        Print, If, Else, Const, Var, For, Function, Return, Break,
    }

    public readonly record struct Token(TokenType Type, object? Value, string Lexeme, int Line);
}

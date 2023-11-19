namespace ConcumaCompiler.Lexing
{
    public enum TokenType
    {
        // Control Flow
        EoF, LeftParen, RightParen, LeftBrace, RightBrace, Identifier, Semicolon, Comma, Dot, As,

        // Types - Literals
        Integer, Double, String, True, False, Null,

        // Operations
        Plus, Minus, Star, Slash, Bang, Equal, PlusEqual, MinusEqual, StarEqual, SlashEqual, PlusPlus, MinusMinus,

        // Comparisons
        EqualEqual, BangEqual, GreaterEqual, Greater, LessEqual, Less,

        // Statements
        Print, If, Else, Const, Var, For, Function, Return, Break, Class, Module, Import,
    }

    public readonly record struct Token(TokenType Type, object? Value, string Lexeme, int Line);
}

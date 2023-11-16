using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Parsing
{
    public abstract class Expression
    {
        public sealed class Literal : Expression
        {
            public Literal(object? value)
            {
                Value = value;
            }

            public object? Value { get; }
        }

        public sealed class Unary : Expression
        {
            public Unary(Expression right, Token @operator)
            {
                Right = right;
                Operator = @operator;
            }

            public Expression Right { get; }
            public Token Operator { get; }
        }

        public sealed class Binary : Expression
        {
            public Binary(Expression left, Expression right, Token @operator)
            {
                Left = left;
                Right = right;
                Operator = @operator;
            }

            public Expression Left { get; }
            public Expression Right { get; }
            public Token Operator { get; }
        }

        public sealed class Grouping : Expression
        {
            public Grouping(Expression expression)
            {
                Expression = expression;
            }

            public Expression Expression { get; }
        }
    }
}

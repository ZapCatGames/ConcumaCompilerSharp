﻿using ConcumaCompiler.Lexing;

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
            public Binary(Expression left, Expression right, Expression @operator)
            {
                Left = left;
                Right = right;
                Operator = @operator;
            }

            public Expression Left { get; }
            public Expression Right { get; }
            public Expression Operator { get; }
        }

        public sealed class Grouping : Expression
        {
            public Grouping(Expression expression)
            {
                Expression = expression;
            }

            public Expression Expression { get; }
        }

        public sealed class Var : Expression
        {
            public Var(Token name)
            {
                Name = name;
            }

            public Token Name { get; }
        }

        public sealed class Call : Expression
        {
            public Call(Token name, List<Expression> parameters)
            {
                Name = name;
                Parameters = parameters;
            }

            public Token Name { get; }
            public List<Expression> Parameters { get; }
        }

        public sealed class Accessor : Expression
        {
            public Accessor(Expression left, Expression right, Token @operator)
            {
                Left = left;
                Right = right;
                Operator = @operator;
            }

            public Expression Left { get; }
            public Expression Right { get; }
            public Token Operator { get; }
        }
    }
}

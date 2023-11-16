using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Parsing
{
    public abstract class Statement
    {
        public abstract class ExpressionStmt : Statement
        {
            protected ExpressionStmt(Expression expression)
            {
                Expression = expression;
            }

            public Expression Expression { get; }
        }

        public sealed class PrintStmt : ExpressionStmt
        {
            public PrintStmt(Expression expression) : base(expression)
            {
            }
        }

        public sealed class IfStmt : Statement
        {
            public IfStmt(Expression condition, Statement ifBranch, Statement? elseBranch)
            {
                this.Condition = condition;
                this.If = ifBranch;
                this.Else = elseBranch;
            }

            public Expression Condition { get; }
            public Statement If { get; }
            public Statement? Else { get; }
        }

        public sealed class BlockStmt : Statement
        {
            public BlockStmt(List<Statement> statements)
            {
                this.Statements = statements;
            }

            public List<Statement> Statements { get; }
        }

        public sealed class DeclarationStmt : Statement
        {
            public DeclarationStmt(Token name, Expression? initializer, bool isConst)
            {
                Name = name;
                Initializer = initializer;
                IsConst = isConst;
            }

            public Token Name { get; }
            public Expression? Initializer { get; }
            public bool IsConst { get; }
        }

        public sealed class DefinitionStmt : Statement
        {
            public DefinitionStmt(Token name, Expression value)
            {
                Name = name;
                Value = value;
            }

            public Token Name { get; }
            public Expression Value { get; }
        }

        public sealed class ForStmt : Statement
        {
            public ForStmt(Statement? initializer, Expression condition, Statement? accumulator, Statement action)
            {
                Initializer = initializer;
                Condition = condition;
                Accumulator = accumulator;
                Action = action;
            }

            public Statement? Initializer { get; }
            public Expression Condition { get; }
            public Statement? Accumulator { get; }
            public Statement Action { get; }
        }
    }
}

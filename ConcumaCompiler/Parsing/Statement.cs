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
    }
}

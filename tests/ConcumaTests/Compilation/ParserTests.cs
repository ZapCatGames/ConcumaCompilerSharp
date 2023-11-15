using ConcumaCompilerFramework.Lexing;
using ConcumaCompilerFramework.Parsing;

namespace ConcumaTests.Compilation
{
    public class ParserTests
    {
        [Fact]
        public void SimpleTest()
        {
            Tokenizer tokenizer = new("print 1 + 1;");
            Parser parser = new(tokenizer.Lex());
            List<Statement> statements = parser.Parse();
            if (statements[0] is not Statement.PrintStmt) throw new Exception("Invalid first statement type.");
        }
    }
}

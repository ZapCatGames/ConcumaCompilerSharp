﻿using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaTests.Compilation
{
    public class ParserTests
    {
        [Fact]
        public void SimpleTest()
        {
            Tokenizer tokenizer = new("print 1 + 1; print 5 + 2 == 7;");
            Parser parser = new(tokenizer.Lex());
            List<Statement> statements = parser.Parse();
            if (statements[0] is not Statement.PrintStmt) throw new Exception("Invalid first statement type.");
            if (statements[1] is not Statement.PrintStmt) throw new Exception("Invalid second statement type.");
        }
    }
}

using ConcumaCompilerFramework.Compiling;
using ConcumaCompilerFramework.Lexing;
using ConcumaCompilerFramework.Parsing;

namespace ConcumaTests.Compilation
{
    public class CompilerTests
    {
        [Fact]
        public void BasicTest()
        {
            Tokenizer tokenizer = new("print 1 + 1;");
            Parser parser = new(tokenizer.Lex());
            Compiler compiler = new(parser.Parse());
            List<byte> byteCode = compiler.Compile();

            Assert.Equal(0x01, byteCode[0]);
            Assert.Equal(15, byteCode.Count);
        }
    }
}

using ConcumaCompilerFramework.Lexing;

namespace ConcumaTests
{
    public class TokenizerTests
    {
        [Fact]
        public void BasicTest()
        {
            Tokenizer tokenizer = new("+*((<=>");
            List<Token> tokens = tokenizer.Lex();
            Assert.Equal(7, tokens.Count);
        }

        [Fact]
        public void IntegerTest()
        {
            Tokenizer tokenizer = new("1 + 69");
            List<Token> tokens = tokenizer.Lex();
            Assert.Equal(1, tokens[0].Value);
            Assert.Equal(69, tokens[2].Value);
            Assert.Equal(4, tokens.Count);
        }

        [Fact]
        public void DoubleTest()
        {
            Tokenizer tokenizer = new("1.4 + 9.87");
            List<Token> tokens = tokenizer.Lex();
            Assert.Equal(1.4d, tokens[0].Value);
            Assert.Equal(9.87d, tokens[2].Value);
            Assert.Equal(4, tokens.Count);
        }

        [Fact]
        public void KeywordTest()
        {
            Tokenizer tokenizer = new("true false null");
            List<Token> tokens = tokenizer.Lex();
            Assert.Equal(TokenType.True, tokens[0].Type);
            Assert.Equal(TokenType.False, tokens[1].Type);
            Assert.Equal(TokenType.Null, tokens[2].Type);
            Assert.Equal(4, tokens.Count);
        }

        [Fact]
        public void StringTest()
        {
            Tokenizer tokenizer = new("\"Hello World!\"");
            List<Token> tokens = tokenizer.Lex();
            Assert.Equal("Hello World!", tokens[0].Value);
            Assert.Equal(2, tokens.Count);
        }
    }
}

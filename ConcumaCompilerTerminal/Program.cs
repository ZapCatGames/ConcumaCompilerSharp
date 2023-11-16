using ConcumaCompiler.Compiling;
using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaCompilerTerminal
{
    static class Program
    {
        private static void Main(string[] args)
        {
            while (true)
            {
                Tokenizer tokenizer = new(Console.ReadLine()!);
                Parser parser = new(tokenizer.Lex());
                Compiler compiler = new(parser.Parse());
            }
        }
    }
}
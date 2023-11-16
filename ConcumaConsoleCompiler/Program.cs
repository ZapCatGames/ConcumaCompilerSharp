using ConcumaCompilerFramework.Compiling;
using ConcumaCompilerFramework.Lexing;
using ConcumaCompilerFramework.Parsing;
using ConcumaRuntimeFramework;

namespace ConcumaConsoleCompiler
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

                VM vm = new(compiler.Compile().ToArray());
                vm.Run();
            }
        }
    }
}
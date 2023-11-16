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
                string outFile = "";

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-o")
                    {
                        outFile = args[i + 1];
                        break;
                    }
                }

                if (string.IsNullOrEmpty(outFile))
                {
                    Console.WriteLine("Need -o flag for output file.");
                }

                Tokenizer tokenizer = new(Console.ReadLine()!);
                Parser parser = new(tokenizer.Lex());
                Compiler compiler = new(parser.Parse());

                File.WriteAllBytes(outFile, compiler.Compile().ToArray());
            }
        }
    }
}
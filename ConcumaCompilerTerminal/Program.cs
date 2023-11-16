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
                if (args.Length < 3)
                {
                    Console.WriteLine("Not enough args.");
                    return;
                }

                string outFile = "";
                if (!File.Exists(args[0]))
                {
                    Console.WriteLine("File does not exist.");
                    return;
                }
                string content = File.ReadAllText(args[0]);

                for (int i = 1; i < args.Length; i++)
                {
                    if (args[i] == "-o")
                    {
                        outFile = args[++i];
                        continue;
                    }
                }

                if (string.IsNullOrEmpty(outFile))
                {
                    Console.WriteLine("Need -o flag for output file.");
                    return;
                }

                Tokenizer tokenizer = new(content);
                Parser parser = new(tokenizer.Lex());
                Compiler compiler = new(parser.Parse());

                File.WriteAllBytes(outFile, compiler.Compile().ToArray());

                Environment.Exit(0);
            }
        }
    }
}
using ConcumaCompiler;
using ConcumaCompiler.Compiling;
using ConcumaCompiler.Lexing;
using ConcumaCompiler.Parsing;

namespace ConcumaCompilerTerminal
{
    static class Program
    {
        private static void Main(string[] args)
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
            List<Token> tokens = tokenizer.Lex();
            if (ErrorHandling.ThrowLexingExceptions()) return;
            Parser parser = new(tokens);
            List<Statement> statements = parser.Parse();
            if (ErrorHandling.ThrowParsingExceptions()) return;
            Compiler compiler = new(statements);

            File.WriteAllBytes(outFile, compiler.Compile().ToArray());
        }
    }
}
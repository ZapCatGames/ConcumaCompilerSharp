using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Compiling
{
    public sealed class ConcumaEnvironment
    {
        private readonly Dictionary<string, int> _symbols = new();
        private readonly ConcumaEnvironment? _parent;

        public ConcumaEnvironment(ConcumaEnvironment? parent)
        {
            _parent = parent;
        }

        public void Add(string name, int addr) => _symbols.Add(name, addr);
        public int Find(Token name)
        {
            if (_symbols.ContainsKey(name.Lexeme))
            {
                return _symbols[name.Lexeme];
            }

            if (_parent is null) throw new CompilerException(name.Line, "Tried to access unknown variable.");

            return _parent.Find(name);
        }
        public ConcumaEnvironment? Exit() => _parent;
    }
}

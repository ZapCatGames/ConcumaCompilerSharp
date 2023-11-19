using ConcumaCompiler.Lexing;

namespace ConcumaCompiler.Compiling
{
    public sealed class ConcumaEnvironment
    {
        private readonly Dictionary<string, int> _symbols = new();
        private readonly Dictionary<int, ConcumaEnvironment> _children = new();
        private readonly ConcumaEnvironment? _parent;

        public ConcumaEnvironment(int addr, ConcumaEnvironment? parent)
        {
            _parent = parent;
            _parent?.AddChild(addr, this);
        }

        public void AddChild(int addr, ConcumaEnvironment child)
        {
            _children.Add(addr, child);
        }

        public ConcumaEnvironment GetChild(int addr) => _children[addr];

        public void Add(string name, int addr) => _symbols.Add(name, addr);
        public int Find(Token name)
        {
            if (_symbols.TryGetValue(name.Lexeme, out int value))
            {
                return value;
            }

            if (_parent is null) throw new CompilerException(name.Line, "Tried to access unknown variable.");

            return _parent.Find(name);
        }
        public Dictionary<string, int> GetSymbols() => _symbols;
        public ConcumaEnvironment? Exit() => _parent;
    }
}

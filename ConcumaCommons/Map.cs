namespace ConcumaCommons
{
    public sealed class Map<T1, T2> where T1 : notnull where T2 : notnull
    {
        private readonly Dictionary<T1, T2> _forward = new();
        private readonly Dictionary<T2, T1> _backward = new();

        public void Add(T1 left, T2 right)
        {
            _forward.Add(left, right);
            _backward.Add(right, left);
        }

        public T2 this[T1 value]
        {
            get { return _forward[value]; }
        }

        public T1 this[T2 value]
        {
            get { return _backward[value]; }
        }
    }
}
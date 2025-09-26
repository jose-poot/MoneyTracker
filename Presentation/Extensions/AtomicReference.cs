
    public class AtomicReference<T> where T : class
    {
        private volatile T? _value;
        private readonly object _lock = new();

        public AtomicReference() { }

        public AtomicReference(T? initialValue)
        {
            _value = initialValue;
        }

        public T? GetAndSet(T? newValue)
        {
            lock (_lock)
            {
                var old = _value;
                _value = newValue;
                return old;
            }
        }

        public void Set(T? newValue)
        {
            lock (_lock)
            {
                _value = newValue;
            }
        }

        public T? Get()
        {
            return _value;
        }
    }

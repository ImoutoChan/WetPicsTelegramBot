using System;

namespace WetPicsTelegramBot.Helpers
{
    public class CircleList<T>
    {
        private readonly T[] _items;
        private int _nextIndex = 0;
        private readonly object _syncRoot = new object();

        public CircleList(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be more than or equal to 1.");
            }
            
            _items = new T[capacity];
        }

        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _items[_nextIndex] = item;

                _nextIndex++;
                if (_items.Length == _nextIndex)
                {
                    _nextIndex = 0;
                }
            }
        }

        public void Clear()
        {
            lock (_syncRoot)
            {
                Array.Clear(_items, 0, _items.Length);

                _nextIndex = 0;
            }
        }

        public bool Contains(T item)
        {
            lock (_syncRoot)
            {
                return Array.IndexOf(_items, item) != -1;
            }
        }

        public int Count => _items.Length;
    }
}
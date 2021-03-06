﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace WetPicsTelegramBot.WebApp.Helpers
{
    public class CircleList<T> : IEnumerable<T>
    {
        private readonly T[] _items;
        private int _nextIndex;
        private readonly object _syncRoot = new object();
        private int _version;

        public CircleList(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity should be more than or equal to 1.");

            _items = new T[capacity];
        }

        public void Add(T item)
        {
            lock (_syncRoot)
            {
                _version++;
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
                _version++;
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

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private struct Enumerator : IEnumerator<T>
        {
            private readonly CircleList<T> _list;
            private readonly int _version;
            private int _index;
            private T _current;

            internal Enumerator(CircleList<T> list)
            {
                _list = list;
                _index = 0;
                _version = list._version;
                _current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                CircleList<T> localList = _list;

                if (_version == localList._version && ((uint)_index < (uint)localList.Count))
                {
                    _current = localList._items[_index];
                    _index++;
                    return true;
                }
                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }

                _index = _list.Count + 1;
                _current = default(T);
                return false;
            }

            public T Current
            {
                get
                {
                    return _current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    if (_index == 0 || _index == _list.Count + 1)
                    {
                        throw new InvalidOperationException("InvalidOperation_EnumOpCantHappen");
                    }
                    return Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_version != _list._version)
                {
                    throw new InvalidOperationException("InvalidOperation_EnumFailedVersion");
                }

                _index = 0;
                _current = default(T);
            }
        }
    }
}
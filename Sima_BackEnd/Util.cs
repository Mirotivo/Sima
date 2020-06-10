using System;
using System.Collections;
using System.Collections.Generic;

namespace Simulator.Backend
{
    public class Deque<T> : IEnumerable<T>, ICollection, IEnumerable
    {
        public T this[int index]
        {
            get
            {
                if (index < 0 && index >= _front.Count)
                    throw new IndexOutOfRangeException("Index out of range");

                return _front[index];
            }

            set
            {
                if (index < 0 || index >= _front.Count)
                    throw new IndexOutOfRangeException("Index out of range");

                _front[index] = value;
            }
        }

        #region  Fields

        private List<T> _front;

        private List<T> _back;

        private int _frontDeleted;

        private int _backDeleted;

        #endregion

        #region Properties

        public int Capacity
        {
            get
            {
                return _front.Capacity + _back.Capacity;
            }
        }

        public int Count
        {
            get
            {
                return _front.Count + _back.Count - _frontDeleted - _backDeleted;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this.Count == 0;
            }
        }

        public IEnumerable<T> Reversed
        {
            get
            {
                if (_back.Count - _backDeleted > 0)
                {
                    for (int i = _back.Count - 1; i >= _backDeleted; i--)
                    {
                        yield return _back[i];
                    }
                }

                if (_front.Count - _frontDeleted > 0)
                {
                    for (int i = _frontDeleted; i < _front.Count; i++)
                    {
                        yield return _front[i];
                    }
                }
            }
        }

        #endregion

        #region Constructors

        public Deque()
        {
            _front = new List<T>();
            _back = new List<T>();
        }

        public Deque(int capacity)
        {
            if (capacity < 0)
            {
                throw new ArgumentException("Capacity cannot be negative");
            }

            int temp = capacity / 2;
            int temp2 = capacity - temp;
            _front = new List<T>(temp);
            _back = new List<T>(temp2);
        }

        public Deque(IEnumerable<T> backCollection)
            : this(backCollection, null)
        {
        }

        public Deque(IEnumerable<T> backCollection, IEnumerable<T> frontCollection)
        {
            if (backCollection == null && frontCollection == null)
            {
                throw new ArgumentException("Collections cannot both be null");
            }

            _front = new List<T>();
            _back = new List<T>();

            if (backCollection != null)
            {
                foreach (T item in backCollection)
                {
                    _back.Add(item);
                }
            }

            if (frontCollection != null)
            {
                foreach (T item in frontCollection)
                {
                    _front.Add(item);
                }
            }
        }

        #endregion

        #region API

        public virtual void Prepend(T item)
        {
            if (_frontDeleted > 0 && _front.Count == _front.Capacity)
            {
                _front.RemoveRange(0, _frontDeleted);
                _frontDeleted = 0;
            }

            _front.Add(item);
        }

        public virtual void Prepend(IEnumerable<T> range)
        {
            if (range != null)
            {
                foreach (T item in range)
                {
                    this.Prepend(item);
                }
            }
        }

        public virtual void Push(T item)
        {
            if (_backDeleted > 0 && _back.Count == _back.Capacity)
            {
                _back.RemoveRange(0, _backDeleted);
                _backDeleted = 0;
            }

            _back.Add(item);
        }

        public virtual void Push(IEnumerable<T> range)
        {
            if (range != null)
            {
                foreach (T item in range)
                {
                    this.Push(item);
                }
            }
        }

        public void Reverse()
        {
            List<T> temp = _front;
            _front = _back;
            _back = temp;
            int temp2 = _frontDeleted;
            _frontDeleted = _backDeleted;
            _backDeleted = temp2;
        }

        public T[] ToArray()
        {
            if (this.Count == 0) return new T[0];
            T[] result = new T[this.Count];
            this.CopyTo(result, 0);
            return result;
        }

        public void TrimExcess()
        {
            if (_frontDeleted > 0)
            {
                _front.RemoveRange(0, _frontDeleted);
                _frontDeleted = 0;
            }

            if (_backDeleted > 0)
            {
                _back.RemoveRange(0, _backDeleted);
                _backDeleted = 0;
            }

            _front.TrimExcess();
            _back.TrimExcess();
        }

        public bool TryPeekFirst(out T item)
        {
            if (!this.IsEmpty)
            {
                item = this.PeekFirst();
                return true;
            }

            item = default(T);
            return false;
        }

        public bool TryPeekLast(out T item)
        {
            if (!this.IsEmpty)
            {
                item = this.PeekLast();
                return true;
            }

            item = default(T);
            return false;
        }

        public bool TryPopFirst(out T item)
        {
            if (!this.IsEmpty)
            {
                item = this.PopFirst();
                return true;
            }

            item = default(T);
            return false;
        }

        public bool TryPopLast(out T item)
        {
            if (!this.IsEmpty)
            {
                item = this.PopLast();
                return true;
            }

            item = default(T);
            return false;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_front.Count - _frontDeleted > 0)
            {
                for (int i = _front.Count - 1; i >= _frontDeleted; i--)
                {
                    yield return _front[i];
                }
            }

            if (_back.Count - _backDeleted > 0)
            {
                for (int i = _backDeleted; i < _back.Count; i++)
                {
                    yield return _back[i];
                }
            }
        }

        public T PeekFirst()
        {
            if (_front.Count > _frontDeleted)
            {
                return _front[_front.Count - 1];
            }
            else if (_back.Count > _backDeleted)
            {
                return _back[_backDeleted];
            }
            else
            {
                throw new InvalidOperationException("Can't peek at empty Deque");
            }
        }

        public T PeekLast()
        {
            if (_back.Count > _backDeleted)
            {
                return _back[_back.Count - 1];
            }
            else if (_front.Count > _frontDeleted)
            {
                return _front[_frontDeleted];
            }
            else
            {
                throw new InvalidOperationException("Can't peek at empty Deque");
            }
        }

        public T PopFirst()
        {
            T result;

            if (_front.Count > _frontDeleted)
            {
                result = _front[_front.Count - 1];
                _front.RemoveAt(_front.Count - 1);
            }
            else if (_back.Count > _backDeleted)
            {
                result = _back[_backDeleted];
                _backDeleted++;
            }
            else
            {
                throw new InvalidOperationException("Can't pop empty Deque");
            }

            return result;
        }

        public T PopLast()
        {
            T result;

            if (_back.Count > _backDeleted)
            {
                result = _back[_back.Count - 1];
                _back.RemoveAt(_back.Count - 1);
            }
            else if (_front.Count > _frontDeleted)
            {
                result = _front[_frontDeleted];
                _frontDeleted++;
            }
            else
            {
                throw new InvalidOperationException("Can't pop empty Deque");
            }

            return result;
        }

        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("Array cannot be null");
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("Index cannot be negative");
            }
            if (array.Length < index + this.Count)
            {
                throw new ArgumentException("Index is invalid");
            }

            int i = index;

            foreach (T item in this)
            {
                array[i++] = item;
            }
        }

        public bool Contains(T item)
        {
            for (int i = _frontDeleted; i < _front.Count; i++)
            {
                if (Object.Equals(_front[i], item))
                {
                    return true;
                }
            }

            for (int i = _backDeleted; i < _back.Count; i++)
            {
                if (Object.Equals(_back[i], item))
                {
                    return true;
                }
            }

            return false;
        }

        public void Clear()
        {
            _front.Clear();
            _back.Clear();
            _frontDeleted = 0;
            _backDeleted = 0;
        }

        #endregion

        #region  Explicit property implementations

        bool ICollection.IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                return (ICollection)this;
            }
        }

        #endregion

        #region explicit method implementations

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo((T[])array, index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}
using System.Collections;

namespace Predictor.Framework
{
    class PushStack<T> : IEnumerable<T>
    {
        private int pointer;
        private int first;
        private T[] data;

        public int Size => data.Length;

        public PushStack(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.data = new T[size];
            this.pointer = -1;
            this.first = this.data.Length - 1;
        }

        public void Push(T item)
        {
            if (++this.pointer >= Size)
            {
                this.pointer = 0;
            }

            if (this.pointer == this.first)
            {
                if (++this.first >= Size)
                {
                    this.first = 0;
                }
            }

            this.data[this.pointer] = item;
        }

        public T Peak()
        {
            if (this.pointer < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return data[this.pointer];
        }

        public void Clear()
        {
            this.pointer = -1;
            this.first = Size - 1;
        }

        public int Count()
        {
            if (this.pointer < 0)
            {
                return 0;
            }

            if (this.first <= this.pointer)
            {
                return this.pointer - this.first + 1;
            }

            return (Size - this.first) + this.pointer + 1;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PushStackEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        class PushStackEnumerator : IEnumerator<T>
        {
            private int pointer;
            private readonly int first;
            private readonly int end;
            private readonly T[] data;

            internal PushStackEnumerator(PushStack<T> stack)
            {
                this.pointer = stack.first;
                this.first = stack.first;
                this.end = stack.pointer;
                this.data = stack.data;
            }

            public T Current => this.data[this.pointer];

            object? IEnumerator.Current => Current;

            public void Dispose()
            {
                this.pointer = -1;
            }

            public bool MoveNext()
            {
                if (this.pointer < 0 || this.end < 0)
                {
                    return false;
                }

                if (++this.pointer >= data.Length)
                {
                    this.pointer = 0;
                }

                return this.pointer != this.end;
            }

            public void Reset()
            {
                this.pointer = this.first;
            }
        }
    }
}

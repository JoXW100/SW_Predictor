using System.Collections;

namespace PredictorPatchFramework
{
    public class FixedSizePushStack<T> : IEnumerable<T>
    {
        private int pointer;
        private int first;
        private readonly T[] data;

        public int Size => data.Length;

        /// <summary>
        /// Creates a <see cref="PushStack"/>
        /// </summary>
        /// <param name="size">The number of items the push stack is limitied to contain, must be greater than 0.</param>
        /// <exception cref="ArgumentOutOfRangeException">If the size is outside the valid range.</exception>
        public FixedSizePushStack(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            this.data = new T[size];
            this.pointer = -1;
            this.first = this.data.Length - 1;
        }

        /// <summary>
        /// Pushes <paramref name="item"/> to the end of the stack, removing the leading item if the stack would exceed <see cref="Size"/>.
        /// </summary>
        /// <param name="item">The item to add to the stack</param>
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

        /// <summary>
        /// Gets the value of the first value in the stack.
        /// </summary>
        /// <returns>The first value in the stack.</returns>
        /// <exception cref="NullReferenceException">If no value exists in the stack.</exception>
        public T PeakFirst()
        {
            if (this.pointer < 0)
            {
                throw new NullReferenceException();
            }
            var ptr = this.first + 1;
            if (ptr >= Size)
            {
                ptr = 0;
            }
            return data[ptr];
        }

        /// <summary>
        /// Gets the value of the last value in the stack.
        /// </summary>
        /// <returns>The last value in the stack.</returns>
        /// <exception cref="NullReferenceException">If no value exists in the stack.</exception>
        public T PeakLast()
        {
            if (this.pointer < 0)
            {
                throw new NullReferenceException();
            }
            return data[this.pointer];
        }

        /// <summary>
        /// Clears the content of the stack.
        /// </summary>
        public void Clear()
        {
            this.pointer = -1;
            this.first = Size - 1;
        }

        /// <summary>
        /// Finds the number of item in the stack.
        /// </summary>
        /// <returns>The number of item in the stack</returns>
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

            internal PushStackEnumerator(FixedSizePushStack<T> stack)
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

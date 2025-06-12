using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace PowerStrings
{
    public unsafe struct PowerString : IDisposable, ICloneable, IEquatable<PowerString>, IComparable<PowerString>, IEnumerable<char>
    {
        private char* _heapBuffer = null;
        private int _length = 0;

        [Obsolete("PowerString cannot be instantiated directly. Use PowerString.From() instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public PowerString()
        {
            throw new Exception("PowerString cannot be instantiated directly. Use PowerString.From() instead.");
        }

        private PowerString(ReadOnlySpan<char> buffer)
        {
            try
            {
                if (buffer.Length == 0)
                {
                    _heapBuffer = null;
                    _length = 0;
                    return;
                }

                _heapBuffer = (char*)NativeMemory.Alloc((nuint)buffer.Length * sizeof(char));
                if (_heapBuffer == null)
                {
                    throw new OutOfMemoryException("Failed to allocate memory for PowerString heap buffer.");
                }
                buffer.CopyTo(new Span<char>(_heapBuffer, buffer.Length));

                _length = buffer.Length;
            }
            catch (Exception ex)
            {
                if (_heapBuffer != null)
                {
                    NativeMemory.Free(_heapBuffer);
                }

                throw new InvalidOperationException("Failed to create PowerString instance.", ex);
            }
        }

        public static PowerString From(in string str)
        {
            if (str is null)
            {
                throw new ArgumentException("String cannot be null.", nameof(str));
            }

            return From(str.AsSpan());
        }

        public static PowerString From(char[] array)
        {
            if (array is null)
            {
                throw new ArgumentException("Array cannot be null.", nameof(array));
            }

            return From(array.AsSpan());
        }

        public static PowerString From(ReadOnlySpan<char> span)
        {
            return new PowerString(span);
        }

        public static PowerString Empty()
        {
            return new PowerString([]);
        }

        public char this[int index]
        {
            get
            {
                if (index < 0 || index >= _length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                return _heapBuffer[index];
            }
            set
            {
                if (index < 0 || index >= _length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
                }
                _heapBuffer[index] = value;
            }
        }

        public readonly int Length => _length;
        public readonly bool IsEmpty => _length == 0;

        public readonly object Clone()
        {
            return From(new ReadOnlySpan<char>(_heapBuffer, _length));
        }

        public readonly int CompareTo(PowerString other)
        {
            if (_length == 0 && other._length == 0)
            {
                return 0;
            }

            if (_length == 0)
            {
                return -1;
            }

            if (other._length == 0)
            {
                return 1;
            }

            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);

            return thisSpan.SequenceCompareTo(otherSpan);
        }

        public readonly bool Equals(PowerString other)
        {
            return CompareTo(other) == 0;
        }

        public void Append(PowerString other)
        {
            Append(other.AsSpan());
        }

        public void Append(string other)
        {
            if (other is null)
            {
                throw new ArgumentException("String cannot be null.", nameof(other));
            }

            Append(other.AsSpan());
        }

        public void Append(char[] other)
        {
            if (other is null)
            {
                throw new ArgumentException("Array cannot be null.", nameof(other));
            }

            Append(other.AsSpan());
        }

        public void Append(ReadOnlySpan<char> other)
        {
            Insert(_length, other);
        }

        public void Prepend(PowerString other)
        {
            Prepend(other.AsSpan());
        }

        public void Prepend(string other)
        {
            if (other is null)
            {
                throw new ArgumentException("String cannot be null.", nameof(other));
            }
            Prepend(other.AsSpan());
        }

        public void Prepend(char[] other)
        {
            if (other is null)
            {
                throw new ArgumentException("Array cannot be null.", nameof(other));
            }
            Prepend(other.AsSpan());
        }

        public void Prepend(ReadOnlySpan<char> other)
        {
            Insert(0, other);
        }

        public void Insert(int index, PowerString other)
        {
            Insert(index, other.AsSpan());
        }

        public void Insert(int index, string other)
        {
            if (other is null)
            {
                throw new ArgumentException("String cannot be null.", nameof(other));
            }

            Insert(index, other.AsSpan());
        }

        public void Insert(int index, char[] other)
        {
            if (other is null)
            {
                throw new ArgumentException("Array cannot be null.", nameof(other));
            }

            Insert(index, other.AsSpan());
        }

        public void Insert(int index, ReadOnlySpan<char> other)
        {
            if (index < 0 || index > _length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range.");
            }

            if (other.Length == 0)
            {
                return;
            }

            var newLength = _length + other.Length;
            char* newBuffer = null;
            try
            {
                newBuffer = (char*)NativeMemory.Alloc((nuint)newLength * sizeof(char));

                if (newBuffer == null)
                {
                    throw new OutOfMemoryException("Failed to allocate memory for PowerString heap buffer.");
                }

                var newBufferSpan = new Span<char>(newBuffer, newLength);
                var thisSpan = new Span<char>(_heapBuffer, _length);

                thisSpan[..index].CopyTo(newBufferSpan);
                other.CopyTo(newBufferSpan[index..]);
                thisSpan[index..].CopyTo(newBufferSpan[(index + other.Length)..]);

                NativeMemory.Free(_heapBuffer);
                _heapBuffer = newBuffer;
                _length = newLength;
            }
            catch
            {
                if (newBuffer != null)
                {
                    NativeMemory.Free(newBuffer);
                }

                throw;
            }
        }

        public readonly bool Contains(PowerString other)
        {
            return Contains(other, StringComparison.CurrentCulture);
        }

        public readonly bool Contains(PowerString other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);
            return thisSpan.Contains(otherSpan, comparisonType);
        }

        public readonly bool Contains(char other)
        {
            return Contains(other, StringComparison.CurrentCulture);
        }

        public readonly bool Contains(char other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(&other, 1);
            return thisSpan.Contains(otherSpan, comparisonType);
        }

        public readonly bool EndsWith(PowerString other)
        {
            return EndsWith(other, StringComparison.CurrentCulture);
        }

        public readonly bool EndsWith(PowerString other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);
            return thisSpan.EndsWith(otherSpan, comparisonType);
        }

        public readonly bool EndsWith(char other)
        {
            return EndsWith(other, StringComparison.CurrentCulture);
        }

        public readonly bool EndsWith(char other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(&other, 1);
            return thisSpan.EndsWith(otherSpan, comparisonType);
        }

        public readonly bool StartsWith(PowerString other)
        {
            return StartsWith(other, StringComparison.CurrentCulture);
        }

        public readonly bool StartsWith(PowerString other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);
            return thisSpan.StartsWith(otherSpan, comparisonType);
        }

        public readonly bool StartsWith(char other)
        {
            return StartsWith(other, StringComparison.CurrentCulture);
        }

        public readonly bool StartsWith(char other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(&other, 1);
            return thisSpan.StartsWith(otherSpan, comparisonType);
        }

        public readonly int IndexOf(PowerString other)
        {
            return IndexOf(other, StringComparison.CurrentCulture);
        }

        public readonly int IndexOf(PowerString other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);
            return thisSpan.IndexOf(otherSpan, comparisonType);
        }

        public readonly int IndexOf(char other)
        {
            return IndexOf(other, StringComparison.CurrentCulture);
        }

        public readonly int IndexOf(char other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(&other, 1);
            return thisSpan.IndexOf(otherSpan, comparisonType);
        }

        public readonly int LastIndexOf(PowerString other)
        {
            return LastIndexOf(other, StringComparison.CurrentCulture);
        }

        public readonly int LastIndexOf(PowerString other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(other._heapBuffer, other._length);
            return thisSpan.LastIndexOf(otherSpan, comparisonType);
        }

        public readonly int LastIndexOf(char other)
        {
            return LastIndexOf(other, StringComparison.CurrentCulture);
        }

        public readonly int LastIndexOf(char other, StringComparison comparisonType)
        {
            var thisSpan = new ReadOnlySpan<char>(_heapBuffer, _length);
            var otherSpan = new ReadOnlySpan<char>(&other, 1);
            return thisSpan.LastIndexOf(otherSpan, comparisonType);
        }

        public void Replace(PowerString oldValue, PowerString newValue)
        {
            if (oldValue.IsEmpty)
            {
                throw new ArgumentException("Old value cannot be empty.", nameof(oldValue));
            }

            if (newValue.IsEmpty)
            {
                throw new ArgumentException("New value cannot be empty.", nameof(newValue));
            }

            int index;
            if (oldValue.Length == newValue.Length)
            {
                index = IndexOf(oldValue);
                while (index != -1)
                {
                    var thisSpan = new Span<char>(_heapBuffer, _length);
                    newValue.AsSpan().CopyTo(thisSpan.Slice(index, newValue.Length));
                    var newIndex = thisSpan[index..].IndexOf(oldValue.AsSpan());

                    if (newIndex == -1)
                    {
                        break;
                    }

                    index += newIndex + newValue.Length;
                }
                return;
            }

            index = IndexOf(oldValue);
            while (index != -1)
            {
                var newLength = _length - oldValue.Length + newValue.Length;
                char* newBuffer = null;
                try
                {
                    newBuffer = (char*)NativeMemory.Alloc((nuint)newLength * sizeof(char));

                    if (newBuffer == null)
                    {
                        throw new OutOfMemoryException("Failed to allocate memory for PowerString heap buffer.");
                    }

                    var newBufferSpan = new Span<char>(newBuffer, newLength);
                    var thisSpan = new Span<char>(_heapBuffer, _length);

                    thisSpan[..index].CopyTo(newBufferSpan);
                    newValue.AsSpan().CopyTo(newBufferSpan[index..]);
                    thisSpan[(index + oldValue.Length)..].CopyTo(newBufferSpan[(index + newValue.Length)..]);

                    NativeMemory.Free(_heapBuffer);
                    _heapBuffer = newBuffer;
                    _length = newLength;

                    index = IndexOf(oldValue, StringComparison.CurrentCulture);
                }
                catch
                {
                    if (newBuffer != null)
                    {
                        NativeMemory.Free(newBuffer);
                    }
                    throw;
                }
            }
        }

        public void Replace(char oldValue, char newValue)
        {
            if (oldValue == newValue)
            {
                return;
            }

            for (int i = 0; i < _length; i++)
            {
                if (_heapBuffer[i] == oldValue)
                {
                    _heapBuffer[i] = newValue;
                }
            }
        }

        public void ToLower()
        {
            if (_length == 0)
            {
                return;
            }

            for (int i = 0; i < _length; i++)
            {
                _heapBuffer[i] = char.ToLower(_heapBuffer[i]);
            }
        }

        public void ToUpper()
        {
            if (_length == 0)
            {
                return;
            }
            for (int i = 0; i < _length; i++)
            {
                _heapBuffer[i] = char.ToUpper(_heapBuffer[i]);
            }
        }

        public override readonly string ToString()
        {
            if (_length == 0)
            {
                return string.Empty;
            }

            return new string(_heapBuffer, 0, _length);
        }

        public char[] ToCharArray()
        {
            if (_length == 0)
            {
                return [];
            }

            var array = new char[_length];
            new ReadOnlySpan<char>(_heapBuffer, _length).CopyTo(array);

            return array;
        }

        public readonly ReadOnlySpan<char> AsSpan()
        {
            return new ReadOnlySpan<char>(_heapBuffer, _length);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public readonly IEnumerator<char> GetEnumerator()
        {
            return new PowerStringEnumerator(this);
        }

        public static implicit operator PowerString(string str)
        {
            return From(str);
        }

        public static implicit operator PowerString(char[] array)
        {
            return From(array);
        }

        public static implicit operator PowerString(ReadOnlySpan<char> span)
        {
            return From(span);
        }

        public void Dispose()
        {
            if (_heapBuffer != null)
            {
                NativeMemory.Free(_heapBuffer);
                _heapBuffer = null;
                _length = 0;
            }
        }
    }
}

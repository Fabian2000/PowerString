using System.Collections;

namespace PowerStrings
{
    internal struct PowerStringEnumerator : IEnumerator<char>
    {
        private readonly PowerString _powerString;
        private int _currentIndex = -1;
        public PowerStringEnumerator(PowerString powerString)
        {
            _powerString = powerString;
        }

        public bool MoveNext()
        {
            if (_currentIndex < _powerString.Length - 1)
            {
                _currentIndex++;
                return true;
            }
            return false;
        }

        public readonly char Current => _powerString[_currentIndex];

        readonly object IEnumerator.Current => Current;

        public void Reset()
        {
            _currentIndex = -1;
        }

        public readonly void Dispose()
        {
        }
    }
}

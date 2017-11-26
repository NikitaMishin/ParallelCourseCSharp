using System.Threading;

namespace AsyncAwaitTask
{
    public class Counter
    {
        private int _index = 0;
        public int GetCounter() => _index;
        public int Increment() => Interlocked.Increment(ref _index);
        public int Decrement() => Interlocked.Decrement(ref _index);

        public void Reset()
        {
            _index = 0;
        }
    }
}
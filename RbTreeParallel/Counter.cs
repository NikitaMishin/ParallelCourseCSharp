using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace RbTreeParallel
{
    internal sealed class Counter
    {
        public volatile int Current = 0;
        public int Next() => Interlocked.Increment(ref Current);
        public int Prev() => Interlocked.Decrement(ref Current);
        public void Reset()
        {
            Current = 0;
        }
    }
}
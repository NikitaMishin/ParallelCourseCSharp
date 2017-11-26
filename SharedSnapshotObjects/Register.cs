using System;
using System.Linq;
using System.Reflection.Emit;

namespace AtomicSnapshot
{
    public class Register //where TV:new()
    {
        public int Value;
        public bool Toogle;
        public int[] SnapShot; //vector
        public bool[] P;
        public readonly int Size;

        public Register(int size = 4)
        {
            Size = size;
            Value = default(int);
            Toogle = true;
            SnapShot = Enumerable.Repeat(default(int), size).ToArray();
            P = Enumerable.Repeat(default(bool), size).ToArray();
        }

        public Register Copy()
        {
            var copy = new Register(Size);
            Array.Copy(P, copy.P, Size);
            copy.Toogle = Toogle;
            copy.Value = Value;
            Array.Copy(SnapShot, copy.SnapShot, Size);
            return copy;
        }
    }
}
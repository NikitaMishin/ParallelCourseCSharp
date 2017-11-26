using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;


namespace AtomicSnapshot
{
    // bounded version
    public class SingleWrMultRdr
    {
        private object _writerLock = new object();
        public readonly int Readers;
        private Register[] _registers;
        private bool[,] _q;

        public SingleWrMultRdr(int readers = 4)
        {
            Readers = readers;
            _registers = new Register[Readers];
            _q = new bool[Readers, Readers];
            for (var i = 0; i < Readers; i++)
            {
                for (int j = 0; j < Readers; j++)
                {
                    _q[i, j] = default(bool);
                }
            }

            for (var i = 0; i < Readers; i++)
                _registers[i] = new Register(size: Readers);
        }

        public int[] ScanI(int registerIndex)
        {
            var i = registerIndex;
            var moved = Enumerable.Repeat(default(bool), Readers);
            while (true)
            {
                for (int j = 0; j < Readers; j++)
                    _q[i, j] = _registers[i].P[j]; //sure??
                var a = Collect();
                var b = Collect();
                //TODO 
                throw new NotImplementedException();
            }
        }

        public void UpdateI(int registerIndex, int value)
        {
            var i = registerIndex;
            var f = new bool[Readers];
            for (var j = 1; j < Readers; j++)
            {
                f[j] = !_q[j, i];
            }
            var snapShot = ScanI(i);
            _registers[i].Value = value;
            _registers[i].Toogle = !_registers[i].Toogle;
            _registers[i].P = f;
            _registers[i].SnapShot = snapShot;
        }

        private Register[] Collect() //aka return copy
        {
            var registersCopy = new Register[Readers];
            for (int i = 0; i < Readers; i++)
            {
                registersCopy[i] = _registers[i].Copy();
            }
            return registersCopy;
        }
    }
}
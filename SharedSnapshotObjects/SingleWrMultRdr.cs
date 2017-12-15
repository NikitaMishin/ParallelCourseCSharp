using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Messaging;


namespace AtomicSnapshot
{
    // bounded version
    public class SingleWrMultRdr
    {
        // private object _writerLock = new object();
        public readonly int Readers;

        private readonly Register[] _registers;
        private readonly bool[,] _q;
        private Stopwatch _timer = new Stopwatch();
        private Dictionary<TimeSpan, int[]> _loggerReader = new Dictionary<TimeSpan, int[]>(100);
        private Dictionary<TimeSpan, int[]> _loggerWriter = new Dictionary<TimeSpan, int[]>(100);

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
            _timer.Start();

            for (var i = 0; i < Readers; i++)
                _registers[i] = new Register(size: Readers);
        }

        public int[] ScanI(int registerIndex, bool isFromUpdateI = false)
        {
            var i = registerIndex;
            var moved = Enumerable.Repeat(default(bool), Readers).ToArray();
            while (true)
            {
                for (var j = 0; j < Readers; j++) //HandShake
                    _q[i, j] = _registers[j].P[i];
                var a = Collect();
                var b = Collect();
                var status = true;
                for (var k = 0; k < Readers; k++)
                {
                    if (a[k].P[i] == b[k].P[i] && a[k].P[i] == _q[i, k] && a[k].Toogle == b[k].Toogle) continue;
                    status = false;
                    break;
                }
                if (status)
                {
                    var result = b.Select((register, i1) => register.Value).ToArray();
                    if (isFromUpdateI) _loggerReader[_timer.Elapsed] = result;
                    return result;
                }
                for (var j = 0; j < Readers; j++)
                {
                    if (a[j].P[i] == _q[i, j] && b[j].P[i] == _q[i, j] && a[j].Toogle == b[j].Toogle) continue;
                    if (moved[j])
                    {
                        return b[j].SnapShot;
                    }
                    moved[j] = !moved[j];
                }
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
            _loggerWriter[_timer.Elapsed] = snapShot;
            _registers[i].Toogle = !_registers[i].Toogle;
            _registers[i].P = f;
            _registers[i].SnapShot = snapShot;
        }

        private Register[] Collect() //aka return copy
        {
            var registersCopy = new Register[Readers];
            for (var i = 0; i < Readers; i++)
            {
                registersCopy[i] = _registers[i].Copy();
            }
            return registersCopy;
        }

        public void Trace()
        {
            Console.WriteLine("Trace:\nLogReader");
            foreach (var elem in _loggerReader)
            {
                Console.WriteLine("[{2},{0},{1}]",elem.Value[0],elem.Value[1],elem.Key);
            }
            Console.WriteLine("Trace:\nLogWriter");
            foreach (var elem in _loggerWriter)
            {
                Console.WriteLine("[{2},{0},{1}]",elem.Value[0],elem.Value[1],elem.Key);
            }
        }
    }
}
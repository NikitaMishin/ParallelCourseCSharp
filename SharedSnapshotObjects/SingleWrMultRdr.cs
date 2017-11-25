using System;
using System.Collections.Generic;
using System.Linq;

namespace AtomicSnapshot
{
    // bounded version
    public class SingleWrMultRdr<TV>
    {
        private object _writerLock = new object();
        public readonly int Readers;
        private Register<TV>[] _registers;
        
        public SingleWrMultRdr(int readers = 4)
        {
            Readers = readers;
            //_registers = Enumerable.Repeat(default(TV), Readers).ToArray();//stored default values
        }
        
        public TV Scan()
        {
            throw  new NotImplementedException();
        }
        public void Update(int registerIndex, TV value)
        {
            throw  new NotImplementedException();
        }
       
    }
}
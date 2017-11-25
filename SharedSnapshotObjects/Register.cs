namespace AtomicSnapshot
{
    public struct Register<TV>
    {
        public TV Value;
        public bool toogle;
        public TV[] snapShot;
        public bool[] p;
    }
}
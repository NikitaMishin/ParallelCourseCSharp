using System;

namespace RbTreeParallel
{
    public interface ITree <in TK,TV> where TK: IComparable<TK>
    {
        bool Insert(TK key, TV value);
        bool Delete(TK key);
        TV Search(TK key);//return default but be carefull!
    }
}
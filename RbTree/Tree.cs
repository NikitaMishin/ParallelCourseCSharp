using System;

namespace RbTreeParallel
{
    public interface ITree <in K,V> where K: IComparable<K>
    {
        bool Insert(K key, V value);
        bool Delete(K key);
        V Search(K key);//return default but be carefull!
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace RbTreeParallel
{
    /*
        _hashMap store  Key:(Value,_counter.Current,Action)
        _counter responsible for timeline
        _dictSise responsible for size of _hashMap
    */
    public class RbTreeParallel<TK, TV> : ITree<TK, TV> where TK : IComparable<TK>
    {
        private readonly RbTree<TK, TV> _rbTree;
        private volatile static int _lockUpdateFlag = 0;
        private volatile object _insertDeletionLock = new object();
        private readonly Dictionary<TK, Tuple<TV, int, Operation>> _hashMap;
        private volatile Counter _counter = new Counter();
        private volatile Counter _dictSize = new Counter();
        private readonly int _bunchSize;

        public void PrintRbTree()
        {
            lock (_rbTree) //or make update and after that print>?
            {
                _rbTree.PrintTree(_rbTree.GetRoot());
            }
        }

        public RbTreeParallel(RbNode<TK, TV> root = null, int bunchSize = 1000)
        {
            if (bunchSize < 50) bunchSize = 1000;
            _rbTree = new RbTree<TK, TV>();
            _hashMap = new Dictionary<TK, Tuple<TV, int, Operation>>(100000);
            _bunchSize = bunchSize;
            _counter.Next();
            if (root == null)
            {
                return;
            }
            _rbTree.Insert(root.Key, root.Value);
        }

        private async void UpdateAsync()
        {
            if (Interlocked.CompareExchange(ref _lockUpdateFlag, 1, 0) != 0) return;
            lock (_hashMap)
            {
                Console.WriteLine("hello");
                _counter.Reset();
                foreach (var tuple in _hashMap)
                {
                    switch (tuple.Value.Item3)
                    {
                        case Operation.NoAction:
                            _rbTree.Replace(tuple.Key, tuple.Value.Item1);
                            break;
                        case Operation.Insert:
                            _rbTree.Insert(tuple.Key, tuple.Value.Item1);
                            break;
                        case Operation.Delete:
                            _rbTree.Delete(tuple.Key);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                _hashMap.Clear();
                _dictSize.Reset();
                _counter.Next();
                Interlocked.Decrement(ref _lockUpdateFlag);
            }
        }

        private async Task<bool> DeleteAsync(TK key)
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                var flag = 0;
                while (_lockUpdateFlag == 1)
                {
                    flag = 1;
                }
                var old = _counter.Current;
                if (flag == 1)
                {
                    searchInTree = SearchAsync(key, _rbTree);
                    await searchInTree;
                }
                lock (_insertDeletionLock)
                {
                    if (_hashMap.ContainsKey(key))
                    {
                        var tuple = _hashMap[key];
                        if (old >= tuple.Item2)
                        {
                            if (searchInTree.Result == null)
                            {
                                switch (tuple.Item3)
                                {
                                    case Operation.NoAction:
                                        flag = -1;
                                        break;
                                    case Operation.Insert:
                                        _hashMap[key] = new Tuple<TV, int, Operation>(tuple.Item1, _counter.Current, 0);
                                        flag = -1;
                                        break;
                                    case Operation.Delete:
                                        //it's error - impssiible so i think try again
                                        // _hashMap[key] = new Tuple<TV, int, int>(tuple.Item1,_counter.Current,);
                                        break;
                                }
                            }
                            else
                            {
                                switch (tuple.Item3)
                                {
                                    case Operation.NoAction:
                                        _hashMap[key] = new Tuple<TV, int, Operation>(tuple.Item1, _counter.Current,
                                            Operation.Delete);
                                        break;
                                    case Operation.Insert:
                                        _hashMap[key] = new Tuple<TV, int, Operation>(tuple.Item1, _counter.Current,
                                            Operation.NoAction);
                                        break;
                                    case Operation.Delete:
                                        _hashMap[key] = new Tuple<TV, int, Operation>(tuple.Item1, _counter.Current,
                                            Operation.Delete);
                                        break;
                                }
                                flag = -1;
                            }
                        }
                    }
                    else
                    {
                        if (searchInTree.Result != null)
                        {
                            _hashMap[key] = new Tuple<TV, int, Operation>(searchInTree.Result, _counter.Current,
                                Operation.Delete);
                            if (_dictSize.Next() >= _bunchSize) UpdateAsync();
                        }
                        //or do nothin
                        flag = -1;
                    }
                }
                if (flag != -1) continue;
                _counter.Next();
                return true;
            }
        }

        private async Task<bool> InsertAsync(TK key, TV value)
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                var flag = 0;
                while (_lockUpdateFlag == 1)
                {
                    flag = 1;
                }
                var old = _counter.Current;
                if (flag == 1)
                {
                    searchInTree = SearchAsync(key, _rbTree);
                    await searchInTree;
                }
                lock (_insertDeletionLock)
                {
                    if (_hashMap.ContainsKey(key))
                    {
                        var tuple = _hashMap[key];

                        if (old >= tuple.Item2)
                        {
                            _hashMap[key] = new Tuple<TV, int, Operation>(value, _counter.Current, Operation.Insert);
                            flag = -1;
                        }
                    }
                    else
                    {
                        _hashMap[key] = new Tuple<TV, int, Operation>(value, _counter.Current, Operation.Insert);
                        if (_dictSize.Next() >= _bunchSize) UpdateAsync();
                        flag = -1;
                    }
                }
                if (flag != -1) continue;
                _counter.Next();
                return true;
            }
        }

        private async Task<TV> SearchAsync(TK key, RbTree<TK, TV> tree)
        {
            //old = val then optimistic    
            var old = tree[key];
            while (true)
            {
                var cur = tree[key];
                if (old == null && cur == null)
                {
                    return default(TV);
                }
                if (old != null && old.Equals(cur))
                {
                    return cur; // cannot do this new TV(cur)
                }
                old = cur;
            }
        }

        private async Task<Tuple<TV, int, Operation>> SearchAsync(TK key,
            Dictionary<TK, Tuple<TV, int, Operation>> dict)
        {
            Tuple<TV, int, Operation> old;
            try
            {
                old = dict[key];
            }
            catch (KeyNotFoundException e)
            {
                old = null;
            }
            while (true)
            {
                Tuple<TV, int, Operation> cur;
                try
                {
                    cur = dict[key];
                }
                catch (KeyNotFoundException e)
                {
                    cur = null;
                }
                if (old == null && cur == null)
                {
                    return null;
                }
                if (old != null && old.Equals(cur))
                {
                    return new Tuple<TV, int, Operation>(old.Item1, old.Item2, old.Item3);
                }
                old = cur;
            }
        }

        public bool Delete(TK key) => DeleteAsync(key).Result;

        private async Task<TV> SearchAsync(TK key)
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                var flag = 0;
                while (_lockUpdateFlag == 1)
                {
                    flag = 1;
                }

                if (flag == 1)
                {
                    searchInTree = SearchAsync(key, _rbTree);
                    await searchInTree;
                }

                var searchInDict = await SearchAsync(key, _hashMap);
                var item1 = searchInDict == null ? default(TV) : searchInDict.Item1;
                var item2 = searchInDict?.Item2; //version
                var item3 = searchInDict?.Item3; // action
                var value = searchInTree.Result; //value stored in Tree
                if ((searchInDict != null) && (!(_counter.Current >= item2))) continue;
                switch (item3)
                {
                    case null:
                        return value; //no in dictionary so return just value from Tree whather it.s null or TV
                    case Operation.NoAction: //no action
                        return value;
                    case Operation.Insert:
                        return item1; //action is isnsert that mean that on server lay old value by key
                    case Operation.Delete: //action is deleted
                        return default(TV); //so return null
                }
            }
        }

        public TV Search(TK key) => SearchAsync(key).Result;
        public bool Insert(TK key, TV value) => InsertAsync(key, value).Result;
    }
}
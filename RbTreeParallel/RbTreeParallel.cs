using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;


namespace RbTreeParallel
{
    public class RbTreeParallel<TK, TV> : ITree<TK, TV> where TK : IComparable<TK>
    {
        private RbTree<TK, TV> _rbTree;
        private static volatile int _lockUpdateFlag = 0;
        private volatile object _insertDeletionLock = new object();

        private volatile Dictionary<TK, Tuple<TV, int, Operation>> _hashMap =
            new Dictionary<TK, Tuple<TV, int, Operation>>(100000);

        //private volatile Counter _updated = new Counter(); 
        private volatile Counter _counter = new Counter();

        private volatile Counter _dictSize = new Counter();
        private readonly int _bunchSize;

        public RbTreeParallel(RbNode<TK, TV> root = null, int bunchSize = 1000)
        {
            if (bunchSize < 50) bunchSize = 1000;
            _rbTree = new RbTree<TK, TV>();
            _bunchSize = bunchSize;
            //_updated.Next();
            _counter.Next();
            if (root == null)
            {
                return;
            }
            _rbTree.Insert(root.Key, root.Value);
        }

        /* description async Task<bool> UpdateAsync:
            Extend Tree by hashMap
            Clear hashMap
            Set  _counter = 0
            Set _updated = 1
        */
        private async void UpdateAsync()
        {
            if (Interlocked.CompareExchange(ref _lockUpdateFlag, 1, 0) != 0) return;
            // only 1 thread will enter here without locking the object/put the
            // other threads RETURN 
            lock (_hashMap) //to avoid problems like _hashMap[notExistingKey] in Search
            {
                //maybe add lock to rbTree???
                //_updated.Reset();
                _counter.Reset();
                foreach (var tuple in _hashMap)
                {
                    switch (tuple.Value.Item3)
                    {
                        case Operation.NoAction:
                            _rbTree.Replace(tuple.Key, tuple.Value.Item1); //insert if no or replace value 
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
                _counter.Next();
                _hashMap.Clear();
                _dictSize.Reset();
                Interlocked.Decrement(ref _lockUpdateFlag);
                //_updated.Next();
            }
        }

        /*descrirption async Task<bool> DeleteAsync:
            Search in Tree, _hashMap
            Insert in _hashMap or change state if needed
            update counters
            Launch Update if needed
        */
        private async Task<bool> DeleteAsync(TK key) //
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                //var old = _updated.Current;
                var flag = 0;
                //block of searching in tree
                while (_lockUpdateFlag == 1)
                    //while (old == 0 || old < _updated.Current) //tree is in update  right know 
                {
                    //    old = _updated.Current;
                    flag = 1;
                } //guarantee that on that moment in tree no updates -actual info

                var old = _counter.Current;
                //cover case when tree updated//cas on  _updateFlag
                if (flag == 1) searchInTree = SearchAsync(key, _rbTree); //remember about this task
                if (flag == 1) await searchInTree; //filth place
                lock (_insertDeletionLock)
                {
                    try
                    {
                        var tuple = _hashMap[key];
                        if (old >= tuple.Item2) //or cirrent.Current
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
                    catch (KeyNotFoundException e)
                    {
                        if (searchInTree.Result != null)
                        {
                            _hashMap[key] = new Tuple<TV, int, Operation>(searchInTree.Result, _counter.Current,
                                Operation.Delete);
                            if (_dictSize.Next() >= _bunchSize) UpdateAsync(); //with or without await
                        }
                        //or do nothing
                        flag = -1;
                    }
                }
                if (flag == -1)
                {
                    _counter.Next();
                    return true;
                }
            }
        }

        /*
         description async Task <bool> InsertAsync:
         base
             Search in Tree, _hashMap
             Insert in  _hashMap or do Nothing if needed or change state
             update counters
             Launch Update if needed
        */
        private async Task<bool> InsertAsync(TK key, TV value)
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                //var node = new Tuple<TV, int, int>(value, _counter.Current, 1);
                //var old = _updated.Current;
                var flag = 0;
                //block of searching in tree

                while (_lockUpdateFlag == 1)
                    //while (old == 0 || old < _updated.Current) //tree is in update  right know 
                {
                    //old = _updated.Current;
                    flag = 1;
                } //guarantee that on that moment in tree no updates -actual info
                var old = _counter.Current;
                //cover case when tree updated//cas on  _updateFlag
                if (flag == 1) searchInTree = SearchAsync(key, _rbTree); //remember about this task
                if (flag == 1) await searchInTree; //filth place

                lock (_insertDeletionLock)
                {
                    try
                    {
                        var tuple = _hashMap[key];
                        if (old >= tuple.Item2) //or cirrent.Current
                        {
                            _hashMap[key] = new Tuple<TV, int, Operation>(value, _counter.Current, Operation.Insert);
                            flag = -1; //???????????????//
                        }
                    }
                    catch (KeyNotFoundException e)
                    {
                        _hashMap[key] = new Tuple<TV, int, Operation>(value, _counter.Current, Operation.Insert);
                        if (_dictSize.Next() >= _bunchSize) UpdateAsync(); //with or without await
                        flag = -1;
                    }
                }
                if (flag == -1)
                {
                    _counter.Next();
                    return true;
                }
            }
        }

        /* descrirption async SearchAsync:
            Search in Tree, _hashMap
            Return value or default(TV)
        */
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
                //var old = _updated.Current;
                var flag = 0;
                while (_lockUpdateFlag == 1)
                    //while (old == 0 || old < _updated.Current) //tree is in update  right know
                {
                    //old = _updated.Current;
                    flag = 1;
                } //guarantee that on that moment in tree no updates -actual info

                if (flag == 1) //cover case when tree updated
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
                // old < _update.Curernt or no need???
                //safe place
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
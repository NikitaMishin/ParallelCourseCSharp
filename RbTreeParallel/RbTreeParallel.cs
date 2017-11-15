using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;



// 0 - no acition
// 1 - insert
// 2 - deletion
namespace RbTreeParallel
{
    public class RbTreeParallel<TK, TV> : ITree<TK, TV> where TK : IComparable<TK>
    {
        private RbTree<TK, TV> _rbTree; //-1 do nothing empty

        private volatile Dictionary<TK, Tuple<TV, int, int>> _hashMap = new Dictionary<TK, Tuple<TV, int, int>>(100000);
        //TK:{TV,index,-1/0/1-del or ins}//also set capacity

        private volatile Counter _updated = new Counter(); //if 0 then must be update
        private volatile Counter _counter = new Counter();
        private int _bunchSize;

        public RbTreeParallel(RbNode<TK, TV> root = null, int bunchSize = 100)
        {
            _rbTree = new RbTree<TK, TV>();
            _bunchSize = bunchSize;
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
        private async Task<bool> UpdateAsync()
        {
            _updated.Reset();
            foreach (var tuple in _hashMap)
            {
                switch (tuple.Value.Item3)
                {
                    case 1:
                        _rbTree.Replace(tuple.Key, tuple.Value.Item1);//insert if no or replace value 
                        break;
                    case 2:
                        _rbTree.Delete(tuple.Key);
                        break;
                    default:break;
                }
                _counter.Next();
                _hashMap.Clear();
                _updated.Next();
            }
            return true;
        }

        /*descrirption async Task<bool> DeleteAsync:
            Search in Tree, _hashMap
            Insert in _hashMap or change state if needed
            update counters
            Launch Update if needed
        */
        private async Task<bool> DeleteAsync(TK key)
        {
            //await SearchAsync(key,_rbTree);
            throw new NotImplementedException();
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
            
            
            
            throw new NotImplementedException();
        }

        /* descrirption async SearchAsync:
            Search in Tree, _hashMap
            Return value or default(TV)
        */
        private async Task<TV> SearchAsync(TK key, RbTree<TK, TV> tree)
        {
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

        private async Task<Tuple<TV, int, int>> SearchAsync(TK key, Dictionary<TK, Tuple<TV, int, int>> dict)
        {
            Tuple<TV, int, int> old = null;
            if (dict.ContainsKey(key)) //to avoid exeception
            {
                old = dict[key];
            }

            if (old == null && !dict.ContainsKey(key)) //nothing changed and null
            {
                return null;
            }

            old = dict[key];
            while (true)
            {
                var cur = dict[key];
                if (old == null && cur == null)
                {
                    return null;
                }
                if (old != null && old.Equals(cur))
                {
                    return new Tuple<TV, int, int>(cur.Item1, cur.Item2, cur.Item3);
                }
                old = cur;
            }
        }


        public bool Delete(TK key)
        {
            throw new NotImplementedException();
        }

        private async Task<TV> SearchAsync(TK key)
        {
            var searchInTree = SearchAsync(key, _rbTree);
            await searchInTree;
            while (true)
            {
                var old = _updated.Current;
                var flag = 0;

                //block of searching in tree
                while (old == 0 || old < _updated.Current) //tree is in update  right know
                {
                    old = _updated.Current;
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

                //safe place
                switch (item3)
                {
                    case null:
                        return value; //no in dictionary so return just value from Tree whather it.s null or TV
                    case 0: //no action
                        return value;
                    case 1:
                        return item1; //action is isnsert that mean that on server lay old value by key
                    case 2: //action is deleted
                        return default(TV);
                }
            }
        }
        public TV Search(TK key) => SearchAsync(key).Result;
        public bool Insert(TK key, TV value) => InsertAsync(key, value);
    }
}
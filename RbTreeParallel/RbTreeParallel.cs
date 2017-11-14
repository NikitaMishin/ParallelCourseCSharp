using System;
using System.Threading.Tasks;

namespace RbTreeParallel
{
    public class RbTreeParallel<TK, TV> : ITree<TK, TV> where TK : IComparable<TK>
    {
        private RbNode<TK, TV> _root;
        private RbNode<TK, TV> _rootInsertionPool = null;
        private RbNode<TK, TV> _rootDeletionPool = null;
        private volatile int _insertionPoolSize = 0;
        private volatile int _deletionPoolSize = 0;
        private volatile bool _updated = true; //flag
        private volatile int _index = 0;
        private int _bunchSize;

        public RbTreeParallel(RbNode<TK, TV> root = null, int bunchSize = 100)
        {
            _bunchSize = bunchSize;
            _root = root;
        }

        private async Task<int> GetIdAsync()
        {
            throw new NotImplementedException();
        }

        private async Task<int> IncIdAsync()
        {
            //cas
            throw new NotImplementedException();
        }

        /* description async Task<bool> UpdateAsync:
            Extend Tree by DeletionPool and InsertionPool
            Clear InsertionPool and DeletionPool
            Set index = 0,deletionPoolSize = 0,insetionPoolSize=0
        */
        private async Task<bool> UpdateAsync()
        {
            throw new NotImplementedException();
        }

        /*descrirption async Task<bool> DeleteAsync:
            Search in Tree, DeletionPool,InsertionPool
            Insert in deletionPool or delete from InsertionPool or do Nothing if needed
            Launch Update if needed
        */
        private async Task<bool> DeleteAsync(TK key)
        {
            throw new NotImplementedException();
        }

        /*
         description async Task <bool> InsertAsync:
         base
             Search in Tree,InsertionPool,DeletionPool
             Insert in  InsertionPool or delete from InsertionPool or do Nothing if needed
             Launch Update if needed
        */
        private async Task<bool> InsertAsync(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        /* descrirption async Task<bool> SearchAsync:
            Search in Tree,Deletion,Insertion
            Return value if needed
        */
        private async Task<bool> SearchAsync(TK key)
        {
            throw new NotImplementedException();
        }

        public bool Insert(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        public bool Delete(TK key)
        {
            throw new NotImplementedException();
        }

        public TV Search(TK key)
        {
            throw new NotImplementedException();
        }

        private bool MergeTrees()
        {
            throw new NotImplementedException();
        }
    }
}
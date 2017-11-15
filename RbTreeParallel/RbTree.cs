using System;
using System.Collections.Generic;

namespace RbTreeParallel
{
    public class RbTree<TK, TV> : ITree<TK, TV> where TK : IComparable<TK>
    {
        private RbNode<TK, TV> _root;

        public RbNode<TK,TV> GetRoot()
        {
            return _root;
        }

        public RbTree(RbNode<TK, TV> root = null)
        {
            _root = root;
        }
        
       
        public bool Insert(TK key, TV value)
        {
            var tmp = _root;
            if (tmp == null)
            {
                _root = new RbNode<TK, TV>(key, value);
                return true;
            }
            var pos = 'r';
            while (tmp != null)
            {
                if (tmp.Key.Equals(key)) return false;
                if (key.CompareTo(tmp.Key) == 1)
                {
                    if (tmp.Right == null)
                    {
                        pos = 'r';
                        break;
                    }
                    tmp = tmp.Right;
                }
                else
                {
                    if (tmp.Left == null)
                    {
                        pos = 'l';
                        break;
                    }
                    tmp = tmp.Left;
                }
            }
            var newRbNode = new RbNode<TK, TV>(key, value, Color.Red) {Parent = tmp};
            if (pos == 'l')
            {
                tmp.Left = newRbNode;
            }
            else
            {
                tmp.Right = newRbNode;
            }
            FixUpInsertRbNode(newRbNode);
            return true;
        }
       
        public TV Search(TK key)
        {
            var tmp = _root;
            while (tmp != null)
            {
                switch (key.CompareTo(tmp.Key))
                {
                    case 0:
                        return tmp.Value;
                    case 1:
                        tmp = tmp.Right;
                        break;
                    case -1:
                        tmp = tmp.Left;
                        break;
                }
            }

            return default(TV); //not default;
        }


        public TV this[TK key]
        {
            get => Search(key);
            set => Insert(key,value);
        }

        public IEnumerator<RbNode<TK, TV>> GetEnumerator()
        {
            foreach (var node in Inorder(_root))
            {
                yield return node;
            }
        }

        public IEnumerable<RbNode<TK, TV>> Inorder(RbNode<TK,TV>root)
        {
            if (root.Left != null)
            foreach (var left  in Inorder(root.Left))
            {
                yield return left;
            }
            yield return root;
            
            if (root.Right != null)
            foreach (var right in Inorder(root.Right))
            {
                yield return right;
            }
        }

        private void FixUpInsertRbNode(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            while ( node.Parent != null && node.Parent.Color == Color.Red)
            {
                RbNode<TK, TV> tmp;
                if (node.Parent.Equals(node.Parent?.Parent?.Left))
                {
                    tmp = node.Parent?.Parent?.Right;
                    if (tmp != null && tmp.Color == Color.Red)
                    {
                        node.Parent.Color = Color.Black; 
                        tmp.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node.Equals(node.Parent?.Right))
                        {
                            node = node.Parent;
                            LeftRotate(node);
                        }
                        node.Parent.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        RightRotate(node.Parent.Parent);
                    }
                }
                else
                {
                    tmp = node.Parent?.Parent?.Left;
                    if (tmp != null && tmp.Color == Color.Red)
                    {
                        node.Parent.Color = Color.Black;
                        tmp.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node.Equals(node.Parent?.Left))
                        {
                            node = node.Parent;
                            RightRotate(node);
                        }
                        node.Parent.Color = Color.Black;
                        node.Parent.Parent.Color = Color.Red;
                        LeftRotate(node.Parent.Parent);
                    }
                }
            }
            _root.Color = Color.Black;
        }

        private void LeftRotate(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            if (node.Right == null)
            {
                Console.Write("Error in Left Rotate");
                return;
                //throw new what;
            }
            var copyNode = node.Right;
            node.Right = copyNode?.Left;

            if (copyNode.Left != null) copyNode.Left.Parent = node;
            copyNode.Parent = node.Parent;

            if (node.Parent == null)
            {
                _root = copyNode;
            }
            else
            {
                if (node.Equals(node.Parent?.Left))
                {
                    node.Parent.Left = copyNode;
                }
                else
                {
                    node.Parent.Right = copyNode;
                }
            }
            copyNode.Left = node;
            node.Parent = copyNode;
        }

        private void RightRotate(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            //if (node.Left == null) ; //throw UnsupportedOperationException("Bad in fun left rotate left child ==null!")
            var copyNode = node.Left;
            node.Left = copyNode?.Right;

            if (copyNode.Right != null) copyNode.Right.Parent = node;
            copyNode.Parent = node.Parent;

            if (node.Parent == null) _root = copyNode;
            else
            {
                if (node.Equals(node.Parent?.Left))
                {
                    node.Parent.Left = copyNode;
                }
                else
                {
                    node.Parent.Right = copyNode;
                }
            }
            copyNode.Right = node;
            node.Parent = copyNode;
        }

        private void RbRemoveFixUp(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            while (!ReferenceEquals(_root, node) && IsBlack(node))
            {
                RbNode<TK, TV> tmp;
                if (ReferenceEquals(node, node.Parent?.Left))
                {
                    tmp = node.Parent.Right;
                    if (!IsBlack(tmp))
                    {
                        tmp.Color = Color.Black;
                        node.Parent.Color = Color.Red;
                        LeftRotate(node.Parent);
                        tmp = node.Parent.Right;
                    }
                    if (IsBlack(tmp.Left) && IsBlack(tmp.Right))
                    {
                        tmp.Color = Color.Red;
                        node = node.Parent;
                    }
                    else if (IsBlack(tmp.Right))
                    {
                        tmp.Left.Color = Color.Black;
                        tmp.Color = Color.Red;
                        RightRotate(tmp);
                    }
                    else
                    {
                        tmp.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        tmp.Right.Color = Color.Black;
                        LeftRotate(node.Parent);
                        node = _root;
                    }
                }
                else
                {
                    tmp = node.Parent.Left;
                    if (!IsBlack(tmp))
                    {
                        tmp.Color = Color.Black;
                        node.Parent.Color = Color.Red;
                        RightRotate(node.Parent);
                        tmp = node.Parent.Left;
                    }
                    if (IsBlack(tmp.Right) && IsBlack(tmp.Left))
                    {
                        tmp.Color = Color.Red;
                        node = node.Parent;
                    }
                    else if (IsBlack(tmp.Left))
                    {
                        tmp.Right.Color = Color.Black;
                        tmp.Color = Color.Red;
                        LeftRotate(tmp);
                    }
                    else
                    {
                        tmp.Color = node.Parent.Color;
                        node.Parent.Color = Color.Black;
                        tmp.Left.Color = Color.Black;
                        RightRotate(node.Parent);
                        node = _root;
                    }
                }
            }
            if (!IsBlack(node)) node.Color = Color.Black;
        }

        private bool IsBlack(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            if (node == null) return true;
            if (node.Color == Color.Black) return true;
            return false;
        }

        private RbNode<TK, TV> GetNodeByMinKey(RbNode<TK, TV> node)
        {
            var tmp = node;
            while (tmp.Left != null) tmp = tmp.Left;
            return tmp;
        }


        public bool Delete(TK key)
        {
            var removedNode = _root;
            while (removedNode != null)
            {
                if (key.Equals(removedNode.Key))
                {
                    break;
                }
                removedNode = key.CompareTo(removedNode.Key) == -1 ? removedNode.Left : removedNode.Right;
            }
            if (removedNode == null) return false;

            if (removedNode.Equals(_root) && removedNode.Left == null && removedNode.Right == null)
            {
                _root = null;
                return true;
            }

            RemoveNode(removedNode);
            return true;
        }


        private void RemoveNode(RbNode<TK, TV> argNode)
        {
            var node = argNode;
            var remNode = node;
            var right = true; // position of remNode

            if (node.Left != null && node.Right != null)
            {
                remNode = GetNodeByMinKey(node.Right);
                node.Key = remNode.Key;
                node.Value = remNode.Value;
                if (remNode.Right != null) remNode.Right.Parent = remNode.Parent;
            }
            else
            {
                if (node.Left != null)
                {
                    right = false;
                    remNode.Left.Parent = node.Parent;
                }
                else if (node.Right != null)
                {
                    remNode.Right.Parent = node.Parent;
                }
            }

            if (remNode.Parent == null)
            {
                _root = right ? remNode.Right : remNode.Left;
            }
            else
            {
                if (remNode.Equals(remNode.Parent.Left))
                {
                    remNode.Parent.Left = right ? remNode.Right : remNode.Left;
                }
                else
                {
                    remNode.Parent.Right = right ? remNode.Right : remNode.Left;
                }
            }

            if (IsBlack(remNode))
            {
                if (remNode.Left != null)
                {
                    RbRemoveFixUp(remNode.Left);
                }
                else if (remNode.Right != null)
                {
                    RbRemoveFixUp(remNode.Right);
                }
                else
                {
                    var nil = new RbNode<TK, TV>(remNode.Key, remNode.Value) {Parent = remNode.Parent};
                    if (remNode.Parent.Left == null)
                    {
                        remNode.Parent.Left = nil;
                    }
                    else
                    {
                        remNode.Parent.Right = nil;
                    }
                    RbRemoveFixUp(nil);
                    if (nil.Parent.Left.Equals(nil))
                    {
                        nil.Parent.Left = null;
                    }
                    else
                    {
                        nil.Parent.Right = null;
                    }
                }
            }
        }
        
        public void PrintTree(RbNode<TK, TV> node, int level = 0) {
            
            if (node != null)
            {
                PrintTree(node.Right, level + 1);
                for (int i = 1;i <= level;i++) Console.Write("  |");
                if (node.Color == Color.Red)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(node.Value);
                }
                else {
                    Console.WriteLine(node.Value);
                }
                PrintTree(node.Left, level + 1);
            }
            Console.ForegroundColor = ConsoleColor.Black;
        }
    }
}
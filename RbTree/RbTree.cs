using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Odbc;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

namespace RbTreeParallel
{
    public class RbTree<K, V> : ITree<K, V>, IEnumerable<RbNode<K, V>> where K : IComparable<K>
    {
        public RbNode<K, V> Root;

        public RbTree(RbNode<K, V> root = null)
        {
            this.Root = root;
        }
        
        //ok
        public bool Insert(K key, V value)
        {
            var tmp = Root;
            if (tmp == null)
            {
                Root = new RbNode<K, V>(key, value);
                return true;
            }
            var pos = 'r';
            while (tmp != null)
            {
                if (tmp.key.Equals(key)) return false;
                if (key.CompareTo(tmp.key) == 1)
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
            var newRbNode = new RbNode<K, V>(key, value, Color.Red) {Parent = tmp};
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
        //ok
        public V Search(K key)
        {
            var tmp = this.Root;
            while (tmp != null)
            {
                switch (key.CompareTo(tmp.key))
                {
                    case 0:
                        return tmp.value;
                    case 1:
                        tmp = tmp.Right;
                        break;
                    case -1:
                        tmp = tmp.Left;
                        break;
                }
            }

            return default(V); //not default;
        }

        
        public IEnumerator<RbNode<K, V>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return 5;
        }

        public IEnumerable<RbNode<K, V>> Inorder(RbNode<K,V>root)
        {
            foreach (var left  in Inorder(root.Left))
            {
                yield return left;
            }
            yield return root;
            foreach (var right in Inorder(root.Right))
            {
                yield return right;
            }
        }

        private void FixUpInsertRbNode(RbNode<K, V> x)
        {
            var node = x;
            //var rbNode = modifiedNode;
            RbNode<K, V> tmp;
            while ( node.Parent != null && node.Parent.color == Color.Red)
            {
                if (node.Parent.Equals(node.Parent?.Parent?.Left))
                {
                    tmp = node.Parent?.Parent?.Right;
                    if (tmp != null && tmp.color == Color.Red)
                    {
                        node.Parent.color = Color.Black; //????
                        tmp.color = Color.Black;
                        node.Parent.Parent.color = Color.Red; //????
                        node = node.Parent.Parent; //???
                    }
                    else
                    {
                        if (node.Equals(node.Parent?.Right))
                        {
                            node = node.Parent;
                            LeftRotate(node);
                        }
                        node.Parent.color = Color.Black;
                        node.Parent.Parent.color = Color.Red;
                        RightRotate(node.Parent.Parent);
                    }
                }
                else
                {
                    tmp = node.Parent?.Parent?.Left;
                    if (tmp != null && tmp.color == Color.Red)
                    {
                        node.Parent.color = Color.Black;
                        tmp.color = Color.Black;
                        node.Parent.Parent.color = Color.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node.Equals(node.Parent?.Left))
                        {
                            node = node.Parent;
                            RightRotate(node);
                        }
                        node.Parent.color = Color.Black;
                        node.Parent.Parent.color = Color.Red;
                        LeftRotate(node.Parent.Parent);
                    }
                }
            }
            Root.color = Color.Black;
        }

        private void LeftRotate(RbNode<K, V> node)
        {
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
                this.Root = copyNode;
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

        private void RightRotate(RbNode<K, V> node)
        {
            if (node.Left == null) ; //throw UnsupportedOperationException("Bad in fun left rotate left child ==null!")
            var copyNode = node.Left;
            node.Left = copyNode?.Right;

            if (copyNode.Right != null) copyNode.Right.Parent = node;
            copyNode.Parent = node.Parent;

            if (node.Parent == null) this.Root = copyNode;
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

        private void RbRemoveFixUp(RbNode<K, V> x)
        {
            var node = x;
            while (!ReferenceEquals(Root, node) && IsBlack(node))
            {
                RbNode<K, V> tmp;
                if (ReferenceEquals(node, node.Parent?.Left))
                {
                    tmp = node.Parent.Right;
                    if (!IsBlack(tmp))
                    {
                        tmp.color = Color.Black;
                        node.Parent.color = Color.Red;
                        LeftRotate(node.Parent);
                        tmp = node.Parent.Right;
                    }
                    if (IsBlack(tmp.Left) && IsBlack(tmp.Right))
                    {
                        tmp.color = Color.Red;
                        node = node.Parent;
                    }
                    else if (IsBlack(tmp.Right))
                    {
                        tmp.Left.color = Color.Black;
                        tmp.color = Color.Red;
                        RightRotate(tmp);
                        tmp = node.Parent.Right;
                    }
                    else
                    {
                        tmp.color = node.Parent.color;
                        node.Parent.color = Color.Black;
                        tmp.Right.color = Color.Black;
                        LeftRotate(node.Parent);
                        node = Root;
                    }
                }
                else
                {
                    tmp = node.Parent.Left;
                    if (!IsBlack(tmp))
                    {
                        tmp.color = Color.Black;
                        node.Parent.color = Color.Red;
                        RightRotate(node.Parent);
                        tmp = node.Parent.Left;
                    }
                    if (IsBlack(tmp.Right) && IsBlack(tmp.Left))
                    {
                        tmp.color = Color.Red;
                        node = node.Parent;
                    }
                    else if (IsBlack(tmp.Left))
                    {
                        tmp.Right.color = Color.Black;
                        tmp.color = Color.Red;
                        LeftRotate(tmp);
                        tmp = node.Parent.Left;
                    }
                    else
                    {
                        tmp.color = node.Parent.color;
                        node.Parent.color = Color.Black;
                        tmp.Left.color = Color.Black;
                        RightRotate(node.Parent);
                        node = Root;
                    }
                }
            }
            if (!IsBlack(node)) node.color = Color.Black;
        }

        private bool IsBlack(RbNode<K, V> node)
        {
            if (node == null) return true;
            if (node.color == Color.Black) return true;
            return false;
        }

        private RbNode<K, V> GetNodeByMinKey(RbNode<K, V> node)
        {
            var tmp = node;
            while (tmp.Left != null) tmp = tmp.Left;
            return tmp;
        }


        public bool Delete(K key)
        {
            var removedNode = Root;
            while (removedNode != null)
            {
                if (key.Equals(removedNode.key))
                {
                    break;
                }
                removedNode = key.CompareTo(removedNode.key) == -1 ? removedNode.Left : removedNode.Right;
            }
            if (removedNode == null) return false;

            if (removedNode.Equals(Root) && removedNode.Left == null && removedNode.Right == null)
            {
                Root = null;
                return true;
            }

            RemoveNode(removedNode);
            return true;
        }


        private void RemoveNode(RbNode<K, V> x)
        {
            var remNode = x;
            var right = true; // position of remNode
            RbNode<K, V> nil;

            if (x.Left != null && x.Right != null)
            {
                remNode = GetNodeByMinKey(x.Right);
                x.key = remNode.key;
                x.value = remNode.value;
                if (remNode.Right != null) remNode.Right.Parent = remNode.Parent;
            }
            else
            {
                if (x.Left != null)
                {
                    right = false;
                    remNode.Left.Parent = x.Parent;
                }
                else if (x.Right != null)
                {
                    remNode.Right.Parent = x.Parent;
                }
            }

            if (remNode.Parent == null)
            {
                Root = right ? remNode.Right : remNode.Left;
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
                    nil = new RbNode<K, V>(remNode.key, remNode.value);
                    nil.Parent = remNode.Parent;
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
    }
}
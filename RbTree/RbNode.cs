using System;

namespace RbTreeParallel
{
    public class RbNode<K, V> : INode<K, V> where K : IComparable<K>
    {
        public RbNode<K, V> Left { get; set; } = null;
        public RbNode<K, V> Parent { get; set; } = null;
        public RbNode<K, V> Right { get; set; } = null;
        public Color color;
        public K key;
        public V value;

        public RbNode(K key, V value,Color color = Color.Black)
        {
            this.key = key;
            this.value = value;
            this.color = color;
        }

        public override bool Equals(object obj)
        {
            RbNode<K, V> other = obj as RbNode<K, V>;
            if (other == null)
            {
                return false;
            }
            return this.key.Equals(other.key) &&
                   this.color.Equals(other.color) &&
                   this.value.Equals(other.value);
        }
    }
}
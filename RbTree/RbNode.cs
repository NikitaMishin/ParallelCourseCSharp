using System;

namespace RbTreeParallel
{
    public class RbNode<TK, TV> : INode<TK, TV> where TK : IComparable<TK>
    {
        public RbNode<TK, TV> Left { get; set; } = null;
        public RbNode<TK, TV> Parent { get; set; } = null;
        public RbNode<TK, TV> Right { get; set; } = null;
        public Color Color;
        public TK Key;
        public TV Value;

        public RbNode(TK key, TV value,Color color = Color.Black)
        {
            Key = key;
            Value = value;
            Color = color;
        }

        public override bool Equals(object obj)
        {
            RbNode<TK, TV> other = obj as RbNode<TK, TV>;
            if (other == null)
            {
                return false;
            }
            return Key.Equals(other.Key) &&
                   Color.Equals(other.Color) &&
                   Value.Equals(other.Value);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _2048_c_sharp.Utilities;

namespace _2048_c_sharp
{
    public abstract class Node<T> where T: Node<T>
    {
        #region Branch/node scoring

        public float Chance { get; set; } = 1f;
        private float _chanceFromRoot = 0f;
        public float ChanceFromRoot
        {
            get
            {
                if (IsRoot) return Chance;
                if (_chanceFromRoot == 0f)
                {
                    _chanceFromRoot = 1f;
                    foreach(var a in Ancestors)
                        _chanceFromRoot *= a.Chance;
                }
                return _chanceFromRoot;
            }
        }
        public abstract float Reward { get; } //represents the reward for only this move
        public float SumOfRewards => ChanceFromRoot * (Reward - Root.Reward);
        public override bool Equals(object obj)
        {
            if (obj.GetType() == GetType()) return Equals((T)obj);
            return false;
        }
        public bool Equals(Node<T> node) => node.Index == Index;

        #endregion

        #region Node traversal

        public T Parent { get; set; }
        private T _root = null;
        public T Root
        {
            get => _root ?? (_root = GetRoot());
            set
            {
                //setting the root of a node sets the root of all child nodes
                _root = value;
                Children.ForEach(c => c.ResetRoot(value));
            }
        }
        public List<T> Children { get; set; } = new List<T>();
        public List<T> Siblings => Parent?.Children;
        public bool IsRoot => Root == this;

        public void ResetRoot(T root)
        {
            Root = root;
            _chanceFromRoot = 0f;
        }

        private int _index = -1;
        public int Index
        {
            get
            {
                if (_index == -1)
                    _index = NodeIndexer.GetNextInt;
                return _index;
            }
        }

        protected Node(T parent, T root = null)
        {
            Parent = parent;
            _root = root ?? parent?.Root ?? (T)this;
        }

        public override int GetHashCode() => Index;

        public T RootEldestChild
        {
            get
            {
                if (IsRoot) return null;
                if (Parent == null) return null;

                T current = (T)this;
                while (true)
                {
                    if (current.Parent.IsRoot)
                        return current;
                    current = current.Parent;
                }
            }
        }

        private T GetRoot()
        {
            if (IsRoot) return (T)this;

            var current = (T)this;
            while (current.Parent != null)
                current = current.Parent;

            return current;
        }

        /// <summary>
        /// The ancestors of the current node, starting with the parent
        /// and working up to the root.
        /// </summary>
        public List<T> Ancestors
        {
            get
            {
                //handle if this is the root
                if (IsRoot) return new List<T>();

                //handle ancestors that have been abandoned
                var ancestors = new List<T>();
                T current = (T)this;
                while ((current = current.Parent) != null)
                {
                    ancestors.Add(current);
                    if (current.IsRoot) break;
                }

                return ancestors;
            }
        }

        #endregion

        #region Branch building

        const int MAX_NODES_WITH_CHILDREN = 10_000;

        /// <summary>
        /// Returns a list of the youngest children
        /// </summary>
        /// <param name="layers"></param>
        /// <returns></returns>
        public List<T> BuildBranches(int layers = 1)
        {
            if (layers < 1) throw new Exception("Cannot have < 1 layers.");
            var nextLayer = new List<T>() { (T)this };

            while(nextLayer.Count() < MAX_NODES_WITH_CHILDREN)
            {
                var currentLayer = nextLayer;
                nextLayer = new List<T>();
                nextLayer.AddRange(currentLayer.AsParallel().SelectMany(l => l.GetChildren()));

                if (!nextLayer.Any())
                    return currentLayer.Count == 1 && currentLayer[0].Equals(this) ? new List<T>() : currentLayer;
            }
            return nextLayer;
        }

        /// <summary>
        /// This is the object-specific method by which children are populated.
        /// </summary>
        /// <returns></returns>
        public abstract List<T> GetChildren();

        #endregion
    }

    public static class NodeIndexer
    {
        private static int _currentInt = 0;
        public static int GetNextInt => ++_currentInt;
    }
}

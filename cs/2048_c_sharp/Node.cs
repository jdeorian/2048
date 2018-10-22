using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            get
            {
                if (_root == null)
                    _root = GetRoot();
                return _root;
            }
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

        public Node(T parent, T root = null)
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

            T current = (T)this;
            while ((current = current.Parent) != null) { }

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
            List<T> currentLayer = new List<T>();
            List<T> nextLayer = new List<T>() { (T)this };

            //int layer = 0;
            while(nextLayer.Count() < MAX_NODES_WITH_CHILDREN)
            {
                //layer++;
                //if (layer > 5)
                //    Console.WriteLine($"Extra layer: {layer}");
                currentLayer = nextLayer;
                nextLayer = new List<T>();
                foreach (var n in currentLayer)
                    nextLayer.AddRange(n.GetChildren());

                if (!nextLayer.Any())
                    return currentLayer.Count == 1 && currentLayer[0].Equals(this) ? new List<T>() : currentLayer;
            }
            return nextLayer;
        }

        /// <summary>
        /// Recursively build out the node structure to the desired number of layers. 1 means
        /// just the possible outcomes of the current state. 2 is 1 extra layer into the future,
        /// and so on. YoungestChildren is a performance concession and is used so that finding
        /// the youngest children doesn't require a second traversal of the tree.
        /// 
        /// This is faster if you don't need the list of youngest children returned.
        /// </summary>
        /// <param name="layers"></param>
        public T BuildBranches_Recursive(int layers = 1)
        {
            if (layers < 1) throw new Exception("Cannot have 0 or negative layers.");

            if (!Children.Any())
                Children = GetChildren();

            if (layers != 1)
                foreach (var child in Children)
                    child.BuildBranches_Recursive(layers - 1);

            return (T)this;
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

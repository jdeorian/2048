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
        public abstract float GetReward(); //represents the reward for only this move
        public float SumOfRewards => Chance * (GetReward() - Ancestors.Last().GetReward());

        #endregion

        #region Node traversal

        public T Parent { get; set; }
        public List<T> Children { get; set; }
        public List<T> Siblings => Parent?.Children;
        public bool IsRoot => Parent == null;

        public Node(T parent)
        {
            Parent = parent;
        }

        private T _rootEldestChild = null;
        public T RootEldestChild
        {
            get
            {
                //reset if an eldest child becomes the root
                if (_rootEldestChild != null && IsRoot) _rootEldestChild = null;

                //set if unset
                if (_rootEldestChild == null && !IsRoot) //if this is the root, then the eldest child SHOULD be null
                {
                    _rootEldestChild = (T)this;
                    while (_rootEldestChild.Parent.Parent != null)
                        _rootEldestChild = _rootEldestChild.Parent;
                }

                return _rootEldestChild;
            }
        }

        private T _root = null;
        public T Root
        {
            get
            {
                if (_root == null)
                    _root = IsRoot ? (T)this : RootEldestChild.Parent;              
                return _root;
            }
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
                    ancestors.Add(current);

                return ancestors;
            }
        }

        #endregion

        #region Branch building

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

            for(int layer_num = 1; layer_num <= layers; layer_num++)
            {
                currentLayer = nextLayer;
                nextLayer = new List<T>();
                foreach (var n in currentLayer)
                    nextLayer.AddRange(n.GetChildren());

                if (!nextLayer.Any()) return currentLayer;
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
}

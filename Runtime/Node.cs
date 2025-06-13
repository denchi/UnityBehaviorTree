using UnityEngine;
using System.Collections.Generic;

namespace Behaviours.BT
{
    public class Node : ScriptableObject
    {
        public Tree tree;
        public Task task;
        public Node parent;
        public List<IService> services;

        public string title;

        #region RECT

        public float X { get; set; }
        public int Y { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public float Mod { get; set; }

        public List<Node> Children
        {
            get
            {                
                if (task is Composite composite)
                {
                    return composite.nodes;
                }

                return new List<Node>();
            }
        }

        public bool IsLeaf()
        {
            return this.Children.Count == 0;
        }

        public Node Parent
        {
            get
            {
                if (tree == null)
                    return null;
                return findNodeParent(tree.root, this);
            }
        }

        static Node findNodeParent(Node node, Node target)
        {
            for (int i = 0; i < node.Children.Count; ++i)
            {
                Node temp = node.Children[i];

                if (temp == target)
                {
                    return node;
                }
                else
                {
                    Node child = findNodeParent(temp, target);
                    if (child)
                        return child;
                }
            }

            return null;
        }

        public bool IsLeftMost()
        {
            if (this.Parent == null)
                return true;

            return this.Parent.Children[0] == this;
        }

        public bool IsRightMost()
        {
            if (this.Parent == null)
                return true;

            return this.Parent.Children[this.Parent.Children.Count - 1] == this;
        }

        public Node GetPreviousSibling()
        {
            if (this.Parent == null || this.IsLeftMost())
                return null;

            return this.Parent.Children[this.Parent.Children.IndexOf(this) - 1];
        }

        public Node GetNextSibling()
        {
            if (this.Parent == null || this.IsRightMost())
                return null;

            return this.Parent.Children[this.Parent.Children.IndexOf(this) + 1];
        }

        public Node GetLeftMostSibling()
        {
            if (this.Parent == null)
                return null;

            if (this.IsLeftMost())
                return this;

            return this.Parent.Children[0];
        }

        public Node GetLeftMostChild()
        {
            if (this.Children.Count == 0)
                return null;

            return this.Children[0];
        }

        public Node GetRightMostChild()
        {
            if (this.Children.Count == 0)
                return null;

            return this.Children[Children.Count - 1];
        }

        #endregion

        [HideInInspector]
        public Rect rect = new Rect(0, 0, 200, 50);

        private void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;

            if (services == null)
            {
                services = new List<IService>();
            }
        }

        public string ToFullQualifiedName
        {
            get
            {
                // TREE_NAME/Repeat(0)/Selector(1)

                var text = "";
                var node = this;
                while (node != null)
                {
                    if (node.parent)
                    {
                        var children = node.parent.Children;
                        var idx = children.IndexOf(node);

                        if (node.task)
                        {
                            text = node.task.GetType().Name + "(" + idx.ToString() + ")/" + text;
                        }
                        else
                        {
                            
                            text = "NULL" + "(" + idx.ToString() + ")/" + text;
                        }
                    }
                    else
                    {
                        if (node.task)
                        {
                            text = node.tree.name + "/" + node.task.GetType().Name + "/" + text;
                        }
                        else
                        {
                            text = node.tree.name + "/NULL/" + text;
                        }
                    }
                    node = node.parent;
                }
                return text;
            }
        }
    }
}
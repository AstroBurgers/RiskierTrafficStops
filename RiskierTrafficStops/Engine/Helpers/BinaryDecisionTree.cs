namespace RiskierTrafficStops.Engine.Helpers;

internal class Bdt {
    
    internal class Node {
        internal Node Left;
        internal Node Right;
        internal bool Value;
        internal Action OutcomeAssociated;

        internal Node(bool value, Node left, Node right, Action outcomeAssociated) {
                Value = value;
                Left = left;
                Right = right;
                OutcomeAssociated = outcomeAssociated;
            }

        internal Node(bool value, Node left, Node right)
        {
                Value = value;
                Left = left;
                Right = right;
                OutcomeAssociated = null;
            }
    }

    private Node _root;

    internal Bdt() {
            _root = null;
        }
    internal Bdt(Node root) {
            _root = root;
        }
    
    
    internal void FollowTruePath()
    {
            FollowTruePath(_root);
        }

    private void FollowTruePath(Node subroot)
    {
            // Follow the true path recursively
            if (subroot.Value)
            {
                if (subroot.Right == null && subroot.OutcomeAssociated != null)
                {
                    subroot.OutcomeAssociated();
                }
                else
                {
                    FollowTruePath(subroot.Right);
                }
            }
            else
            {
                if (subroot.Left == null && subroot.OutcomeAssociated != null)
                {
                    subroot.OutcomeAssociated();
                }
                else
                {
                    FollowTruePath(subroot.Left);
                }
            }
        }

    internal bool IsEmpty() {
            return _root == null;
        }

    internal void Add(Node node, bool insertToLeft)
    {
            if (IsEmpty())
            {
                _root = node;
            }
            else
            {
                Add(_root, node, insertToLeft);
            }
        }

    private void Add(Node parentRoot, Node node, bool insertToLeft)
    {
            if (insertToLeft)
            {
                if (parentRoot.Left == null)
                {
                    parentRoot.Left = new Node(node.Value, null, null, node.OutcomeAssociated);
                }
                else
                {
                    Add(parentRoot.Left, node, insertToLeft);
                }
            }
            else
            {
                if (parentRoot.Right == null)
                {
                    parentRoot.Right = new Node(node.Value, null, null, node.OutcomeAssociated);
                }
                else
                {
                    Add(parentRoot.Right, node, insertToLeft);
                }
            }
        }

}
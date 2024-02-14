namespace RiskierTrafficStops.Engine.Helpers;

public class Bdt {
    
    public class Node {
        public Node Left;
        public Node Right;
        public bool Value;
        public Action OutcomeAssociated;

        public Node(bool value, Node left, Node right, Action outcomeAssociated) {
            this.Value = value;
            this.Left = left;
            this.Right = right;
            this.OutcomeAssociated = outcomeAssociated;
        }

        public Node(bool value, Node left, Node right)
        {
            this.Value = value;
            this.Left = left;
            this.Right = right;
            OutcomeAssociated = null;
        }
    }

    private Node _root;

    public Bdt() {
        _root = null;
    }
    public Bdt(Node root) {
        this._root = root;
    }
    
    
    public void FollowTruePath()
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

    public bool IsEmpty() {
        return _root == null;
    }

    public void Add(Node node, bool insertToLeft)
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
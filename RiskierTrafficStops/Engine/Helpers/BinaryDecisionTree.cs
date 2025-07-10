namespace RiskierTrafficStops.Engine.Helpers;

internal class Bdt
{
    internal class Node
    {
        internal Node Left { get; set; }
        internal Node Right { get; set; }
        internal bool Value { get; }
        internal Action OutcomeAssociated { get; }

        internal Node(bool value, Node left = null, Node right = null, Action outcomeAssociated = null)
        {
            Value = value;
            Left = left;
            Right = right;
            OutcomeAssociated = outcomeAssociated;
        }
    }

    private Node _root;

    internal Bdt() => _root = null;
    internal Bdt(Node root) => _root = root;

    internal bool IsEmpty() => _root == null;

    internal void Add(Node node, bool insertToLeft)
    {
        if (IsEmpty())
        {
            _root = node;
        }
        else
        {
            AddRecursive(_root, node, insertToLeft);
        }
    }

    private void AddRecursive(Node current, Node node, bool insertToLeft)
    {
        if (insertToLeft)
        {
            if (current.Left == null)
                current.Left = node;
            else
                AddRecursive(current.Left, node, true);
        }
        else
        {
            if (current.Right == null)
                current.Right = node;
            else
                AddRecursive(current.Right, node, false);
        }
    }

    internal void FollowTruePath() => FollowTruePath(_root);

    private void FollowTruePath(Node node)
    {
        if (node == null) return;

        if (node.Value)
        {
            if (node.Right == null)
            {
                node.OutcomeAssociated?.Invoke();
                return;
            }
            FollowTruePath(node.Right);
        }
        else
        {
            if (node.Left == null)
            {
                node.OutcomeAssociated?.Invoke();
                return;
            }
            FollowTruePath(node.Left);
        }
    }
}
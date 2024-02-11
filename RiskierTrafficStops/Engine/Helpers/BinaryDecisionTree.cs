namespace RiskierTrafficStops.Engine.Helpers;

public class BDT {
    
    public class Node {
        public Node left;
        public Node right;
        public bool value;
        public Action outcomeAssociated;

        public Node(bool value, Node left, Node right, Action outcomeAssociated) {
            this.value = value;
            this.left = left;
            this.right = right;
            this.outcomeAssociated = outcomeAssociated;
        }

        public Node(bool value, Node left, Node right)
        {
            this.value = value;
            this.left = left;
            this.right = right;
            outcomeAssociated = null;
        }
    }

    private Node root;

    public BDT() {
        root = null;
    }
    public BDT(Node root) {
        this.root = root;
    }
    
    
    public void FollowTruePath()
    {
        FollowTruePath(root);
    }

    private void FollowTruePath(Node subroot)
    {
        // Follow the true path recursively
        if (subroot.value)
        {
            if (subroot.right == null && subroot.outcomeAssociated != null)
            {
                subroot.outcomeAssociated();
            }
            else
            {
                FollowTruePath(subroot.right);
            }
        }
        else
        {
            if (subroot.left == null && subroot.outcomeAssociated != null)
            {
                subroot.outcomeAssociated();
            }
            else
            {
                FollowTruePath(subroot.left);
            }
        }
    }

    public bool isEmpty() {
        return root == null;
    }

    public void add(Node node, bool insertToLeft)
    {
        if (isEmpty())
        {
            root = node;
        }
        else
        {
            add(root, node, insertToLeft);
        }
    }

    private void add(Node parentRoot, Node node, bool insertToLeft)
    {
        if (insertToLeft)
        {
            if (parentRoot.left == null)
            {
                parentRoot.left = new Node(node.value, null, null, node.outcomeAssociated);
            }
            else
            {
                add(parentRoot.left, node, insertToLeft);
            }
        }
        else
        {
            if (parentRoot.right == null)
            {
                parentRoot.right = new Node(node.value, null, null, node.outcomeAssociated);
            }
            else
            {
                add(parentRoot.right, node, insertToLeft);
            }
        }
    }

}
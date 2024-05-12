public class MultiNodeTree<T> {
    public class Node {
        public T value;
        public List<Node> children;
        public Node(T value) {
            this.value = value;
            this.children = new List<Node>();
        }

        public void AddChild(T value) {
            children.Append(new Node(value));
        }
    }
    public List<Node> root;
    public MultiNodeTree() {
        root = new List<Node>();
    }
    
    public void AddAtRoot(T value) {
        root.Append(new Node(value));
    }    
}
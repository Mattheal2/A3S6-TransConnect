public class MultiNodeTree<T> {
    public class Node {
        public T value { get; private set; }
        public List<Node> children { get; private set; }
        public int id { get; private set; }
        public int parent_id { get; private set; }
        public Node(T value, int id, int parent_id) {
            this.value = value;
            this.id = id;
            this.parent_id = parent_id;
            this.children = new List<Node>();
        }
        public void AddChild(Node child) {
            children.Append(child);
        }
    }
    public List<Node> root { get; private set; }

    public MultiNodeTree() {
        root = new List<Node>();
    }

    public void AddNode(T value, int id, int parent_id) {
        Node new_node = new Node(value, id, parent_id);
        // Find the parent node
        Node? parent = FindNode(parent_id);
        if (parent == null) {
            root.Append(new_node);
        } else {
            parent.AddChild(new_node);
        }

        // Process "orphan" nodes with this node as a parent
        // They are all in the root list
        for (int i = 0; i < root.Length; i++) {
            Node node = root.Get(i);
            if (node.parent_id == id) {
                new_node.AddChild(node);
                root.RemoveAt(i);
                i--;
            }
        }
    }

    public Node? FindNode(int id) {
        List<Node> nodes = root;
        Node? parent = null;
        while (parent == null) {
            nodes.ForEach(node => {
                if (node.id == id) {
                    parent = node;
                }
            });

            if (parent == null) {
                List<Node> new_nodes = new List<Node>();
                nodes.ForEach(node => {
                    new_nodes.Extend(node.children);
                });
                nodes = new_nodes;
            }
        }

        return parent;
    }

    #region JSON
    public class JsonNode {
        public T value { get; private set; }
        public JsonNode[] children { get; private set; }
        public JsonNode(Node node) {
            value = node.value;
            children = new JsonNode[node.children.Length];
            for (int i = 0; i < node.children.Length; i++) {
                children[i] = new JsonNode(node.children.Get(i));
            }
        }
    }

    public JsonNode[] ToJson() {
        JsonNode[] json = new JsonNode[root.Length];
        for (int i = 0; i < root.Length; i++) {
            json[i] = new JsonNode(root.Get(i));
        }
        return json;
    }
    #endregion
}
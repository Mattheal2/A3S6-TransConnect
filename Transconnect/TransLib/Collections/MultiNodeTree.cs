
/// <summary>
/// A tree data structure that allows each node to have multiple children.
/// The tree is represented as a list of root nodes.
/// Each node has a value, an id, and a parent_id.
/// </summary>
/// <typeparam name="T"></typeparam>
public class MultiNodeTree<T> {
    /// <summary>
    /// A node in the tree.
    /// </summary>
    public class Node {
        #region Properties
        /// <summary>
        /// The value of the node.
        /// </summary>
        public T value { get; private set; }
        /// <summary>
        /// The children of the node.
        /// </summary>
        public List<Node> children { get; private set; }
        /// <summary>
        /// The id of the node, must be unique.
        /// </summary>
        public int id { get; private set; }
        /// <summary>
        /// The id of the parent node, if no parent then it must not correspond to any node id.
        /// Preferably, the root nodes should have negative parent_ids.
        /// </summary>
        public int parent_id { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new node.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="id">The id of the node, must be unique.</param>
        /// <param name="parent_id">The id of the parent node, if no parent then it must not correspond to any node id.</param>
        public Node(T value, int id, int parent_id) {
            this.value = value;
            this.id = id;
            this.parent_id = parent_id;
            this.children = new List<Node>();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a child to the node.
        /// </summary>
        /// <param name="child"></param>
        public void AddChild(Node child) {
            children.Append(child);
        }

        /// <summary>
        /// Display the node and its children as a string.
        /// We use depth to indent the children.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public string Display(int depth = 0) {
            string str = new string(' ', depth * 2) + value?.ToString() + "\n";
            children.ForEach(child => {
                str += child.Display(depth + 1);
            });
            return str;
        }

        /// <summary>
        /// Get the depth of the node.
        /// </summary>
        /// <returns></returns>
        public int Depth() {
            int max_depth = 0;
            children.ForEach(child => {
                max_depth = Math.Max(max_depth, child.Depth());
            });
            return max_depth + 1;
        }

        /// <summary>
        /// Get the nodes at a specific depth.
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        public List<Node> GetNodesAtDepth(int depth) {
            List<Node> nodes = new List<Node>();
            if (depth == 0) {
                nodes.Append(this);
            } else {
                children.ForEach(child => {
                    nodes.Extend(child.GetNodesAtDepth(depth - 1));
                });
            }
            return nodes;
        }
        #endregion
    }
    
    #region Properties
    /// <summary>
    /// The root nodes of the tree.
    /// </summary>
    public List<Node> root { get; private set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Create a new tree.
    /// </summary>
    public MultiNodeTree() {
        root = new List<Node>();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a node to the tree.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="id">The id of the node, must be unique.</param>
    /// <param name="parent_id">The id of the parent node, if no parent then it must not correspond to any node id.</param>
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

    /// <summary>
    /// Find a node by its id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Node or null if not found.</returns>
    public Node? FindNode(int id) {
        List<Node> nodes = root;
        Node? node_found = null;
        while (node_found == null) {
            nodes.ForEach(node => {
                if (node.id == id) {
                    node_found = node;
                }
            });

            if (node_found == null) {
                List<Node> new_nodes = new List<Node>();
                nodes.ForEach(node => {
                    new_nodes.Extend(node.children);
                });
                nodes = new_nodes;
            }
        }

        return node_found;
    }

    /// <summary>
    /// Remove a node by its id.
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Node or null if not found.</returns>
    public Node? RemoveNode(int id) {
        Node? node = FindNode(id);
        if (node == null) return null;

        Node? parent = FindNode(node.parent_id);
        if (parent != null) parent.children.Remove(node);

        return node;
    }
    
    /// <summary>
    /// Get the (max) depth of the tree.
    /// </summary>
    /// <returns></returns>
    public int Depth() {
        int max_depth = 0;
        root.ForEach(node => {
            max_depth = Math.Max(max_depth, node.Depth());
        });
        return max_depth;
    }

    /// <summary>
    /// Get the nodes at a specific depth.
    /// </summary>
    /// <param name="depth"></param>
    /// <returns></returns>
    public List<Node> GetNodesAtDepth(int depth) {
        List<Node> nodes = new List<Node>();
        root.ForEach(node => {
            nodes.Extend(node.GetNodesAtDepth(depth));
        });
        return nodes;
    }
    
    /// <summary>
    /// Display the tree as a string.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        string str = "";
        root.ForEach(node => {
            str += node.Display();
        });
        return str;
    }
    #endregion

    #region JSON
    /// <summary>
    /// An intermediate class to convert the tree to JSON.
    /// </summary>
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

    /// <summary>
    /// Convert the tree to a JSON-seriazable format.
    /// </summary>
    /// <returns></returns>
    public JsonNode[] ToJson() {
        JsonNode[] json = new JsonNode[root.Length];
        for (int i = 0; i < root.Length; i++) {
            json[i] = new JsonNode(root.Get(i));
        }
        return json;
    }
    #endregion
}
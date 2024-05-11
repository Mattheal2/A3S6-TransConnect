/// <summary>
/// A binary tree set implementation.
/// As a set, it does not allow duplicate elements.
/// It does not allow null elements and the order is not guaranteed.
/// It returns the elements in ascending order.
/// This tree uses in-order traversal to return the elements: left, root, right.
/// </summary>
/// <typeparam name="T"></typeparam>
public class BinaryTreeSet<T>: IListAlike<T> where T: IComparable<T>, IEquatable<T> {
    /// <summary>
    /// A node in the binary tree.
    /// Contains a value and two child nodes (may be null).
    /// </summary>
    internal class Node {
        #region Fields
        /// <summary>
        /// The value of the node.Must not be null.
        /// </summary>
        internal T value;
        /// <summary>
        /// The left child node. May be null.
        /// </summary>
        internal Node? left;
        /// <summary>
        /// The right child node. May be null.
        /// </summary>
        internal Node? right;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a node with a value.
        /// </summary>
        /// <param name="value"></param>
        internal Node(T value) {
            this.value = value;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Inserts a value into the tree.
        /// </summary>
        /// <param name="value"></param>
        internal void Insert(T value) {
            if (value.CompareTo(this.value) < 0) {
                if (left == null) left = new Node(value);
                else left.Insert(value);
            } else if (value.CompareTo(this.value) > 0) {
                if (right == null) right = new Node(value);
                else right.Insert(value);
            }
        }

        /// <summary>
        /// Gets the node at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        internal Node? GetAt(int index, ref int count) {
            if (left != null) {
                Node? leftNode = left.GetAt(index, ref count);
                if (leftNode != null) return leftNode;
            }
            if (count == index) return this;
            count++;
            if (right != null) {
                Node? rightNode = right.GetAt(index, ref count);
                if (rightNode != null) return rightNode;
            }
            return null;
        }

        /// <summary>
        /// Iterates over the elements of the tree in ascending order.
        /// </summary>
        /// <param name="func"></param>
        internal void ForEach(IListAlike<T>.IterFunc func) {
            if (left != null) left.ForEach(func);
            func(value);
            if (right != null) right.ForEach(func);
        }
        
        /// <summary>
        /// Removes the node with the specified value from the tree.
        /// </summary>
        /// <param name="value"></param>
        internal Node? RemoveByValue(T value, ref bool removed) {
            if (value.CompareTo(this.value) < 0) {
                if (left != null) left = left.RemoveByValue(value, ref removed);
            } else if (value.CompareTo(this.value) > 0) {
                if (right != null) right = right.RemoveByValue(value, ref removed);
            } else if (this.value.Equals(value)) {
                removed = true;
                return Remove(ref removed);
            } 
            return this;
        }

        /// <summary>
        /// Removes this node from the tree.
        /// </summary>
        /// <returns></returns>
        internal Node? Remove(ref bool removed) {
            if (left == null && right == null) return null;
            if (left == null) return right;
            if (right == null) return left;

            Node min = right.GetMin();
            value = min.value;
            right = right.RemoveByValue(min.value, ref removed);

            return this;
        }

        internal Node GetMin() {
            Node current = this;
            while (current.left != null) {
                current = current.left;
            }
            return current;
        }

        internal int IndexOf(T value, int index) {
            if (left != null) {
                int leftIndex = left.IndexOf(value, index);
                if (leftIndex != -1) return leftIndex;
            }
            if (this.value.Equals(value)) return index;
            index++;
            if (right != null) {
                int rightIndex = right.IndexOf(value, index);
                if (rightIndex != -1) return rightIndex;
            }
            return -1;
        }
        #endregion
    }

    #region Fields
    private Node? root; // The root node of the tree
    private int count; // The number of elements in the tree
    #endregion

    #region Constructors
    /// <summary>
    /// Creates an empty tree.
    /// </summary>
    public BinaryTreeSet() {
        root = null;
        count = 0;
    }
    
    /// <summary>
    /// Creates a tree with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public BinaryTreeSet(IListAlike<T> other) {
        root = null;
        count = 0;
        // Extend(other);
    }
    #endregion

    #region IListAlike Properties and Methods
    /// <summary>
    /// The length of the list.
    /// </summary>
    public int Length { get { return count; } }

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds or negative.</exception>
    public T Get(int index) {
        if (index < 0 || index >= count || root == null) throw new IndexOutOfRangeException();
        int i = 0;
        return root.GetAt(index, ref i)!.value;
    }

    

    /// <summary>
    /// Appends a value to the end of the list.
    /// </summary>
    /// <param name="value"></param>
    /// /// <exception cref="ArgumentNullException">if value is null</exception>
    public void Append(T value) {
        if (value == null) throw new ArgumentNullException();
        if (root == null) root = new Node(value);
        else root.Insert(value);
        count++;
    }

    /// <summary>
    /// Extends the list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public void Extend(IListAlike<T> other) {
        other.ForEach(Append);
    }

    /// <summary>
    /// Unsupported operation.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="NotSupportedException">Insertion is not supported in a set. Use Append instead.</exception>
    public void Insert(int index, T value) {
        throw new NotSupportedException("Insertion is not supported in a set. Use Append instead.");
    }
    
    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index) {
        if (index < 0 || index >= count || root == null) throw new IndexOutOfRangeException();
        int i = 0;
        bool removed = false;
        root = root.RemoveByValue(root.GetAt(index, ref i)!.value, ref removed);
        count--;
    }

    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the value was found and removed, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">if value is null</exception>
    public bool Remove(T value) {
        if (root == null) return false;
        bool removed = false;
        root = root.RemoveByValue(value, ref removed);
        if (removed) count--;
        return removed;
    }

    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1 if not found, index otherwise</returns>
    public int IndexOf(T value) {
        if (root == null) return -1;
        return root.IndexOf(value, 0);
    }

    /// <summary>
    /// Filters the list based on a predicate. The resulting list contains only the elements for which the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IListAlike<T> Filter(IListAlike<T>.MapFunc<bool> predicate) {
        BinaryTreeSet<T> result = new BinaryTreeSet<T>();
        ForEach((T elem) => {
            if (predicate(elem)) result.Append(elem);
        });
        return result;
    }

    /// <summary>
    /// Sorts the list based on a comparison function. Algorithm is implementation-specific.
    /// </summary>
    /// <param name="compare"></param>
    public void Sort(IListAlike<T>.Compare compare) {
        // No-op since the tree is always sorted
    }

    /// <summary>
    /// Maps the list to another list based on a mapping function.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Mapping is not supported in a set.</exception>
    public IListAlike<U> Map<U>(IListAlike<T>.MapFunc<U> map) {
        throw new NotImplementedException("Mapping is not supported in a set.");
    }


    /// <summary>
    /// Sorts the list based on a comparison function. Algorithm is implementation-specific.
    /// </summary>
    /// <param name="compare"></param>
    public void ForEach(IListAlike<T>.IterFunc func) {
        if (root != null) root.ForEach(func);
    }

    /// <summary>
    /// Returns an array of the elements in the list.
    /// </summary>
    /// <returns></returns>
    public T[] ToArray()
    {
        T[] array = new T[count];
        int i = 0;
        ForEach((T elem) =>
        {
            array[i] = elem;
            i++;
        });
        return array;
    }

    /// <summary>
    /// Converts the list to a string, with each element separated by a comma.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        string s = "BinaryTreeSet[";
        int length = 0;

        ForEach((T elem) => {
            if (length > 0) s += ", ";
            s += elem.ToString();
            length++;
        });

        s += "]";
        return s;
    }

    /// <summary>
    /// Converts the list to a string, with each element separated by a specified separator.
    /// </summary>
    /// <param name="separator"></param>
    public string Join(string separator) {
        string s = "";
        int length = 0;

        ForEach((T elem) => {
            if (length > 0) s += separator;
            s += elem.ToString();
            length++;
        });

        return s;
    }
    #endregion
}
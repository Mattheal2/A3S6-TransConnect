using System.Diagnostics;

/// <summary>
/// A singly linked list that supports chaining operations.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ChainedList<T> : IListAlike<T> {
    /// <summary>
    /// A Node in a singly linked list.
    /// Safety: this class should never be exposed to the user.
    /// Values are public for simplicity.
    /// </summary>
    internal class ChainedListNode {

        #region Fields
        /// <summary>
        /// The next node in the list (null if this is the last node).
        /// </summary>
        internal ChainedListNode? Next;
        /// <summary>
        /// The value of this node.
        /// </summary>
        internal T Val;
        #endregion

        #region Constructors
        internal ChainedListNode(T v) {
            Val = v;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the last node in the list.
        /// </summary>
        /// <returns>The last node in the linked list.</returns>
        internal ChainedListNode GetLast() {
            return Next == null ? this : Next.GetLast();
        }


        /// <summary>
        /// Sets the next node of the last node in the list.
        /// </summary>
        /// <param name="node">The node to set as the next node of the last node.</param>
        internal void SetLast(ChainedListNode node) {
            GetLast().Next = node;
        }

        /// <summary>
        /// Returns the Nth child of this node.
        /// </summary>
        /// <param name="index">The index of the child to return.</param>
        /// <returns>The Nth child of this node.</returns>
        /// <remarks>
        /// Do not call this method with a negative index.
        /// Returns null if the index is out of bounds.
        /// </remarks>
        internal ChainedListNode? GetNthChild(int index) {
            if (index < 0) return this;
            if (Next == null) return null;
            return Next.GetNthChild(index - 1);
        }
        /// <summary>
        ///  Returns the number of children of this node + 1.
        /// </summary>
        /// <returns></returns>
        internal int Size() {
            return Next == null ? 1 : Next.Size() + 1;
        }

        /// <summary>
        /// Returns the representation of the list starting from this node.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            string s = Val?.ToString() ?? "null";
            if (Next != null) s += ", " + Next.ToString();
            return s;
        }
        #endregion
    }

    #region Fields
    /// <summary>
    /// The first node in the list (null if the list is empty).
    /// </summary>
    private ChainedListNode? first;
    /// <summary>
    /// The length of the list.
    /// This can be obtained using dynamic: first == null ? 0 : first.CountChildren().
    /// But for performance reasons, we store it here.
    /// </summary>
    private int length;
    #endregion

    #region Constructors
    /// <summary>
    ///  Creates an empty list.
    /// </summary>
    public ChainedList() {
        first = null;
    }

    /// <summary>
    /// Creates a list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public ChainedList(IListAlike<T> other) {
        first = null;
        length = 0;
        Extend(other);
    }
    #endregion

    #region IListAlike Properties and Methods
    // int IListAlike<T>.Length => first == null ? 0 : first.CountChildren();
    /// <summary>
    /// The length of the list.
    /// </summary>
    int IListAlike<T>.Length { get { return length; } }

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds or negative.</exception>
    public T Get(int index) {
        if (index < 0 || first == null) throw new IndexOutOfRangeException();

        ChainedListNode? node = first.GetNthChild(index);
        if (node == null) throw new IndexOutOfRangeException();

        return node.Val;
    }

    /// <summary>
    /// Appends a value to the end of the list.
    /// </summary>
    /// <param name="value"></param>
    public void Append(T value) {
        ChainedListNode nw = new ChainedListNode(value);
        if (first == null) {
            first = nw;
        } else {
            first.SetLast(nw);
        }
        length++;
    }

    /// <summary>
    /// Extends the list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public void Extend(IListAlike<T> other){
        // NOTE: we removed fast-path if other is also a ChainedList because we want to clone the list.
        this.length += other.Length;
        other.ForEach(e => {
            ChainedListNode nw = new ChainedListNode(e);
            if (first == null) {
                first = nw;
            } else {
                first.SetLast(nw);
            }
        });
    }

    /// <summary>
    /// Inserts a value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds or negative.</exception>
    public void Insert(int index, T value) {
        if (index < 0 || index > length) throw new IndexOutOfRangeException();

        ChainedListNode nw = new ChainedListNode(value);
        if (index == 0) {
            nw.Next = first;
            first = nw;
        } else {
            ChainedListNode? prev = first?.GetNthChild(index - 1);
            if (prev == null) throw new UnreachableException(); // bound already checked
            nw.Next = prev.Next;
            prev.Next = nw;
        }
        length++;
    }

    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <exception cref="IndexOutOfRangeException"></exception>
    public void RemoveAt(int index) {
        if (index < 0 || index >= length || first == null) {
            throw new IndexOutOfRangeException();
        }

        if (index == 0) {
            first = first.Next;
        } else {
            ChainedListNode? prev = first.GetNthChild(index - 1);
            if (prev == null) throw new UnreachableException(); // bound already checked
            prev.Next = prev.Next?.Next;
        }
        length--;
    }
    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int IndexOf(T value) {
        ChainedListNode? current = first;
        int index = 0;
        while (current != null && current.Val != null) {
            if (current.Val.Equals(value)) return index;
            current = current.Next;
            index++;
        }
        return -1;
    }


    /// <summary>
    /// Filters the list, keeping only the elements that satisfy the predicate.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IListAlike<T> Filter(IListAlike<T>.MapFunc<bool> predicate) {
        ChainedList<T> result = new ChainedList<T>();
        ChainedListNode? current = first;
        while (current != null) {
            if (predicate(current.Val)) result.Append(current.Val);
            current = current.Next;
        }
        return result;
    }



    /// <summary>
    /// Sorts the list in place using bubble sort.
    /// </summary>
    /// <param name="compare"></param>
    public void Sort(IListAlike<T>.Compare compare) {
        ChainedListNode? current = first;
        while (current != null){
            ChainedListNode? next = current.Next;
            while (next != null) {
                if (compare(current.Val, next.Val) > 0) {
                    T temp = current.Val;
                    current.Val = next.Val;
                    next.Val = temp;
                }
                next = next.Next;
            }
            current = current.Next;
        }
    }

    /// <summary>
    /// Maps the list to another list using the specified function.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    public IListAlike<U> Map<U>(IListAlike<T>.MapFunc<U> func) {
        ChainedList<U> result = new ChainedList<U>();
        ChainedListNode? current = first;
        while (current != null) {
            result.Append(func(current.Val));
            current = current.Next;
        }
        return result;
    }

    /// <summary>
    /// Applies a function to each element of the list.
    /// </summary>
    /// <param name="fn"></param>
    public void ForEach(IListAlike<T>.IterFunc fn) {
        ChainedListNode? current = first;
        while (current != null) {
            fn(current.Val);
            current = current.Next;
        }
    }

    /// <summary>
    /// Converts the list to a string, with each element separated by a comma.
    /// </summary>
    /// <returns></returns>
    public override string ToString() {
        string s = "ChainedList[";

        if (first != null) s += first.ToString();

        return s + "]";
    }
    #endregion
}
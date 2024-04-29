public interface IListAlike<T> {
    #region Properties
    /// <summary>
    /// The length of the list.
    /// </summary>
    int Length { get; }
    #endregion
        
    #region Delegates
    /// <summary>
    /// A delegate that compares two elements of the list.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    delegate int Compare(T a, T b);
    /// <summary>
    /// A delegate that iterates over the elements of the list.
    /// </summary>
    /// <param name="a"></param>
    delegate void IterFunc(T a);
    /// <summary>
    /// A delegate that maps an element of the list to another type.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="a"></param>
    /// <returns></returns>
    delegate U MapFunc<U>(T a);
    #endregion

    #region Methods
    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    T Get(int index);
    /// <summary>
    /// Appends a value to the end of the list.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException">if value is null</exception>
    void Append(T value);
    /// <summary>
    /// Extends the list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    void Extend(IListAlike<T> other);

    /// <summary>
    /// Inserts a value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds or negative.</exception>
    /// <exception cref="ArgumentNullException">If value is null.</exception>
    /// <exception cref="NotSupportedException">If the collection is a set (use Append instead).</exception>
    void Insert(int index, T value);
    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    void RemoveAt(int index);
    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the value was found and removed, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">if value is null</exception>
    bool Remove(T value);
    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1 if not found, index otherwise</returns>
    int IndexOf(T value);

    /// <summary>
    /// Filters the list based on a predicate. The resulting list contains only the elements for which the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    IListAlike<T> Filter(MapFunc<bool> predicate);
    /// <summary>
    /// Sorts the list based on a comparison function. Algorithm is implementation-specific.
    /// </summary>
    /// <param name="compare"></param>
    void Sort(Compare compare);
    /// <summary>
    /// Maps the list to another list based on a mapping function.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException">Mapping is not supported in a set.</exception>
    IListAlike<U> Map<U>(MapFunc<U> map);
    /// <summary>
    /// Iterates over the elements of the list.
    /// </summary>
    /// <param name="fn"></param>
    void ForEach(IterFunc fn);

    /// <summary>
    /// Converts the list to an array.
    /// </summary>
    /// <returns></returns>
    T[] ToArray();

    /// <summary>
    /// Converts the list to a string, with each element separated by a comma.
    /// </summary>
    /// <returns></returns>
    string ToString();
    #endregion
}
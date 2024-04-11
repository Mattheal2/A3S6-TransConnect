using System.Diagnostics;

/// <summary>
/// A simple resizable List (Vector) implementation.
/// </summary>
/// We use an array to store the data, with an element counter to 
/// <typeparam name="T"></typeparam>
<<<<<<< HEAD
public class List<T> : IListAlike<T>
{
=======
public class List<T>: IListAlike<T> {
>>>>>>> 74ee83b57139ec6424e0320708b3e9dd6631bcd1
    #region Fields
    private T[] data; // An array to store the data
    private int count; // The number of elements in the list
    #endregion

    #region Constructors
    /// <summary>
    /// Creates an empty list.
    /// </summary>
    public List()
    {
        data = new T[0];
        count = 0;
    }

    /// <summary>
    /// Creates a list with a specified capacity. 
    /// We guarantee that the list will not resize until it reaches the specified capacity.
    /// </summary>
    /// <param name="capacity"></param>
    public List(int capacity)
    {
        data = new T[capacity];
        count = 0;
    }

    /// <summary>
    /// Creates a list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public List(IListAlike<T> other) {
        data = new T[other.Length];
        count = 0;
        Extend(other);
    }
    #endregion

    #region Internal Methods
    /// <summary>
    /// Get the next capacity for the list.
    /// </summary>
    /// <param name="new_elem_count">next number of elements</param>
    /// <returns></returns>
    private int GetNextAllocSize(int new_elem_count)
    {
        int newSize = data.Length == 0 ? 1 : data.Length;
        while (new_elem_count >= newSize)
        {
            newSize *= 2;
        }
        return newSize;
    }

    /// <summary>
    /// Resize the list to the next capacity.
    /// </summary>
    /// <param name="next">next number of elements</param>
    private void Resize(int next)
    {
        int newSize = GetNextAllocSize(next);
        T[] newData = new T[newSize];
        for (int i = 0; i < data.Length; i++)
        {
            newData[i] = data[i];
        }
        data = newData;
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
    public T Get(int index)
    {
        if (index < 0 || index >= count) throw new IndexOutOfRangeException();
        return data[index];
    }

    /// <summary>
    /// Appends a value to the end of the list.
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="ArgumentNullException">If value is null.</exception>
    public void Append(T value)
    {
        if (value == null) throw new ArgumentNullException();
        if (count == data.Length)
        {
            Resize(count + 1);
        }
        data[count] = value;
        count++;
    }

    /// <summary>
    /// Extends the list with the elements of another list.
    /// </summary>
    /// <param name="other"></param>
    public void Extend(IListAlike<T> other)
    {
        int newCount = count + other.Length;
        if (newCount > data.Length)
        {
            Resize(newCount);
        }
        for (int i = 0; i < other.Length; i++)
        {
            data[count + i] = other.Get(i);
        }
        count = newCount;
    }

    /// <summary>
    /// Inserts a value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="value"></param>
    /// <exception cref="IndexOutOfRangeException">If the index is out of bounds or negative.</exception>
    /// <exception cref="ArgumentNullException">If value is null.</exception>
    public void Insert(int index, T value)
    {
        if (index > count || index < 0) throw new IndexOutOfRangeException();
        if (value == null) throw new ArgumentNullException();
        if (count == data.Length)
        {
            Resize(count + 1);
        }

        for (int i = count; i > index; i--)
        {
            data[i] = data[i - 1];
        }
        data[index] = value;
        count++;
    }

    /// <summary>
    /// Removes the value at the specified index.
    /// </summary>
    /// <param name="index"></param>
    public void RemoveAt(int index)
    {
        if (index >= count || index < 0) throw new IndexOutOfRangeException();
        for (int i = index; i < count - 1; i++)
        {
            data[i] = data[i + 1];
        }
        count--;
    }

    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the value was found and removed, false otherwise.</returns>
    public bool Remove(T value) {
        int index = IndexOf(value);
        if (index == -1) {
            return false;
        }
        RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Removes the first occurrence of the specified value.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>-1 if not found, index otherwise</returns>
    /// <exception cref="ArgumentNullException">if value is null</exception>
    public int IndexOf(T value)
    {
        if (value == null) throw new ArgumentNullException();

        for (int i = 0; i < count; i++)
        {
            T elem = data[i];
            if (elem == null) throw new UnreachableException();
            if (elem.Equals(value))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>
    /// Filters the list based on a predicate. The resulting list contains only the elements for which the predicate returns true.
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public IListAlike<T> Filter(IListAlike<T>.MapFunc<bool> predicate)
    {
        List<T> result = new List<T>();
        for (int i = 0; i < count; i++)
        {
            if (predicate(data[i]))
            {
                result.Append(data[i]);
            }
        }
        return result;
    }

    /// <summary>
    /// Sorts the list based on a comparison function. Algorithm is implementation-specific.
    /// </summary>
    /// <param name="compare"></param>
    public void Sort(IListAlike<T>.Compare compare)
    {
        for (int i = 0; i < count; i++)
        {
            for (int j = i + 1; j < count; j++)
            {
                if (compare(data[i], data[j]) > 0)
                {
                    T temp = data[i];
                    data[i] = data[j];
                    data[j] = temp;
                }
            }
        }
    }

    /// <summary>
    /// Maps the list to another list based on a mapping function.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="map"></param>
    /// <returns></returns>
    public IListAlike<U> Map<U>(IListAlike<T>.MapFunc<U> map)
    {
        List<U> result = new List<U>(count);
        for (int i = 0; i < count; i++)
        {
            result.Append(map(data[i]));
        }
        return result;
    }

    /// <summary>
    /// Iterates over the elements of the list.
    /// </summary>
    /// <param name="fn"></param>
    public void ForEach(IListAlike<T>.IterFunc fn)
    {
        for (int i = 0; i < count; i++)
        {
            fn(data[i]);
        }
    }

    /// <summary>
    /// Converts the list to a string, with each element separated by a comma.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        string s = "List[";
        for (int i = 0; i < count; i++)
        {
            T elem = data[i];
            if (elem == null) throw new UnreachableException();

            s += elem.ToString();
            if (i < count - 1)
            {
                s += ", ";
            }
        }
        s += "]";
        return s;
    }
    #endregion
}
using System.Collections;

public class CircularBuffer<T> : IEnumerable<T?>
{
    int index;
    T?[] items;

    public CircularBuffer(int size)
    {
        items = new T?[size];
    }

    public void Clear()
    {
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = default;
        }
    }

    public void Add(T? item)
    {
        items[index] = item;
        index++;
        if (index >= items.Length)
        {
            index = 0;
        }
    }

    public IEnumerator<T?> GetEnumerator()
    {
        return new CircularBufferEnumator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    class CircularBufferEnumator : IEnumerator<T?>
    {
        int i = -1;
        public T? Current => buffer.items[Util.Mod(buffer.index - i - 1, buffer.items.Length)];

        object IEnumerator.Current => Current ?? new object();

        CircularBuffer<T> buffer;
        public CircularBufferEnumator(CircularBuffer<T> buffer)
        {
            this.buffer = buffer;
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            i++;
            return i < buffer.items.Length;
        }

        public void Reset()
        {
            i = 0;
        }
    }
}
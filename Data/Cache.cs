using System;

public class Cache<K,V> 
    where K : struct
    where V : class 
{
    Dictionary<K, WeakReference<V>> cache;
    Func<K,V> createDelegate;

    public Cache(Func<K,V> create)
    {
        cache = new();
        createDelegate = create;
    }

    public V Get(K key)
    {
        bool res = cache[key].TryGetTarget(out V? v);
        if (!res || v == null)
        {
            var newItem = createDelegate.Invoke(key);
            cache[key] = new WeakReference<V>(newItem);
            return newItem;
        }

        return v;
    }
}
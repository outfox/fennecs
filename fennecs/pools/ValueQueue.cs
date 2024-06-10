#if EXPERIMENTAL
using System.Collections;
using System.Numerics;

namespace fennecs.pools;

internal class ValueQueue<T> : IEnumerable<T> where T : struct
{
    private int _head;
    private int _tail;
    private T[] _data;
    public ValueQueue(int initialCapacity)
    {
        _data = new T[initialCapacity];
        _head = 0;
        _tail = _data.Length;
    }

    public int Count => _tail > _head ? _tail - _head : _data.Length - _head + _tail;

    private bool IsEmpty => _head == _tail;
    private bool IsFull => _tail == _data.Length || _tail == _head;
    private int Free => _data.Length - Count;
    private bool IsWrapped => _head > _tail;

    #region Queue Operations
    internal void Clear()
    {
        _head = 0;
        _tail = 1;
    }
    
    internal T Dequeue()
    {
        if (IsEmpty) throw new InvalidOperationException("Queue is empty");
        
        var value = _data[_head++];
        _head %= _data.Length;
        return value;
    }
    
    internal int Dequeue(int amount, PooledList<T> destination)
    {
        // If the head is greater than the tail, take rest and maybe wrap around
        if (_head > _tail)
        {
            var rest = _data.Length - _head;
            if (rest <= amount)
            {
                // Wrap the tail around to start and enqueue all the data in rest
                destination.AddRange(_data.AsSpan(_head, rest));
                amount -= rest;
                _head = 0;
            }
            else
            {
                destination.AddRange(_data.AsSpan(_head, amount));
                _head += amount;
                return amount;
            }
        }

        // If the head is less than the tail, just copy the data
        var count = Math.Min(_tail - _head, amount);
        if (count == 0) return 0;

        destination.AddRange(_data.AsSpan(_head, count));
        _head += count;
        return count;
    }

    internal void Enqueue(in T value)
    {
        if (IsFull) Grow(1);
        
        _data[_tail++] = value;
        _tail %= _data.Length;
    }

    internal void Enqueue(Span<T> values)
    {
        if (Free < values.Length)
        {
            Grow(values.Length);
        }

        if (_head > _tail)
        {
            // If the head is greater than the tail, we need to wrap around
            var rest = _data.Length - _tail;
            if (rest <= values.Length)
            {
                // Wrap the tail around to start and enqueue all the data in rest
                values[..rest].CopyTo(_data.AsSpan(_tail));
                values = values[rest..];
                _tail = 0;
            }
        }

        values.CopyTo(_data.AsSpan(_tail));
        _tail += values.Length;
    }
    
    #endregion

    #region Capacity & Compaction
    private void Compact()
    {
        if (!IsWrapped) return;
        
        var span = _data.AsSpan();
        span[_head..].CopyTo(span[_tail..]);
            
        _tail = _data.Length - _head;
        _head = 0;
    }

    private void Grow(int valuesLength)
    {
        Compact();
        
        var newCapacity = (int)BitOperations.RoundUpToPowerOf2((uint)(_data.Length + valuesLength));
        Array.Resize(ref _data, newCapacity);
    }
    
    #endregion

    #region Enumerable
    public IEnumerator<T> GetEnumerator()
    {
        if (_head < _tail)
        {
            for (var i = _head; i < _tail; i++)
            {
                yield return _data[i];
            }
        }
        else
        {
            for (var i = _head; i < _data.Length; i++)
            {
                yield return _data[i];
            }

            for (var i = 0; i < _tail; i++)
            {
                yield return _data[i];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    #endregion
}
#endif
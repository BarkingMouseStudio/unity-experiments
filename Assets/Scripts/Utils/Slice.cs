using System;
using System.Collections;
using System.Collections.Generic;

public class Slice<T> : IEnumerable<T> {

  private ArraySegment<T> segment;

  public Slice(T[] array) {
    this.segment = new ArraySegment<T>(array, 0, array.Length);
  }

  public Slice(T[] array, int offset, int count) {
    this.segment = new ArraySegment<T>(array, offset, count);
  }

  public int Count {
    get {
      return segment.Count;
    }
  }

  public T this[int index] {
    get {
      if (index >= segment.Count) {
        throw new IndexOutOfRangeException();
      }
      return segment.Array[segment.Offset + index];
    }
    set {
      if (index >= segment.Count) {
        throw new IndexOutOfRangeException();
      }
      segment.Array[segment.Offset + index] = value;
    }
  }

  public T[] ToArray() {
    T[] temp = new T[segment.Count];
    Array.Copy(segment.Array, segment.Offset, temp, 0, segment.Count);
    return temp;
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  public IEnumerator<T> GetEnumerator() {
    for (int i = segment.Offset; i < segment.Offset + segment.Count; i++) {
      yield return segment.Array[i];
    }
  }
}

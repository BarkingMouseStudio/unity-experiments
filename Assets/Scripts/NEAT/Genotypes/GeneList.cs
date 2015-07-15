using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class GeneList<T> : ICollection<T>, IEnumerable<T>, IEnumerable where T : IHistoricalGene {

    private readonly SortedList<int, T> genes;

    public T this[int i] {
      get {
        return genes.Values[i];
      }
    }

    public int Count {
      get {
        return genes.Count;
      }
    }

    public bool IsReadOnly {
      get {
        return ((ICollection<KeyValuePair<int, T>>)genes).IsReadOnly;
      }
    }

    public GeneList() {
      genes = new SortedList<int, T>();
    }

    public GeneList(int capacity) {
      genes = new SortedList<int, T>(capacity);
    }

    public GeneList(GeneList<T> other) {
      genes = new SortedList<int, T>(other.ToDictionary(g => g.InnovationId));
    }

    public GeneList(IEnumerable<T> seq) {
      genes = new SortedList<int, T>(seq.ToDictionary(g => g.InnovationId));
    }

    public int IndexOf(int innovationId) {
      return genes.IndexOfKey(innovationId);
    }

    public int IndexOf(T gene) {
      return genes.IndexOfValue(gene);
    }

    public void Add(T gene) {
      genes.Add(gene.InnovationId, gene);
    }

    public bool Remove(T gene) {
      return genes.Remove(gene.InnovationId);
    }

    public void Clear() {
      genes.Clear();
    }

    public bool Contains(int innovationId) {
      return genes.ContainsKey(innovationId);
    }

    public bool Contains(T gene) {
      return genes.ContainsValue(gene);
    }

    public void CopyTo(T[] arr, int i) {
      genes.Values.CopyTo(arr, i);
    }

    public void CopyTo(int[] arr, int i) {
      genes.Keys.CopyTo(arr, i);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return genes.Values.GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator() {
      return genes.Values.GetEnumerator();
    }
  }
}

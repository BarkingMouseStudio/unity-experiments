using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class GeneList<TGene> : ICollection<TGene>, IEnumerable<TGene>, IEnumerable where TGene : IHistoricalGene {

    private readonly SortedList<int, TGene> genes;

    public TGene this[int innovationId] {
      get {
        return genes[innovationId];
      }
      set {
        genes[innovationId] = value;
      }
    }

    public int Count {
      get {
        return genes.Count;
      }
    }

    public bool IsReadOnly {
      get {
        return ((ICollection<KeyValuePair<int, TGene>>)genes).IsReadOnly;
      }
    }

    public IList<int> InnovationIds {
      get {
        return genes.Keys;
      }
    }

    public IList<TGene> Genes {
      get {
        return genes.Values;
      }
    }

    public GeneList() {
      genes = new SortedList<int, TGene>();
    }

    public GeneList(int capacity) {
      genes = new SortedList<int, TGene>(capacity);
    }

    public GeneList(GeneList<TGene> other) {
      genes = new SortedList<int, TGene>(other.ToDictionary(g => g.InnovationId));
    }

    public GeneList(IEnumerable<TGene> seq) {
      genes = new SortedList<int, TGene>(seq.ToDictionary(g => g.InnovationId));
    }

    public TGene ElementAt(int index) {
      return genes.Values[index];
    }

    public int IndexOf(int innovationId) {
      return genes.IndexOfKey(innovationId);
    }

    public int IndexOf(TGene gene) {
      return genes.IndexOfValue(gene);
    }

    public void Add(TGene gene) {
      genes.Add(gene.InnovationId, gene);
    }

    public bool Remove(int innovationId) {
      return genes.Remove(innovationId);
    }

    public bool Remove(TGene gene) {
      return genes.Remove(gene.InnovationId);
    }

    public void Clear() {
      genes.Clear();
    }

    public bool Contains(int innovationId) {
      return genes.ContainsKey(innovationId);
    }

    public bool Contains(TGene gene) {
      return genes.ContainsValue(gene);
    }

    public void CopyTo(TGene[] arr, int i) {
      genes.Values.CopyTo(arr, i);
    }

    public void CopyTo(int[] arr, int i) {
      genes.Keys.CopyTo(arr, i);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return genes.Values.GetEnumerator();
    }

    public IEnumerator<TGene> GetEnumerator() {
      return genes.Values.GetEnumerator();
    }
  }
}

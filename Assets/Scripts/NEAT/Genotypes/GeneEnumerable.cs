using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class GeneEnumerable<T> : IEnumerable<Tuple<HistoricalGeneTypes, int, int>> where T : IHistoricalGene{

    private readonly T[] genesA;
    private readonly T[] genesB;

    public GeneEnumerable(T[] genesA, T[] genesB) {
      this.genesA = genesA;
      this.genesB = genesB;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public IEnumerator<Tuple<HistoricalGeneTypes, int, int>> GetEnumerator() {
      int i = 0;
      int j = 0;

      while (i < genesA.Length && j < genesB.Length) {
        var geneA = genesA[i];
        var geneB = genesB[j];
        if (geneA.InnovationId == geneB.InnovationId) { // Aligned
          yield return Tuple.Of(HistoricalGeneTypes.Aligned, i, j);
          i++;
          j++;
        } else if (geneA.InnovationId < geneB.InnovationId) { // Disjoint
          yield return Tuple.Of(HistoricalGeneTypes.DisjointA, i, j);
          i++;
        } else if (geneB.InnovationId < geneA.InnovationId) { // Disjoint
          yield return Tuple.Of(HistoricalGeneTypes.DisjointB, i, j);
          j++;
        }
      }

      while (i < genesA.Length) { // Excess
        yield return Tuple.Of(HistoricalGeneTypes.ExcessA, i, j);
        i++;
      }

      while (j < genesB.Length) { // Excess
        yield return Tuple.Of(HistoricalGeneTypes.ExcessB, i, j);
        j++;
      }
    }
  }
}

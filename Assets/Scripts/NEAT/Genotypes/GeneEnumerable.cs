using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class GeneEnumerable<T> : IEnumerable<Tuple<HistoricalGeneTypes, int, int>> where T : IHistoricalGene{

    private readonly GeneList<T> genesA;
    private readonly GeneList<T> genesB;

    public GeneEnumerable(GeneList<T> genesA, GeneList<T> genesB) {
      this.genesA = genesA;
      this.genesB = genesB;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.GetEnumerator();
    }

    public IEnumerator<Tuple<HistoricalGeneTypes, int, int>> GetEnumerator() {
      int i = 0;
      int j = 0;

      var innovationIdsA = genesA.InnovationIds;
      var innovationIdsB = genesB.InnovationIds;

      int innovationIdA = -1;
      int innovationIdB = -1;

      while (i < innovationIdsA.Count && j < innovationIdsB.Count) {
        innovationIdA = innovationIdsA[i];
        innovationIdB = innovationIdsB[j];

        var geneA = genesA[innovationIdA];
        var geneB = genesB[innovationIdB];

        if (geneA.InnovationId == geneB.InnovationId) { // Aligned
          yield return Tuple.Of(HistoricalGeneTypes.Aligned, innovationIdA, innovationIdB);
          i++;
          j++;
        } else if (geneA.InnovationId < geneB.InnovationId) { // Disjoint
          yield return Tuple.Of(HistoricalGeneTypes.DisjointA, innovationIdA, innovationIdB);
          i++;
        } else if (geneB.InnovationId < geneA.InnovationId) { // Disjoint
          yield return Tuple.Of(HistoricalGeneTypes.DisjointB, innovationIdA, innovationIdB);
          j++;
        }
      }

      while (i < genesA.Count) { // Excess
        innovationIdA = innovationIdsA[i];
        yield return Tuple.Of(HistoricalGeneTypes.ExcessA, innovationIdA, innovationIdB);
        i++;
      }

      while (j < genesB.Count) { // Excess
        innovationIdB = innovationIdsB[j];
        yield return Tuple.Of(HistoricalGeneTypes.ExcessB, innovationIdA, innovationIdB);
        j++;
      }
    }
  }
}

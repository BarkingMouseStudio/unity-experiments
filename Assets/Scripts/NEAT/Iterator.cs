using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  // TODO: Consider converting to IEnumerable wrapper class for lists of genes
  public static class Iterator {

    public static IEnumerable<Tuple<HistoricalGeneTypes, int, int>> IterGenes<T>(List<T> genesA, List<T> genesB) where T : IHistoricalGene {
      int i = 0, j = 0;
      for (; i < genesA.Count && j < genesB.Count;) {
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

      for (; i < genesA.Count; i++) { // Excess
        yield return Tuple.Of(HistoricalGeneTypes.ExcessA, i, j);
      }

      for (; j < genesB.Count; j++) { // Excess
        yield return Tuple.Of(HistoricalGeneTypes.ExcessB, i, j);
      }
    }
  }
}

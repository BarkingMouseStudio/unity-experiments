using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public static class IEnumerableGeneListExtensions {

    public static GeneList<T> ToGeneList<T>(this IEnumerable<T> genes) where T : IHistoricalGene {
      return new GeneList<T>(genes);
    }
  }
}

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class GeneEnumerableTests {

    [Test]
    public void TestGeneEnumerable() {
      var innovationIdsA = new int[]{1, 2, 3, 4, 5, 8};
      var innovationIdsB = new int[]{1, 2, 3, 4, 5, 6, 7, 9, 10};

      var genesA = innovationIdsA.Select(i => new NeuronGene(i)).ToList();
      var genesB = innovationIdsB.Select(i => new NeuronGene(i)).ToList();

      var innovationIds = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

      var alignment = new HistoricalGeneTypes[]{
        HistoricalGeneTypes.Aligned,
        HistoricalGeneTypes.Aligned,
        HistoricalGeneTypes.Aligned,
        HistoricalGeneTypes.Aligned,
        HistoricalGeneTypes.Aligned,
        HistoricalGeneTypes.DisjointB,
        HistoricalGeneTypes.DisjointB,
        HistoricalGeneTypes.DisjointA,
        HistoricalGeneTypes.ExcessB,
        HistoricalGeneTypes.ExcessB,
      };

      int j = 0;
      foreach (var t in new GeneEnumerable<NeuronGene>(genesA, genesB)) {
        Assert.That(t.First == alignment[j]);

        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            Assert.That(
              genesA[t.Second].InnovationId == innovationIds[j] ||
              genesB[t.Third].InnovationId == innovationIds[j]
            );
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.ExcessA:
            Assert.That(genesA[t.Second].InnovationId == innovationIds[j]);
            break;
          case HistoricalGeneTypes.DisjointB:
          case HistoricalGeneTypes.ExcessB:
            Assert.That(genesB[t.Third].InnovationId == innovationIds[j]);
            break;
        }

        j++;
      }

      Assert.That(j == innovationIds.Length);
    }
  }
}

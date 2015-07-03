using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class CrossoverTests {

    [Test]
    public void TestMultipointCrossoverGenes() {
      var innovationIdsA = new int[]{1, 2, 3, 4, 5, 8};
      var a = innovationIdsA.Select(i => new NeuronGene(i)).ToList();

      var innovationIdsB = new int[]{1, 2, 3, 4, 5, 6, 7, 9, 10};
      var b = innovationIdsB.Select(i => new NeuronGene(i)).ToList();

      var innovationIds = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
      var genes = MultipointCrossover.CrossoverGenes(a, b, 0.5f, 0.5f);

      Assert.That(genes.Select(g => g.innovationId).SequenceEqual(innovationIds));
    }
  }
}

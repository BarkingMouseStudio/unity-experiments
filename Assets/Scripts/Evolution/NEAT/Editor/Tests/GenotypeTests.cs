using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class GenotypeTests {

    [Test]
    public void TestGenotype() {
      var genotype = new Genotype(3);
      var clonedGenotype = new Genotype(genotype);

      Assert.That(genotype != clonedGenotype);
      Assert.That(genotype.neuronGenes != clonedGenotype.neuronGenes);
      Assert.That(genotype.synapseGenes != clonedGenotype.synapseGenes);
      Assert.That(genotype.neuronGenes.SequenceEqual(clonedGenotype.neuronGenes));
      Assert.That(genotype.synapseGenes.SequenceEqual(clonedGenotype.synapseGenes));
      Assert.That(Genotype.Distance(genotype, clonedGenotype) == 0.0f);

      var otherGenotype = new Genotype(3);
      Assert.That(Genotype.Distance(genotype, otherGenotype) > 0.0f);
    }
  }
}

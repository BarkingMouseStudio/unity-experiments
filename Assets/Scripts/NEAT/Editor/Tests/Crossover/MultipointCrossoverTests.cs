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
    public void TestMultipointCrossover() {
      var innovationIdsA = new int[]{1, 2, 3, 4, 5, 8};
      var neuronGenesA = innovationIdsA.Select(i => new NeuronGene(i)).ToArray();

      var innovationIdsB = new int[]{1, 2, 3, 4, 5, 6, 7, 9, 10};
      var neuronGenesB = innovationIdsB.Select(i => new NeuronGene(i)).ToArray();

      var expectedInnovationIds = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

      var genotypeA = new Genotype(neuronGenesA, new SynapseGene[0]);
      var genotypeB = new Genotype(neuronGenesB, new SynapseGene[0]);

      var phenotypeA = new Phenotype(genotypeA, 0.5f, 1.0f, 180.0f);
      var phenotypeB = new Phenotype(genotypeB, 0.5f, 1.0f, 180.0f);

      var crossover = new MultipointCrossover();
      var offspring = crossover.Crossover(phenotypeA, phenotypeB);

      var offspringInnovationIds = offspring.NeuronGenes.Select(g =>
        g.InnovationId).ToArray();

      Assert.AreEqual(expectedInnovationIds, offspringInnovationIds);
    }
  }
}

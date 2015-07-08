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
      var neuronGenesA = innovationIdsA.Select(i => new NeuronGene(i)).ToList();

      var innovationIdsB = new int[]{1, 2, 3, 4, 5, 6, 7, 9, 10};
      var neuronGenesB = innovationIdsB.Select(i => new NeuronGene(i)).ToList();

      var innovationIds = new int[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10};

      var genotypeA = new Genotype(neuronGenesA, new List<SynapseGene>());
      var genotypeB = new Genotype(neuronGenesB, new List<SynapseGene>());

      var phenotypeA = new Phenotype(genotypeA, 0.5f, 1.0f, 180.0f);
      var phenotypeB = new Phenotype(genotypeB, 0.5f, 1.0f, 180.0f);

      var crossover = new MultipointCrossover();
      var genotypeC = crossover.Crossover(phenotypeA, phenotypeB);
      var neuronGenesC = genotypeC.neuronGenes;

      Assert.That(neuronGenesC.Select(g => g.InnovationId).SequenceEqual(innovationIds));
    }
  }
}

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class PopulationTests {

    [Test]
    public void TestPopulation() {
      var innovations = new Innovations();

      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(innovations.GetAddInitialNeuronInnovationId(i), i))
        .ToList();
      var synapseGenes = new List<SynapseGene>();
      var protoGenotype = new Genotype(neuronGenes, synapseGenes);

      var populationSize = 10;
      var population = new Population(populationSize, protoGenotype, innovations);
      Assert.That(population.Size == populationSize);
    }
  }
}

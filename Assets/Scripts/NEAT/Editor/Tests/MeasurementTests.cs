using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using NUnit.Framework;

namespace NEAT {

  [TestFixture]
  public class MeasurementTests {

    [Test]
    public void TestMeasurement() {
      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i, i))
        .ToList();
      var synapseGenes = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(i, 0, 1, true, 0.5f))
        .ToList();

      var genotypeA = new Genotype(neuronGenes, synapseGenes);
      var genotypeB = genotypeA.Clone();

      var measurement = new Measurement(1.0f, 1.0f, 1.0f);

      Assert.That(measurement.Distance(genotypeA, genotypeB) == 0.0f);

      neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i, i))
        .ToList();
      synapseGenes = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(Random.Range(0, 3),
          Random.Range(0, 3),
          Random.Range(0, 3),
          true))
        .ToList();

      var genotypeC = new Genotype(neuronGenes, synapseGenes);
      Assert.That(measurement.Distance(genotypeA, genotypeC) > 0.0f);
    }
  }
}

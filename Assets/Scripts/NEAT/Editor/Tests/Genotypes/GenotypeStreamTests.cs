using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class GenotypeStreamTests {

    Genotype protoGenotype;

    [SetUp]
    public void Init() {
      var innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId, NeuronType.HiddenNeuron);
      }).ToGeneList();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(i, i + 1);
        return new SynapseGene(inId, i, i + 1, true);
      }).ToGeneList();

      protoGenotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestGenotypeStream() {
      var stream = new GenotypeStream(protoGenotype);
      var genotypes = stream.Take(5);

      // Correct count
      Assert.AreEqual(5, genotypes.Count());

      // Each unique
      var distinct = genotypes.Distinct(new GenotypeEqualityComparer());
      Assert.AreEqual(5, distinct.Count());

      // Same shape
      var shape = genotypes.Distinct(new GenotypeShapeEqualityComparer());
      Assert.AreEqual(1, shape.Count());
    }
  }
}

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
      var innovations = new Innovations();

      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(innovations.GetAddInitialNeuronInnovationId(i), i))
        .ToList();
      var synapseGenes = new List<SynapseGene>();
      var genotype = new Genotype(neuronGenes, synapseGenes);
      var clonedGenotype = genotype.Clone();

      Assert.That(genotype != clonedGenotype);
      Assert.That(genotype.neuronGenes != clonedGenotype.neuronGenes);
      Assert.That(genotype.synapseGenes != clonedGenotype.synapseGenes);
      Assert.That(genotype.neuronGenes.SequenceEqual(clonedGenotype.neuronGenes));
      Assert.That(genotype.synapseGenes.SequenceEqual(clonedGenotype.synapseGenes));
    }

    [Test]
    public void TestGenotypeSerialization() {
      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i, i))
        .ToList();
      var synapseGenes = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(i, 0, 1, true, 0.5f))
        .ToList();
      var genotype = new Genotype(neuronGenes, synapseGenes);

      var json = genotype.ToJSON();

      var expectedJSON = "{\"neurons\":[{\"innovation\":0,\"id\":0,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":1,\"id\":1,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":2,\"id\":2,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5}],\"synapses\":[{\"innovation\":0,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":1,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":2,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}]}";
      Assert.That(json == expectedJSON);

      genotype = Genotype.FromJSON(json);

      Assert.That(genotype.neuronGenes.Count == 3);
      Assert.That(genotype.synapseGenes.Count == 3);
    }
  }
}

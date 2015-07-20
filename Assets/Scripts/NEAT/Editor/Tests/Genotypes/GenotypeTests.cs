using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class GenotypeTests {

    Genotype genotype;

    [SetUp]
    public void Init() {
      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i))
        .ToGeneList();
      var synapseGenes = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(i, 0, 1, true, 0.5f))
        .ToGeneList();
      genotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestGenotype_Clone() {
      var clonedGenotype = new Genotype(genotype);
      Assert.AreNotSame(genotype, clonedGenotype);
      Assert.AreNotSame(genotype.NeuronGenes, clonedGenotype.NeuronGenes);
      Assert.AreNotSame(genotype.SynapseGenes, clonedGenotype.SynapseGenes);

      var comparer = new GenotypeEqualityComparer();
      Assert.That(comparer.Equals(genotype, clonedGenotype));
    }

    [Test]
    public void TestGenotype_ToJSON() {
      var expectedJSON = "{\"neurons\":[{\"innovation\":0,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":1,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":2,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5}],\"synapses\":[{\"innovation\":0,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":1,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":2,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}]}";
      var json = JSON.Serialize(genotype.ToJSON());
      Assert.AreEqual(expectedJSON, json);
    }

    [Test]
    public void TestGenotype_FromJSON() {
      var json = "{\"neurons\":[{\"innovation\":0,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":1,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":2,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5}],\"synapses\":[{\"innovation\":0,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":1,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":2,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}]}";
      var deserializedGenotype = Genotype.FromJSON(JSON.Deserialize(json));

      var comparer = new GenotypeEqualityComparer();
      Assert.That(comparer.Equals(genotype, deserializedGenotype));
    }

    [Test]
    public void TestGenotype_RoundTripJSON() {
      var json = "{\"neurons\":[{\"innovation\":0,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":1,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5},{\"innovation\":2,\"a\":0.5,\"b\":0.5,\"c\":0.5,\"d\":0.5}],\"synapses\":[{\"innovation\":0,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":1,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true},{\"innovation\":2,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}]}";
      var deserializedGenotype = Genotype.FromJSON(JSON.Deserialize(json));
      var serializedGenotype = JSON.Serialize(deserializedGenotype.ToJSON());
      Assert.AreEqual(json, serializedGenotype, "JSON can convert round-trip");
    }
  }
}

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class SynapseGeneTests {

    [Test]
    public void TestSynapseGene() {
      var synapseGene = new SynapseGene(3, 0, 1, true, 0.5f);
      Assert.AreEqual(synapseGene.InnovationId, 3);
      Assert.AreEqual(synapseGene.fromNeuronId, 0);
      Assert.AreEqual(synapseGene.toNeuronId, 1);
      Assert.AreEqual(synapseGene.isEnabled, true);
      Assert.AreEqual(synapseGene.weight, 0.5f);

      var synapseGeneRand = SynapseGene.FromPrototype(synapseGene);
      Assert.AreEqual(synapseGeneRand.InnovationId, synapseGene.InnovationId);
      Assert.AreEqual(synapseGeneRand.fromNeuronId, synapseGene.fromNeuronId);
      Assert.AreEqual(synapseGeneRand.toNeuronId, synapseGene.toNeuronId);
      Assert.AreEqual(synapseGeneRand.isEnabled, synapseGene.isEnabled);
      Assert.AreNotEqual(synapseGeneRand.weight, synapseGene.weight);
    }

    [Test]
    public void TestSynapseGene_ToJSON() {
      var synapseGene = new SynapseGene(3, 0, 1, true, 0.5f);
      var json = JSON.Serialize(synapseGene.ToJSON());
      Assert.AreEqual("{\"innovation\":3,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}", json);
    }

    [Test]
    public void TestSynapseGene_FromJSON() {
      var json = JSON.Deserialize("{\"innovation\":3,\"weight\":0.5,\"from\":0,\"to\":1,\"enabled\":true}");
      var synapseGene = SynapseGene.FromJSON(json);
      var expected = new SynapseGene(3, 0, 1, true, 0.5f);
      Assert.AreEqual(expected, synapseGene);
    }
  }
}

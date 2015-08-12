using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class NeuronGeneTests {

    [Test]
    public void TestNeuronGene() {
      var neuronGene = new NeuronGene(3, NeuronType.HiddenNeuron, 0.1f, 0.2f, 0.3f, 0.4f, 0.0f, 1.0f);
      Assert.AreEqual(neuronGene.InnovationId, 3);
      Assert.AreEqual(neuronGene.a, 0.1f);
      Assert.AreEqual(neuronGene.b, 0.2f);
      Assert.AreEqual(neuronGene.c, 0.3f);
      Assert.AreEqual(neuronGene.d, 0.4f);

      var neuronGeneRand = NeuronGene.FromPrototype(neuronGene);
      Assert.AreEqual(neuronGeneRand.InnovationId, 3);
      Assert.AreNotEqual(neuronGeneRand.a, neuronGene.a);
      Assert.AreNotEqual(neuronGeneRand.b, neuronGene.b);
      Assert.AreNotEqual(neuronGeneRand.c, neuronGene.c);
      Assert.AreNotEqual(neuronGeneRand.d, neuronGene.d);
    }

    [Test]
    public void TestNeuronGene_ToJSON() {
      var neuronGene = new NeuronGene(3, NeuronType.HiddenNeuron, 0.1f, 0.2f, 0.3f, 0.4f, 0.0f, 1.0f);
      var json = JSON.Serialize(neuronGene.ToJSON());
      var expected = "{\"innovation\":3,\"type\":4,\"mean\":0.5,\"sigma\":0,\"a\":0.1,\"b\":0.2,\"c\":0.3,\"d\":0.4}";
      Assert.AreEqual(expected, json);
    }

    [Test]
    public void TestNeuronGene_FromJSON() {
      var json = JSON.Deserialize("{\"innovation\":3,\"type\":4,\"mean\":0.5,\"sigma\":0,\"a\":0.1,\"b\":0.2,\"c\":0.3,\"d\":0.4}");
      var neuronGene = NeuronGene.FromJSON(json);
      var expected = new NeuronGene(3, NeuronType.HiddenNeuron, 0.1f, 0.2f, 0.3f, 0.4f, 0.0f, 1.0f);
      Assert.AreEqual(expected, neuronGene);
    }
  }
}

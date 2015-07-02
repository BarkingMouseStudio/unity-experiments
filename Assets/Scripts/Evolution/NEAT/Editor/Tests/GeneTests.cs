using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class GeneTests {

    [Test]
    public void TestNeuronGene() {
      var neuronGene = new NeuronGene(3, 0.1f, 0.2f, 0.3f, 0.4f);
      Assert.That(neuronGene.innovationId == 3);
      Assert.That(neuronGene.a == 0.1f);
      Assert.That(neuronGene.b == 0.2f);
      Assert.That(neuronGene.c == 0.3f);
      Assert.That(neuronGene.d == 0.4f);

      var neuronGeneRand = neuronGene.Randomize();
      Assert.That(neuronGeneRand.innovationId == 3);
      Assert.That(neuronGeneRand.a != neuronGene.a);
      Assert.That(neuronGeneRand.b != neuronGene.b);
      Assert.That(neuronGeneRand.c != neuronGene.c);
      Assert.That(neuronGeneRand.d != neuronGene.d);

      var neuronGenePert = neuronGene.Perturb();
      Assert.That(neuronGenePert.innovationId == 3);
      Assert.That(neuronGenePert.a != neuronGene.a);
      Assert.That(neuronGenePert.b != neuronGene.b);
      Assert.That(neuronGenePert.c != neuronGene.c);
      Assert.That(neuronGenePert.d != neuronGene.d);
    }

    [Test]
    public void TestSynapseGene() {
      var synapseGene = new SynapseGene(3, 0, 1, true, 0.5f);
      Assert.That(synapseGene.innovationId == 3);
      Assert.That(synapseGene.fromId == 0);
      Assert.That(synapseGene.toId == 1);
      Assert.That(synapseGene.isEnabled == true);
      Assert.That(synapseGene.weight == 0.5f);

      var synapseGeneRand = synapseGene.Randomize();
      Assert.That(synapseGeneRand.innovationId == 3);
      Assert.That(synapseGeneRand.fromId == 0);
      Assert.That(synapseGeneRand.toId == 1);
      Assert.That(synapseGeneRand.weight != synapseGene.weight);

      var synapseGenePert = synapseGene.Perturb();
      Assert.That(synapseGenePert.innovationId == 3);
      Assert.That(synapseGenePert.fromId == 0);
      Assert.That(synapseGenePert.toId == 1);
    }
  }
}

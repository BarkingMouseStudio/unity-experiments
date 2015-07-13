using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class InnovationCollectionTests {

    [Test]
    public void TestInnovationCollection() {
      var innovations = new InnovationCollection();

      // Make sure innovation ids are ascending
      var a = innovations.GetInitialNeuronInnovationId(0);
      var b = innovations.GetInitialNeuronInnovationId(1);
      var c = innovations.GetInitialNeuronInnovationId(2);
      Assert.That(a == 0);
      Assert.That(b == 1);
      Assert.That(c == 2);

      var sA = innovations.GetSynapseInnovationId(a, b);
      var sB = innovations.GetSynapseInnovationId(b, c);
      var sC = innovations.GetSynapseInnovationId(c, a);
      Assert.That(sA == 3);
      Assert.That(sB == 4);
      Assert.That(sC == 5);

      var d = innovations.GetNeuronInnovationId(a, b, sA);
      var e = innovations.GetNeuronInnovationId(b, c, sB);
      var f = innovations.GetNeuronInnovationId(c, d, sC);
      Assert.That(d == 6);
      Assert.That(e == 8);
      Assert.That(f == 10);

      // Make sure duplicate innovations receive their original innovation id
      var a0 = innovations.GetInitialNeuronInnovationId(0);
      var b0 = innovations.GetInitialNeuronInnovationId(1);
      var c0 = innovations.GetInitialNeuronInnovationId(2);
      Assert.That(a0 == 0);
      Assert.That(b0 == 1);
      Assert.That(c0 == 2);

      var sA0 = innovations.GetSynapseInnovationId(a0, b0);
      var sB0 = innovations.GetSynapseInnovationId(b0, c0);
      var sC0 = innovations.GetSynapseInnovationId(c0, a0);
      Assert.That(sA0 == 3);
      Assert.That(sB0 == 4);
      Assert.That(sC0 == 5);

      var d0 = innovations.GetNeuronInnovationId(a0, b0, sA0);
      var e0 = innovations.GetNeuronInnovationId(b0, c0, sB0);
      var f0 = innovations.GetNeuronInnovationId(c0, d0, sC0);
      Assert.That(d0 == 6);
      Assert.That(e0 == 8);
      Assert.That(f0 == 10);

      // Test prune
      var genotypes = Enumerable.Range(0, 3).Select(_0 => {
        var neuronGeneA = new NeuronGene(a);
        var neuronGeneB = new NeuronGene(b);
        var synapseGeneA = new SynapseGene(sA, a, b, true);
        var synapseGeneB = new SynapseGene(sB, b, c, true);
        var neuronGenes = new []{neuronGeneA, neuronGeneB};
        var synapseGenes = new []{synapseGeneA, synapseGeneB};
        return new Genotype(neuronGenes, synapseGenes);
      }).ToList();

      var prunedCount = innovations.Prune(genotypes);
      Assert.That(innovations.Count == 4);
      Assert.That(prunedCount == 5);
    }
  }
}

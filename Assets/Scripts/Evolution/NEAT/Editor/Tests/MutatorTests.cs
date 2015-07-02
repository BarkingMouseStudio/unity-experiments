using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class MutatorTests {

    [Test]
    public void TestAddNeuronMutator() {
      var mutator = new AddNeuronMutator(1.0f);

      var neuronGenes = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i))
        .ToList();

      var synapseGenes = Enumerable.Range(0, 3)
        .Select(i => {
          var inId = innovations.GetAddSynapseInnovationId(0, 1);
          return new SynapseGene(inId, 0, 1, true);
          })
        .ToList();

      var innovations = new Innovations(6);

      var genotype = new Genotype(neuronGenes, synapseGenes);
      mutator.Mutate(genotype, innovations);

      Assert.That(false);
    }

    [Test]
    public void TestAddSynapseMutator() {
      Assert.That(false);
    }

    [Test]
    public void TestPerturbNeuronMutator() {
      Assert.That(false);
    }

    [Test]
    public void TestPerturbSynapseMutator() {
      Assert.That(false);
    }

    [Test]
    public void TestReplaceNeuronMutator() {
      Assert.That(false);
    }

    [Test]
    public void TestReplaceSynapseMutator() {
      Assert.That(false);
    }
  }
}

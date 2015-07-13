using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class MutationCollectionTests {

    InnovationCollection innovations;
    Genotype protoGenotype;

    [SetUp]
    public void Init() {
      innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId);
      }).ToArray();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(-1, 1);
        return new SynapseGene(inId, -1, 1, true);
      }).ToArray();

      protoGenotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestMutationCollection() {
      var genotypes = Enumerable.Range(0, 3).Select(_ =>
        Genotype.FromPrototype(protoGenotype)).ToArray();

      var mutationCollection = new MutationCollection(
        new NEAT.AddNeuronMutator(1.0f, innovations),
        new NEAT.AddSynapseMutator(1.0f, innovations),
        new NEAT.PerturbNeuronMutator(1.0f, 0.5f),
        new NEAT.PerturbSynapseMutator(1.0f, 0.5f),
        new NEAT.ReplaceNeuronMutator(1.0f),
        new NEAT.ReplaceSynapseMutator(1.0f),
        new NEAT.ToggleSynapseMutator(1.0f)
      );

      var results = mutationCollection.Mutate(genotypes);

      Assert.AreEqual(3, results.addedNeurons);
      Assert.AreEqual(6, results.addedSynapses);
      Assert.AreEqual(12, results.perturbedNeurons);
      Assert.AreEqual(15, results.perturbedSynapses);
      Assert.AreEqual(12, results.replacedNeurons);
      Assert.AreEqual(15, results.replacedSynapses);
      Assert.AreEqual(15, results.toggledSynapses);
    }
  }
}

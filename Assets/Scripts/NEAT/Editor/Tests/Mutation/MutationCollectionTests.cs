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
      var genotypes = Enumerable.Range(0, 100).Select(_ =>
        Genotype.FromPrototype(protoGenotype)).ToArray();

      var mutations = new NEAT.MutationCollection();
      mutations.Add(0.75f, new NEAT.AddNeuronMutator(innovations));
      mutations.Add(0.25f, new NEAT.AddSynapseMutator(innovations));

      var results = mutations.Mutate(genotypes);

      Assert.AreEqual(75, results.addedNeurons, 75 * 0.15f);
      Assert.AreEqual(175, results.addedSynapses, 175 * 0.15f);
    }
  }
}

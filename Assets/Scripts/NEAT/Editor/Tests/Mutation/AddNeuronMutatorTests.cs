using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class AddNeuronMutatorTests {

    Genotype genotype;
    InnovationCollection innovations;

    [SetUp]
    public void Init() {
      innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId, NeuronType.HiddenNeuron);
      }).ToGeneList();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(i, i + 1);
        return new SynapseGene(inId, i, i + 1, true);
      }).ToGeneList();

      genotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestAddNeuronMutator() {
      var mutator = new AddNeuronMutator(innovations);

      var mutatedGenotype = new Genotype(genotype);
      var mutationResults = new MutationResults();
      mutator.Mutate(mutatedGenotype, mutationResults);

      Assert.AreEqual(1, mutationResults.addedNeurons);
      Assert.AreEqual(2, mutationResults.addedSynapses);

      // Compare gene counts
      Assert.AreEqual(1, mutatedGenotype.NeuronCount - genotype.NeuronCount);
      Assert.AreEqual(2, mutatedGenotype.SynapseCount - genotype.SynapseCount);

      // Compare innovation ids
      var aN = new []{0, 1, 2, 6};
      var bN = mutatedGenotype.NeuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = new []{3, 4, 5, 6, 7};
      var bS = mutatedGenotype.SynapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));
    }
  }
}

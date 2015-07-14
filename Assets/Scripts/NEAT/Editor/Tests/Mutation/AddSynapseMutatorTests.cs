using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class AddSynapseTests {

    Genotype genotype;
    InnovationCollection innovations;

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

      genotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestAddSynapseMutator() {
      var mutator = new AddSynapseMutator(innovations);

      var mutatedGenotype = new Genotype(genotype);
      var mutationResults = new MutationResults();
      mutator.Mutate(mutatedGenotype, mutationResults);

      Assert.That(mutationResults.addedSynapses == 1);

      // Compare gene counts
      Assert.AreEqual(mutatedGenotype.NeuronCount, genotype.NeuronCount);
      Assert.AreEqual(mutatedGenotype.SynapseCount - genotype.SynapseCount, 1);

      // Compare innovation ids
      var aN = new []{0, 1, 2};
      var bN = mutatedGenotype.NeuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = new []{3, 3, 3, 4};
      var bS = mutatedGenotype.SynapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));
    }
  }
}

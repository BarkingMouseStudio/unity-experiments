using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class PerturbSynapseMutatorTests {

    Genotype genotype;

    [SetUp]
    public void Init() {
      var innovations = new InnovationCollection();

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
    public void TestPerturbSynapseMutator() {
      var mutator = new PerturbSynapseMutator(1.0f, 0.25f);

      var mutatedGenotype = new Genotype(genotype);
      var mutationResults = new MutationResults();
      mutator.Mutate(mutatedGenotype, mutationResults);

      Assert.AreEqual(3, mutationResults.perturbedSynapses);

      // Compare gene counts
      var comparer = new GenotypeShapeEqualityComparer();
      Assert.That(comparer.Equals(mutatedGenotype, genotype));
    }
  }
}

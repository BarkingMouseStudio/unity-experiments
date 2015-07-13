using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class PerturbNeuronMutatorTests {

    Genotype genotype;

    [SetUp]
    public void Init() {
      var innovations = new InnovationCollection();

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
    public void TestPerturbNeuronMutator() {
      var mutator = new PerturbNeuronMutator(1.0f, 0.25f);

      var mutatedGenotype = new Genotype(genotype);
      var mutationResults = new MutationResults();
      mutator.MutateGenotype(mutatedGenotype, mutationResults);

      Assert.AreEqual(mutationResults.perturbedNeurons, 3);

      // Compare gene counts
      var comparer = new GenotypeShapeEqualityComparer();
      Assert.That(comparer.Equals(mutatedGenotype, genotype));
    }
  }
}
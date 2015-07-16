using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Mutation")]
  public class ReplaceNeuronMutatorTests {

    Genotype genotype;

    [SetUp]
    public void Init() {
      var innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId);
      }).ToGeneList();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(i, i + 1);
        return new SynapseGene(inId, i, i + 1, true);
      }).ToGeneList();

      genotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestReplaceNeuronMutator() {
      var mutator = new ReplaceNeuronMutator(1.0f);

      var mutatedGenotype = new Genotype(genotype);
      var mutationResults = new MutationResults();
      mutator.Mutate(mutatedGenotype, mutationResults);

      Assert.AreEqual(mutationResults.replacedNeurons, 3);

      // Compare gene counts
      var comparer = new GenotypeShapeEqualityComparer();
      Assert.That(comparer.Equals(mutatedGenotype, genotype));
    }
  }
}

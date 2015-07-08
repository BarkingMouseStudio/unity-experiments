using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class MutatorTests {

    private Genotype NewGenotype(Innovations innovations) {
      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId);
      }).ToList();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(-1, 1);
        return new SynapseGene(inId, -1, 1, true);
      }).ToList();

      return new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestAddNeuronMutator() {
      var mutator = new AddNeuronMutator(1.0f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count - genotype.neuronGenes.Count == 1);

      Assert.That(mutatedGenotype.synapseGenes.Count - genotype.synapseGenes.Count == 2);

      // Compare innovation ids
      var aN = new []{0, 1, 2, 4};
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = new []{3, 3, 3, 4, 5};
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));
    }

    [Test]
    public void TestAddSynapseMutator() {
      var mutator = new AddSynapseMutator(1.0f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count == genotype.neuronGenes.Count);

      Assert.That(mutatedGenotype.synapseGenes.Count - genotype.synapseGenes.Count == 1);

      // Compare innovation ids
      var aN = new []{0, 1, 2};
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = new []{3, 3, 3, 4};
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));
    }

    [Test]
    public void TestPerturbNeuronMutator() {
      var mutator = new PerturbNeuronMutator(1.0f, 0.25f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count == genotype.neuronGenes.Count);

      Assert.That(mutatedGenotype.synapseGenes.Count == genotype.synapseGenes.Count);

      // Compare innovation ids
      var aN = genotype.neuronGenes.Select(g => g.InnovationId);
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = genotype.synapseGenes.Select(g => g.InnovationId);
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));

      // Compare fields
      Assert.That(!genotype.neuronGenes.SequenceEqual(mutatedGenotype.neuronGenes));
      Assert.That(genotype.synapseGenes.SequenceEqual(mutatedGenotype.synapseGenes));
    }

    [Test]
    public void TestPerturbSynapseMutator() {
      var mutator = new PerturbSynapseMutator(1.0f, 0.25f, 0.2f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count == genotype.neuronGenes.Count);

      Assert.That(mutatedGenotype.synapseGenes.Count == genotype.synapseGenes.Count);

      // Compare innovation ids
      var aN = genotype.neuronGenes.Select(g => g.InnovationId);
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = genotype.synapseGenes.Select(g => g.InnovationId);
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));

      // Compare fields
      Assert.That(genotype.neuronGenes.SequenceEqual(mutatedGenotype.neuronGenes));
      Assert.That(!genotype.synapseGenes.SequenceEqual(mutatedGenotype.synapseGenes));
    }

    [Test]
    public void TestReplaceNeuronMutator() {
      var mutator = new ReplaceNeuronMutator(1.0f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count == genotype.neuronGenes.Count);

      Assert.That(mutatedGenotype.synapseGenes.Count == genotype.synapseGenes.Count);

      // Compare innovation ids
      var aN = genotype.neuronGenes.Select(g => g.InnovationId);
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = genotype.synapseGenes.Select(g => g.InnovationId);
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));

      // Compare fields
      Assert.That(!genotype.neuronGenes.SequenceEqual(mutatedGenotype.neuronGenes));
      Assert.That(genotype.synapseGenes.SequenceEqual(mutatedGenotype.synapseGenes));
    }

    [Test]
    public void TestReplaceSynapseMutator() {
      var mutator = new ReplaceSynapseMutator(1.0f, 0.2f);
      var innovations = new Innovations();

      var genotype = NewGenotype(innovations);
      var mutatedGenotype = mutator.Mutate(genotype, innovations);

      // Compare gene counts
      Assert.That(mutatedGenotype.neuronGenes.Count == genotype.neuronGenes.Count);

      Assert.That(mutatedGenotype.synapseGenes.Count == genotype.synapseGenes.Count);

      // Compare innovation ids
      var aN = genotype.neuronGenes.Select(g => g.InnovationId);
      var bN = mutatedGenotype.neuronGenes.Select(g => g.InnovationId);
      Assert.That(aN.SequenceEqual(bN));

      var aS = genotype.synapseGenes.Select(g => g.InnovationId);
      var bS = mutatedGenotype.synapseGenes.Select(g => g.InnovationId);
      Assert.That(aS.SequenceEqual(bS));

      // Compare fields
      Assert.That(genotype.neuronGenes.SequenceEqual(mutatedGenotype.neuronGenes));
      Assert.That(!genotype.synapseGenes.SequenceEqual(mutatedGenotype.synapseGenes));
    }
  }
}

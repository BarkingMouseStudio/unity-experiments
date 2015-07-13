using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class NetworkIOTests {

    [Test]
    [Ignore("Needs improvement")]
    public void TestNetworkIO() {
      // var innovations = new InnovationCollection();
      // var neuronGenes = Enumerable.Range(0, NetworkIO.inNeuronCount + NetworkIO.outNeuronCount)
      //   .Select(i => NeuronGene.Random(innovations.GetInitialNeuronInnovationId(i)))
      //   .ToArray();
      // var genotype = new Genotype(neuronGenes, new SynapseGene[0]);
      //
      // var mutators = new IMutator[]{
      //   new AddNeuronMutator(1.0f, innovations),
      //   new AddSynapseMutator(1.0f, innovations),
      // };
      //
      // var generationCount = 100;
      // for (int i = 0; i < generationCount; i++) {
      //   var results = new MutationResults();
      //   foreach (var mutator in mutators) {
      //     mutator.MutateGenotype(genotype, results);
      //   }
      //   Reifier.Reify(genotype);
      // }
    }
  }
}

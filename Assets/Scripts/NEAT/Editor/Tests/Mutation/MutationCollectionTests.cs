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

    [Test]
    public void TestMutationCollection() {
      var mutationCollection = new MutationCollection(
        new NEAT.PerturbNeuronMutator(1.0f, 0.5f),
        new NEAT.PerturbSynapseMutator(1.0f, 0.5f),
        new NEAT.ReplaceNeuronMutator(1.0f),
        new NEAT.ReplaceSynapseMutator(1.0f),
        new NEAT.ToggleSynapseMutator(1.0f)
      );

      var results = mutationCollection.Mutate(genotypes);
      Assert.AreEqual();
    }
  }
}

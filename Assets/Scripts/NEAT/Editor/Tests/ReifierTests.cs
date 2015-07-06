using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class ReifierTests {

    [Test]
    public void TestReifier() {
      var innovations = new Innovations();
      var genotype = NEAT.GetProtoGenotype(innovations);

      var mutators = new IMutator[]{
        new AddNeuronMutator(1.0f),
        new AddSynapseMutator(1.0f),
      };

      var generationCount = 100;
      for (int i = 0; i < generationCount; i++) {
        foreach (var mutator in mutators) {
          genotype = mutator.Mutate(genotype, innovations);
        }
        Reifier.Reify(genotype);
      }
    }
  }
}

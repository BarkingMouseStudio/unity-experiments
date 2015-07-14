using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class MutationCollection {

    private List<KeyValuePair<float, IMutator>> mutators =
      new List<KeyValuePair<float, IMutator>>();

    public void Add(float p, IMutator mutator) {
      mutators.Add(new KeyValuePair<float, IMutator>(p, mutator));

      Assert.IsTrue(mutators.Sum(m => m.Key) <= 1.0f,
        "Sum of mutation probabilities should be less than or equal to 1.0");
    }

    public MutationResults Mutate(Genotype[] genotypes) {
      Assert.AreApproximatelyEqual(1.0f, mutators.Sum(m => m.Key),
        "Sum of mutation probabilities should equal 1.0");

      var orderedMutators = mutators.OrderBy(m => m.Key);

      var results = new MutationResults();
      foreach (var genotype in genotypes) {
        var p = Random.value;
        var prev = 0.0f;
        foreach (var p_mutator in orderedMutators) {
          var curr = prev + p_mutator.Key;
          if (p < curr) {
            p_mutator.Value.Mutate(genotype, results);
            break;
          }
          prev = curr;
        }
      }

      return results;
    }
  }
}

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
    }

    public MutationResults Mutate(Genotype[] genotypes) {
      var orderedMutators = mutators.OrderBy(m => m.Key);
      var results = new MutationResults();
      foreach (var genotype in genotypes) {
        foreach (var p_mutator in orderedMutators) {
          var p = p_mutator.Key;
          var mutator = p_mutator.Value;
          if (Random.value < p) {
            mutator.Mutate(genotype, results);
            break;
          }
        }
      }
      return results;
    }
  }
}

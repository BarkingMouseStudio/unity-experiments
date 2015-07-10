using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceNeuronMutator : IMutator {

    float p;

    public ReplaceNeuronMutator(float p) {
      this.p = p;
    }

    public void Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.neuronGenes.Count; i++) {
        if (Random.value < p2) {
          genotype.neuronGenes[i] = genotype.neuronGenes[i].Randomize();
        }
      }
    }
  }
}

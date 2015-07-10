using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbNeuronMutator : IMutator {

    float p;
    float mutationScale;

    public PerturbNeuronMutator(float p, float mutationScale) {
      this.p = p;
      this.mutationScale = mutationScale;
    }

    public void Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.neuronGenes.Count; i++) {
        if (Random.value < p2) {
          genotype.neuronGenes[i] = genotype.neuronGenes[i].Perturb(mutationScale);
        }
      }
    }
  }
}

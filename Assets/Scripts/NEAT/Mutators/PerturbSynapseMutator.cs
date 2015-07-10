using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbSynapseMutator : IMutator {

    float p;
    float mutationScale;
    float toggleProbability;

    public PerturbSynapseMutator(float p, float mutationScale, float toggleProbability) {
      this.p = p;
      this.mutationScale = mutationScale;
      this.toggleProbability = toggleProbability;
    }

    public void Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.synapseGenes.Count; i++) {
        if (Random.value < p2) {
          genotype.synapseGenes[i] = genotype.synapseGenes[i]
            .Perturb(mutationScale, toggleProbability);
        }
      }
    }
  }
}

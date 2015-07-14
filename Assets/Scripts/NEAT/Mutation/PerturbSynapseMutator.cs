using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbSynapseMutator : IMutator {

    float mutationScale;
    float p;

    public PerturbSynapseMutator(float p, float mutationScale) {
      this.mutationScale = mutationScale;
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.weight = Mathf.Clamp01(RandomHelper.NextCauchy(synapseGene.weight, mutationScale));
          genotype.SynapseGenes[i] = synapseGene;
          results.perturbedSynapses++;
        }
      }
    }
  }
}

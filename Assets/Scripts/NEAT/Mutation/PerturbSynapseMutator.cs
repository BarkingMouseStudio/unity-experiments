using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbSynapseMutator : Mutator {

    float p;
    float mutationScale;

    public PerturbSynapseMutator(float p, float mutationScale) {
      this.p = p;
      this.mutationScale = mutationScale;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p2) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.weight = Mathf.Clamp01(RandomHelper.NextCauchy(synapseGene.weight, mutationScale));
          genotype.SynapseGenes[i] = synapseGene;
          results.perturbedSynapses++;
        }
      }
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceSynapseMutator : IMutator {

    float p;

    public ReplaceSynapseMutator(float p) {
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.weight = Random.value;
          genotype.SynapseGenes[i] = synapseGene;
          results.replacedSynapses++;
        }
      }
    }
  }
}

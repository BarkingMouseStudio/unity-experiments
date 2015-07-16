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
      var innovationIds = genotype.SynapseGenes.InnovationIds;
      for (int i = 0; i < innovationIds.Count; i++) {
        if (Random.value < p) {
          var synapseGene = genotype.SynapseGenes[innovationIds[i]];
          synapseGene.weight = Random.value;
          genotype.SynapseGenes[synapseGene.InnovationId] = synapseGene;
          results.replacedSynapses++;
        }
      }
    }
  }
}

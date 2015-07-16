using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ToggleSynapseMutator : IMutator {

    float p;

    public ToggleSynapseMutator(float p) {
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var innovationIds = genotype.SynapseGenes.InnovationIds;
      for (int i = 0; i < innovationIds.Count; i++) {
        if (Random.value < p) {
          var synapseGene = genotype.SynapseGenes[innovationIds[i]];
          synapseGene.isEnabled = !synapseGene.isEnabled;
          genotype.SynapseGenes[synapseGene.InnovationId] = synapseGene;
          results.toggledSynapses++;
        }
      }
    }
  }
}

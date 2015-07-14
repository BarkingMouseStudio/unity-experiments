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
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.isEnabled = !synapseGene.isEnabled;
          genotype.SynapseGenes[i] = synapseGene;
          results.toggledSynapses++;
        }
      }
    }
  }
}

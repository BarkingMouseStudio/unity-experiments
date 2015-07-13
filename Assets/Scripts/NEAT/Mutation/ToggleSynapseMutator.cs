using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ToggleSynapseMutator : Mutator {

    float p;

    public ToggleSynapseMutator(float p) {
      this.p = p;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p2) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.isEnabled = !synapseGene.isEnabled;
          genotype.SynapseGenes[i] = synapseGene;
          results.toggledSynapses++;
        }
      }
    }
  }
}

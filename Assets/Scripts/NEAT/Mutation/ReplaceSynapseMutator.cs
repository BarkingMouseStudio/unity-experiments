using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceSynapseMutator : Mutator {

    float p;

    public ReplaceSynapseMutator(float p) {
      this.p = p;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.SynapseCount; i++) {
        if (Random.value < p2) {
          var synapseGene = genotype.SynapseGenes[i];
          synapseGene.weight = Random.value;
          genotype.SynapseGenes[i] = synapseGene;
          results.replacedSynapses++;
        }
      }
    }
  }
}

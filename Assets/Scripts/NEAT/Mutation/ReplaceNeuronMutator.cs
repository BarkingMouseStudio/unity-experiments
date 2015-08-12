using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceNeuronMutator : IMutator {

    float p;

    public ReplaceNeuronMutator(float p) {
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var innovationIds = genotype.NeuronGenes.InnovationIds;
      for (int i = 0; i < innovationIds.Count; i++) {
        if (Random.value < p) {
          var innovationId = innovationIds[i];
          genotype.NeuronGenes[innovationId] = NeuronGene.Random(innovationId);
          results.replacedNeurons++;
        }
      }
    }
  }
}

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
          var neuronGene = genotype.NeuronGenes[innovationIds[i]];
          neuronGene.a = Random.value;
          neuronGene.b = Random.value;
          neuronGene.c = Random.value;
          neuronGene.d = Random.value;
          genotype.NeuronGenes[neuronGene.InnovationId] = neuronGene;
          results.replacedNeurons++;
        }
      }
    }
  }
}

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
      for (int i = 0; i < genotype.NeuronCount; i++) {
        if (Random.value < p) {
          var neuronGene = genotype.NeuronGenes[i];
          neuronGene.a = Random.value;
          neuronGene.b = Random.value;
          neuronGene.c = Random.value;
          neuronGene.d = Random.value;
          genotype.NeuronGenes[i] = neuronGene;
          results.replacedNeurons++;
        }
      }
    }
  }
}

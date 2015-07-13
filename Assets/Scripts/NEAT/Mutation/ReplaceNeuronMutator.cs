using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceNeuronMutator : Mutator {

    float p;

    public ReplaceNeuronMutator(float p) {
      this.p = p;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.NeuronCount; i++) {
        if (Random.value < p2) {
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

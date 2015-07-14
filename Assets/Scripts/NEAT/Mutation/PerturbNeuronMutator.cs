using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbNeuronMutator : IMutator {

    float mutationScale;
    float p;

    public PerturbNeuronMutator(float p, float mutationScale) {
      this.mutationScale = mutationScale;
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      for (int i = 0; i < genotype.NeuronCount; i++) {
        if (Random.value < p) {
          var neuronGene = genotype.NeuronGenes[i];
          neuronGene.a = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.a, mutationScale));
          neuronGene.b = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.b, mutationScale));
          neuronGene.c = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.c, mutationScale));
          neuronGene.d = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.d, mutationScale));
          genotype.NeuronGenes[i] = neuronGene;
          results.perturbedNeurons++;
        }
      }
    }
  }
}

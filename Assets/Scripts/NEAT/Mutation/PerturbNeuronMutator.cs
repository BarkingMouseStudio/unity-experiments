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
      var innovationIds = genotype.NeuronGenes.InnovationIds;
      for (int i = 0; i < innovationIds.Count; i++) {
        if (Random.value < p) {
          var neuronGene = genotype.NeuronGenes[innovationIds[i]];
          neuronGene.a = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.a, mutationScale));
          neuronGene.b = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.b, mutationScale));
          neuronGene.c = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.c, mutationScale));
          neuronGene.d = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.d, mutationScale));
          neuronGene.mean = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.mean, mutationScale));
          neuronGene.sigma = Mathf.Clamp01(RandomHelper.NextCauchy(neuronGene.sigma, mutationScale));
          genotype.NeuronGenes[neuronGene.InnovationId] = neuronGene;
          results.perturbedNeurons++;
        }
      }
    }
  }
}

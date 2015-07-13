using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbNeuronMutator : Mutator {

    float p;
    float mutationScale;

    public PerturbNeuronMutator(float p, float mutationScale) {
      this.p = p;
      this.mutationScale = mutationScale;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      var p2 = Mathf.Pow(p, 2.0f);
      for (int i = 0; i < genotype.NeuronCount; i++) {
        if (Random.value < p2) {
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

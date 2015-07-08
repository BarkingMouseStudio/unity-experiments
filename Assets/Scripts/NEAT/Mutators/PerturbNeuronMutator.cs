using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbNeuronMutator : IMutator {

    float p;
    float mutationScale;

    public PerturbNeuronMutator(float p, float mutationScale) {
      this.p = p;
      this.mutationScale = mutationScale;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      var neuronGenes = genotype.neuronGenes.Select((neuronGene) => {
        return Random.value < p2 ? neuronGene.Perturb(mutationScale) : neuronGene;
      }).ToList();
      return new Genotype(neuronGenes, genotype.synapseGenes);
    }
  }
}

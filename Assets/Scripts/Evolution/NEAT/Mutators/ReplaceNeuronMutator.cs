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

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      var neuronGenes = genotype.neuronGenes.Select((neuronGene) => {
        return Random.value < p2 ? neuronGene.Randomize() : neuronGene;
      }).ToList();
      return new Genotype(neuronGenes, genotype.synapseGenes);
    }
  }
}

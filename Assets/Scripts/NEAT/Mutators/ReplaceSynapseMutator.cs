using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class ReplaceSynapseMutator : IMutator {

    float p;
    float toggleProbability;

    public ReplaceSynapseMutator(float p, float toggleProbability) {
      this.p = p;
      this.toggleProbability = toggleProbability;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      var synapseGenes = genotype.synapseGenes.Select((synapseGene) => {
        return Random.value < p2 ? synapseGene.Randomize(toggleProbability) : synapseGene;
      }).ToList();
      return new Genotype(genotype.neuronGenes, synapseGenes);
    }
  }
}

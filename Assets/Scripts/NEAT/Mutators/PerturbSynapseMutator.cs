using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbSynapseMutator : IMutator {

    float p;
    float mutationScale;
    float toggleProbability;

    public PerturbSynapseMutator(float p, float mutationScale, float toggleProbability) {
      this.p = p;
      this.mutationScale = mutationScale;
      this.toggleProbability = toggleProbability;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      var synapseGenes = genotype.synapseGenes.Select((synapseGene) => {
        return Random.value < p2 ? synapseGene.Perturb(mutationScale, toggleProbability) : synapseGene;
      }).ToList();
      return new Genotype(genotype.neuronGenes, synapseGenes);
    }
  }
}

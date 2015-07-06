using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PerturbSynapseMutator : IMutator {

    float p;

    public PerturbSynapseMutator(float p) {
      this.p = p;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      var p2 = Mathf.Pow(p, 2.0f);
      var synapseGenes = genotype.synapseGenes.Select((synapseGene) => {
        return Random.value < p2 ? synapseGene.Perturb() : synapseGene;
      }).ToList();
      return new Genotype(genotype.neuronGenes, synapseGenes);
    }
  }
}

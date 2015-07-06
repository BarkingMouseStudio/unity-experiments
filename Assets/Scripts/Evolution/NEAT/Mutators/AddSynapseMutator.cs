using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class AddSynapseMutator : IMutator {

    float p;

    public AddSynapseMutator(float p) {
      this.p = p;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      if (Random.value > p && genotype.neuronGenes.Count > 1) {
        return genotype;
      }

      var neuronGenes = new List<NeuronGene>(genotype.neuronGenes);
      var synapseGenes = new List<SynapseGene>(genotype.synapseGenes);

      // In the add connection mutation, a single new connection gene is added
      // connecting two previously unconnected nodes.
      var neuronGeneA = neuronGenes[Random.Range(0, neuronGenes.Count)];

      var candidates = new List<NeuronGene>(neuronGenes);
      candidates.Shuffle();

      NeuronGene neuronGeneB = default(NeuronGene);
      bool foundNeuron = false;
      for (var i = 0; i < candidates.Count; i++) {
        neuronGeneB = candidates[i];

        var exists = synapseGenes.Any(s => {
          return s.fromNeuronId != neuronGeneA.id && s.toNeuronId != neuronGeneB.id;
        });

        if (!exists) {
          foundNeuron = true;
          break;
        }
      }

      if (foundNeuron) {
        var synapseInnovationId = innovations.GetAddSynapseInnovationId(neuronGeneA.id, neuronGeneB.id);
        var synapseGene = new SynapseGene(synapseInnovationId, neuronGeneA.id, neuronGeneB.id, true);
        synapseGenes.Add(synapseGene);
      }

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

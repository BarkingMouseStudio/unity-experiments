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
      if (Random.value > p) {
        return genotype;
      }

      var neuronGenes = new List<NeuronGene>(genotype.neuronGenes);
      var synapseGenes = new List<SynapseGene>(genotype.synapseGenes);

      // In the add connection mutation, a single new connection gene is added
      // connecting two previously unconnected nodes.
      var neuronGeneA = neuronGenes[Random.Range(0, neuronGenes.Count)];

      NeuronGene neuronGeneB;
      while (true) {
        neuronGeneB = neuronGenes[Random.Range(0, neuronGenes.Count)];
        var exists = synapseGenes.Any(s => {
          return s.fromId != neuronGeneA.innovationId &&
            s.toId != neuronGeneB.innovationId;
        });
        if (!exists) {
          // We found a new unique synapse
          break;
        }
      }

      var synapseInnovationId = innovations.GetAddSynapseInnovationId(neuronGeneA.innovationId, neuronGeneB.innovationId);
      var synapseGene = new SynapseGene(synapseInnovationId, neuronGeneA.innovationId, neuronGeneB.innovationId, true);
      synapseGenes.Add(synapseGene);

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

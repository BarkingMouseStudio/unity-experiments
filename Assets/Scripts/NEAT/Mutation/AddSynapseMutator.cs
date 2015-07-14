using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // In the add connection mutation, a single new connection gene is added
  // connecting two previously unconnected nodes.
  public class AddSynapseMutator : IMutator {

    InnovationCollection innovations;

    public AddSynapseMutator(InnovationCollection innovations) {
      this.innovations = innovations;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var neuronGeneA = genotype.NeuronGenes[Random.Range(0, genotype.NeuronCount)];

      var candidates = new List<NeuronGene>(genotype.NeuronGenes);
      candidates.Shuffle();

      NeuronGene neuronGeneB = default(NeuronGene);
      bool foundNeuron = false;
      for (var i = 0; i < candidates.Count; i++) {
        neuronGeneB = candidates[i];

        var exists = genotype.SynapseGenes.Any(s =>
          s.fromNeuronId != neuronGeneA.InnovationId &&
          s.toNeuronId   != neuronGeneB.InnovationId);

        if (!exists) {
          foundNeuron = true;
          break;
        }
      }

      if (foundNeuron) {
        var synapseInnovationId = innovations.GetSynapseInnovationId(neuronGeneA.InnovationId, neuronGeneB.InnovationId);
        var synapseGene = SynapseGene.Random(synapseInnovationId, neuronGeneA.InnovationId, neuronGeneB.InnovationId, true);

        var synapseGenes = new List<SynapseGene>(genotype.SynapseGenes);
        synapseGenes.Add(synapseGene);
        genotype.SynapseGenes = synapseGenes.ToArray();

        results.addedSynapses += 1;
      }
    }
  }
}

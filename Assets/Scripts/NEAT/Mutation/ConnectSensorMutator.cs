using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // In the connect sensor mutation, a single new connection gene is added
  // connecting two previously input and output nodes.
  public class ConnectSensorMutator : IMutator {

    InnovationCollection innovations;

    public ConnectSensorMutator(InnovationCollection innovations) {
      this.innovations = innovations;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var neuronIndexA = Random.Range(0, NetworkIO.inNeuronCount);
      var neuronGeneA = genotype.NeuronGenes.ElementAt(neuronIndexA);

      var candidates = genotype.NeuronGenes.Skip(NetworkIO.inNeuronCount)
        .Take(NetworkIO.outNeuronCount).ToList();
      candidates.Shuffle();

      NeuronGene neuronGeneB = default(NeuronGene);
      bool foundNeuron = false;
      for (var i = 0; i < candidates.Count; i++) {
        neuronGeneB = candidates[i];

        var exists = genotype.SynapseGenes.Any(s =>
          neuronGeneA.InnovationId == s.fromNeuronId &&
          neuronGeneB.InnovationId == s.toNeuronId);

        if (!exists) {
          foundNeuron = true;
          break;
        }
      }

      if (foundNeuron) {
        var synapseInnovationId = innovations.GetSynapseInnovationId(neuronGeneA.InnovationId, neuronGeneB.InnovationId);
        var synapseGene = SynapseGene.Random(synapseInnovationId, neuronGeneA.InnovationId, neuronGeneB.InnovationId, true);

        var synapseGenes = new GeneList<SynapseGene>(genotype.SynapseGenes);
        synapseGenes.Add(synapseGene);
        genotype.SynapseGenes = synapseGenes;

        results.addedSynapses += 1;
      }
    }
  }
}

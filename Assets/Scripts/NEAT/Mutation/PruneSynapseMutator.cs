using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class PruneSynapseMutator : IMutator {

    float p;

    public PruneSynapseMutator(float p) {
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var prunedSynapses = genotype.SynapseGenes
        .Where(g => !g.isEnabled)
        .Where(_ => Random.value < p);

      var synapseGenes = genotype.SynapseGenes.Except(prunedSynapses);
      genotype.SynapseGenes = new GeneList<SynapseGene>(synapseGenes);

      // An orphan is a neuron with no connecting synapses
      // For each neuron: check if any synapse connects to it
      // Exclude IO neurons
      var orphanedNeurons = genotype.NeuronGenes.Skip(NetworkIO.InitialNeuronCount)
        .Where(n => {
          var hasConnections = synapseGenes.Any(s =>
            s.fromNeuronId == n.InnovationId ||
            s.toNeuronId == n.InnovationId);
          return !hasConnections;
        });
      var neuronGenes = genotype.NeuronGenes.Except(orphanedNeurons);
      genotype.NeuronGenes = new GeneList<NeuronGene>(neuronGenes);

      results.orphanedNeurons += orphanedNeurons.Count();
      results.prunedSynapses += prunedSynapses.Count();
    }
  }
}

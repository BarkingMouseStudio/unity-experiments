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
        .Where(_ => Random.value < p)
        .ToList();

      var synapseGenes = genotype.SynapseGenes.Except(prunedSynapses)
        .ToGeneList();
      genotype.SynapseGenes = synapseGenes;

      // An orphan is a neuron with no connecting synapses
      // For each neuron: check if any synapse connects to it
      // Exclude IO neurons
      var orphanedNeurons = genotype.NeuronGenes.Skip(NetworkPorts.initialNeuronCount)
        .Where(g =>
          synapseGenes.None(s =>
            s.fromNeuronId == g.InnovationId ||
            s.toNeuronId == g.InnovationId)).ToList();

      var neuronGenes = genotype.NeuronGenes.Except(orphanedNeurons)
        .ToGeneList();
      genotype.NeuronGenes = neuronGenes;

      results.orphanedNeurons += orphanedNeurons.Count;
      results.prunedSynapses += prunedSynapses.Count;
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // In the add node mutation, an existing connection is split and the new
  // node placed where the old connection used to be. The old connection is
  // disabled and two new connections are added to the genome.
  public class AddNeuronMutator : IMutator {

    InnovationCollection innovations;

    public AddNeuronMutator(InnovationCollection innovations) {
      this.innovations = innovations;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      // We require a synapse to split
      if (genotype.SynapseCount == 0) {
        return;
      }

      var synapseIndex = Random.Range(0, genotype.SynapseCount);
      var synapseGene = genotype.SynapseGenes[synapseIndex];
      synapseGene.isEnabled = false;

      var innovationId = innovations.GetNeuronInnovationId(
        synapseGene.fromNeuronId,
        synapseGene.toNeuronId,
        synapseGene.InnovationId
      );

      var neuronGene = NeuronGene.Random(innovationId);

      var neuronGenes = new List<NeuronGene>(genotype.NeuronGenes);
      neuronGenes.Add(neuronGene);
      genotype.NeuronGenes = neuronGenes.ToArray();

      var synapseGenes = new List<SynapseGene>(genotype.SynapseGenes);
      synapseGenes[synapseIndex] = synapseGene;

      var synapseGene1 = new SynapseGene(innovationId + 0,
        synapseGene.fromNeuronId,
        neuronGene.InnovationId,
        true, 1.0f);
      synapseGenes.Add(synapseGene1);

      var synapseGene2 = new SynapseGene(innovationId + 1,
        neuronGene.InnovationId,
        synapseGene.toNeuronId,
        true, synapseGene.weight);
      synapseGenes.Add(synapseGene2);

      genotype.SynapseGenes = synapseGenes.ToArray();

      results.addedNeurons += 1;
      results.addedSynapses += 2;
    }
  }
}

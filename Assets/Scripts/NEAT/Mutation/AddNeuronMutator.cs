using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // In the add node mutation, an existing connection is split and the new
  // node placed where the old connection used to be. The old connection is
  // disabled and two new connections are added to the genome.
  public class AddNeuronMutator : Mutator {

    InnovationCollection innovations;
    float p;

    public AddNeuronMutator(float p, InnovationCollection innovations) {
      this.innovations = innovations;
      this.p = p;
    }

    public override void MutateGenotype(Genotype genotype, MutationResults results) {
      // We require a synapse to split
      if (genotype.SynapseCount == 0) {
        return;
      }
      
      if (Random.value > p) {
        return;
      }

      var synapseIndex = Random.Range(0, genotype.SynapseCount);
      var synapseGene = genotype.SynapseGenes[synapseIndex];

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

      var synapseGene1 = new SynapseGene(innovationId + 0,
        synapseGene.fromNeuronId,
        neuronGene.InnovationId,
        true, 0.5f);

      synapseGenes.Add(synapseGene1);
      genotype.SynapseGenes = synapseGenes.ToArray();

      var synapseGene2 = new SynapseGene(innovationId + 1,
        neuronGene.InnovationId,
        synapseGene.toNeuronId,
        true, 0.5f);

      synapseGenes.Add(synapseGene2);
      genotype.SynapseGenes = synapseGenes.ToArray();

      results.addedNeurons += 1;
      results.addedSynapses += 2;
    }
  }
}

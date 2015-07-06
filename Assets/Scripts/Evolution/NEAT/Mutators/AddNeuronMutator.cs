using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class AddNeuronMutator : IMutator {

    float p;

    public AddNeuronMutator(float p) {
      this.p = p;
    }

    public Genotype Mutate(Genotype genotype, Innovations innovations) {
      if (Random.value > p || genotype.synapseGenes.Count == 0) {
        return genotype;
      }

      var neuronGenes = new List<NeuronGene>(genotype.neuronGenes);
      var synapseGenes = new List<SynapseGene>(genotype.synapseGenes);

      // In the add node mutation, an existing connection is split and the new node
      // placed where the old connection used to be. The old connection is disabled
      // and two new connections are added to the genome.
      var synapseIndex = Random.Range(0, synapseGenes.Count);
      var synapseGene = synapseGenes[synapseIndex];

      var innovationId = innovations.GetAddNeuronInnovationId(synapseGene.fromNeuronId, synapseGene.toNeuronId, synapseGene.InnovationId);

      var neuronIndex = neuronGenes.Count;
      var neuronGene = new NeuronGene(innovationId, neuronIndex);
      neuronGenes.Add(neuronGene);

      var synapseGene1 = new SynapseGene(innovationId + 0, synapseGene.fromNeuronId, neuronGene.id, true);
      synapseGenes.Add(synapseGene1);

      var synapseGene2 = new SynapseGene(innovationId + 1, neuronGene.id, synapseGene.toNeuronId, true);
      synapseGenes.Add(synapseGene2);

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

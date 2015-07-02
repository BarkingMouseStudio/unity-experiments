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
      if (Random.value > p) {
        return genotype;
      }

      var neuronGenes = new List<NeuronGene>(genotype.neuronGenes);
      var synapseGenes = new List<SynapseGene>(genotype.synapseGenes);

      // In the add node mutation, an existing connection is split and the new node
      // placed where the old connection used to be. The old connection is disabled
      // and two new connections are added to the genome.
      var synapseIndex = Random.Range(0, synapseGenes.Count);
      var synapseGene = synapseGenes[synapseIndex];

      var innovationId = innovations.GetAddNeuronInnovationId(synapseGene.fromId, synapseGene.toId, synapseGene.innovationId);

      var neuronGene = new NeuronGene(innovationId);
      neuronGenes.Add(neuronGene);

      var synapseGene1 = new SynapseGene(innovationId + 0, synapseGene.fromId, neuronGene.innovationId, true);
      synapseGenes.Add(synapseGene1);

      var synapseGene2 = new SynapseGene(innovationId + 1, neuronGene.innovationId, synapseGene.toId, true);
      synapseGenes.Add(synapseGene2);

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

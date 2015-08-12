using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // In the connect sensor mutation, a single new connection gene is added
  // connecting two previously input and output nodes.
  public class ConnectSensorMutator : IMutator {

    InnovationCollection innovations;
    float p;

    public ConnectSensorMutator(InnovationCollection innovations, float p) {
      this.innovations = innovations;
      this.p = p;
    }

    public void Mutate(Genotype genotype, MutationResults results) {
      var neuronIndexA = Random.Range(0, genotype.InputNeurons.Count);
      var neuronGeneA = genotype.InputNeurons[neuronIndexA];

      var validNeurons = genotype.OutputNeurons
        .Where(_ => Random.value < p)
        .Where(neuronGeneB => genotype.SynapseGenes.None(s =>
          neuronGeneA.InnovationId == s.fromNeuronId &&
          neuronGeneB.InnovationId == s.toNeuronId));

      foreach (var neuronGeneB in validNeurons) {
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

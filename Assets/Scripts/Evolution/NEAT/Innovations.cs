using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // TODO: Consider unifying lists and using enum type (makes it easier to get/add new types).
  public class Innovations {

    private List<NeuronInnovation> neuronInnovations = new List<NeuronInnovation>();
    private List<SynapseInnovation> synapseInnovations = new List<SynapseInnovation>();

    private int nextInnovationId = 0;

    public Innovations(int nextInnovationId) {
      this.nextInnovationId = nextInnovationId;
    }

    // Remove any innovations not found in the genotype set
    public int Prune(List<Genotype> genotypes) {
      var count = neuronInnovations.Count + synapseInnovations.Count;

      neuronInnovations = neuronInnovations.Where(innov => {
        return genotypes.Any(gt => {
          return gt.neuronGenes.Any(g => g.innovationId == innov.innovationId);
        });
      }).ToList();

      synapseInnovations = synapseInnovations.Where(innov => {
        return genotypes.Any(gt => {
          return gt.synapseGenes.Any(g => g.innovationId == innov.innovationId);
        });
      }).ToList();

      var prunedCount = neuronInnovations.Count + synapseInnovations.Count;
      return System.Math.Abs(count - prunedCount);
    }

    public int GetAddNeuronInnovationId(int fromId, int toId, int oldSId) {
      foreach (var innov in neuronInnovations) {
        // Does the added neuron share the same mutation properties?
        if (innov.fromNeuronId == fromId &&
            innov.toNeuronId == toId &&
            innov.oldSynapseInnovationId == oldSId) {
          return innov.innovationId;
        }
      }

      // Increment by two for each synapse—neuron receives first id since it's
      // unique within the network for neurons.
      var innovNext = new NeuronInnovation(nextInnovationId, fromId, toId, oldSId);
      nextInnovationId += 2;

      neuronInnovations.Add(innovNext);
      return innovNext.innovationId;
    }

    public int GetAddSynapseInnovationId(int fromId, int toId) {
      foreach (var innov in synapseInnovations) {
        if (innov.fromNeuronId == fromId && innov.toNeuronId == toId) {
          return innov.innovationId;
        }
      }

      var innovNext = new SynapseInnovation(nextInnovationId, fromId, toId);
      nextInnovationId++;

      synapseInnovations.Add(innovNext);
      return innovNext.innovationId;
    }
  }
}

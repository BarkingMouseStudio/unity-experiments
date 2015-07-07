using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Innovations {

    private List<NeuronInnovation> neuronInnovations = new List<NeuronInnovation>();
    private List<SynapseInnovation> synapseInnovations = new List<SynapseInnovation>();

    private int nextInnovationId = 0;

    public int Count {
      get {
        return neuronInnovations.Count + synapseInnovations.Count;
      }
    }

    public int GetAddInitialNeuronInnovationId(int i) {
      foreach (var innov in neuronInnovations) {
        if (innov.innovationId == i) {
          return innov.innovationId;
        }
      }

      // Catch up (in case we fell behind by index)
      if (nextInnovationId < i) {
        nextInnovationId += i - nextInnovationId;
      }

      var innovNext = new NeuronInnovation(nextInnovationId, -1, 1, -1);
      nextInnovationId++;

      neuronInnovations.Add(innovNext);
      return innovNext.innovationId;
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

    public int GetAddInitialSynapseInnovationId(int fromNeuronId, int toNeuronId) {
      foreach (var innov in synapseInnovations) {
        if (innov.fromNeuronId == fromNeuronId && innov.toNeuronId == toNeuronId) {
          return innov.innovationId;
        }
      }

      var innovNext = new SynapseInnovation(nextInnovationId, fromNeuronId, toNeuronId);
      nextInnovationId++;

      synapseInnovations.Add(innovNext);
      return innovNext.innovationId;
    }

    public int GetAddSynapseInnovationId(int fromNeuronId, int toNeuronId) {
      foreach (var innov in synapseInnovations) {
        if (innov.fromNeuronId == fromNeuronId && innov.toNeuronId == toNeuronId) {
          return innov.innovationId;
        }
      }

      var innovNext = new SynapseInnovation(nextInnovationId, fromNeuronId, toNeuronId);
      nextInnovationId++;

      synapseInnovations.Add(innovNext);
      return innovNext.innovationId;
    }

    // Remove any innovations not found in the genotype set
    public int Prune(List<Genotype> genotypes) {
      var count = neuronInnovations.Count + synapseInnovations.Count;

      neuronInnovations = neuronInnovations.Where(innov => {
        return genotypes.Any(gt => {
          return gt.neuronGenes.Any(g => g.InnovationId == innov.innovationId);
        });
      }).ToList();

      synapseInnovations = synapseInnovations.Where(innov => {
        return genotypes.Any(gt => {
          return gt.synapseGenes.Any(g => g.InnovationId == innov.innovationId);
        });
      }).ToList();

      var prunedCount = neuronInnovations.Count + synapseInnovations.Count;
      return System.Math.Abs(count - prunedCount);
    }
  }
}

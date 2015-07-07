using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Innovations {

    private List<Innovation> innovations = new List<Innovation>();
    private int nextInnovationId = 0;

    public int Count {
      get {
        return innovations.Count;
      }
    }

    public int GetInitialNeuronInnovationId(int i) {
      foreach (var innov in neuronInnovations) {
        if (innov.type == InnovationType.Neuron &&
            innov.innovationId == i) {
          return innov.innovationId;
        }
      }

      // Catch up (in case we fell behind by index)
      if (nextInnovationId < i) {
        nextInnovationId += i - nextInnovationId;
      }

      var innovNext = new Innovation(InnovationType.Neuron, nextInnovationId, -1, 1, -1);
      nextInnovationId++;

      innovations.Add(innovNext);
      return innovNext.innovationId;
    }

    public int GetNeuronInnovationId(int fromNeuronId, int toNeuronId, int oldSynapseInnovationId) {
      foreach (var innov in innovations) {
        if (innov.type == InnovationType.Neuron &&
            innov.fromNeuronId == fromId &&
            innov.toNeuronId == toId &&
            innov.oldSynapseInnovationId == oldSynapseInnovationId) {
          return innov.innovationId;
        }
      }

      var innovation = new Innovation(InnovationType.Neuron, nextInnovationId, fromNeuronId, toNeuronId, oldSynapseInnovationId);
      innovations.Add(innovation);

      nextInnovationId++;
      return innovation.innovationId;
    }

    public int GetSynapseInnovationId(int fromNeuronId, int toNeuronId) {
      foreach (var innov in innovations) {
        if (innov.type == InnovationType.Synapse &&
            innov.fromNeuronId == fromNeuronId &&
            innov.toNeuronId == toNeuronId) {
          return innov.innovationId;
        }
      }

      var innovation = new Innovation(InnovationType.Synapse, nextInnovationId, fromNeuronId, toNeuronId, -1);
      innovations.Add(innovation);

      nextInnovationId++;
      return innovation.innovationId;
    }

    // Remove any innovations not found in the genotype set
    public int Prune(List<Genotype> genotypes) {
      var initialCount = innovations.Count;

      innovations = innovations.Where(innov =>
        genotypes.ContainsInnovation(innov)).ToList();

      var prunedCount = innovations.Count;
      return System.Math.Abs(count - prunedCount);
    }
  }
}

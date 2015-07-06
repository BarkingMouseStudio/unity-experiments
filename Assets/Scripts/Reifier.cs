using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neural;

public static class Reifier {

  // Responsible for relaying the genotype structure to the neural network.
  public static Neural.Network Reify(NEAT.Genotype genotype) {
    var network = new Neural.Network(NetworkIO.MAX_DELAY);

    foreach (var neuronGene in genotype.neuronGenes) {
      float a = NumberHelper.Scale(neuronGene.a, 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(neuronGene.b, 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(neuronGene.c, -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(neuronGene.d, 0.05f, 8.0f); // 2.0
	    network.AddNeuron(a, b, c, d);
    }

    // Connect each input neuron to the output neuron.
    foreach (var synapseGene in genotype.synapseGenes) {
      if (!synapseGene.isEnabled) {
        continue;
      }

      var fromNeuronId = genotype.neuronGenes.FindIndex(n => n.InnovationId == synapseGene.fromNeuronId);
      var toNeuronId = genotype.neuronGenes.FindIndex(n => n.InnovationId == synapseGene.toNeuronId);

      float weight = NumberHelper.Scale(synapseGene.weight, -40.0f, 40.0f);

      if (fromNeuronId == -1) {
        Debug.LogFormat("AddSynapse From: {0}, To: {1} (Weight: {2}, Synapse Count: {3}, Neuron Count: {4})",
          fromNeuronId, toNeuronId, weight, genotype.synapseGenes.Count, genotype.neuronGenes.Count);
        Debug.Log(genotype.ToJSON());
      }

      if (toNeuronId == -1) {
        Debug.LogFormat("AddSynapse From: {0}, To: {1} (Weight: {2}, Synapse Count: {3}, Neuron Count: {4})",
          fromNeuronId, toNeuronId, weight, genotype.synapseGenes.Count, genotype.neuronGenes.Count);
        Debug.Log(genotype.ToJSON());
      }

      try {
        network.AddSynapse((ulong)fromNeuronId, (ulong)toNeuronId, weight, -40.0f, 40.0f);
      } catch (Exception e) {
        Debug.LogFormat("AddSynapse From: {0}, To: {1} (Weight: {2}, Synapse Count: {3}, Neuron Count: {4})",
          fromNeuronId, toNeuronId, weight, genotype.synapseGenes.Count, genotype.neuronGenes.Count);
        Debug.Log(genotype.ToJSON());
        Debug.LogException(e);
      }
    }

    return network;
  }
}

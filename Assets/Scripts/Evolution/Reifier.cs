using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neural;

public static class Reifier {

  public static readonly ulong[] inNeuronIds;
  public static readonly ulong[] outNeuronIds;

  public static readonly int inNeuronCount;
  public static readonly int outNeuronCount;

  private static readonly ulong MAX_DELAY = 20;

  static Reifier() {
    inNeuronCount = (NetworkIO.angularRanges.Length * 2) +
      (NetworkIO.linearRanges.Length * 2);
    outNeuronCount = NetworkIO.speeds.Length;

    // Set up input neuron ids by order
    inNeuronIds = Enumerable.Range(0, inNeuronCount)
      .Select(i => (ulong)i)
      .ToArray();

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = Enumerable.Range(0, outNeuronCount)
      .Select(i => (ulong)(inNeuronCount + i))
      .ToArray();
  }

  // Responsible for relaying the genotype structure to the neural network.
  public static Neural.Network Reify(NEAT.Genotype genotype) {
    var network = new Neural.Network(MAX_DELAY);

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

      var fromNeuronId = (ulong)synapseGene.fromNeuronId;
      var toNeuronId = (ulong)synapseGene.toNeuronId;

      float weight = NumberHelper.Scale(synapseGene.weight, -40.0f, 40.0f);
      network.AddSynapse(fromNeuronId, toNeuronId, weight, -40.0f, 40.0f);
    }

    return network;
  }
}

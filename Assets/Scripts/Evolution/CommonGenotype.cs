using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Neural;

// Responsible for defining the genotype representation and creating the
// phenotypical network.
public class CommonGenotype : IEnumerable<float[]> {

  public static readonly ulong[] inNeuronIds;
  public static readonly ulong[] outNeuronIds;

  public static readonly int neuronCount;
  public static readonly int synapseCount;

  private static readonly ulong MAX_DELAY = 20;
  private readonly float[][] genotype;

  static CommonGenotype() {
    int inNeuronCount = (NetworkIO.angularRanges.Length * 2) +
      (NetworkIO.linearRanges.Length * 2);
    int outNeuronCount = NetworkIO.speeds.Length;

    neuronCount = inNeuronCount + outNeuronCount;
    synapseCount = inNeuronCount * outNeuronCount;

    // Set up input neuron ids by order
    inNeuronIds = new ulong[inNeuronCount];
    for (int i = 0; i < inNeuronCount; i++) {
      inNeuronIds[i] = (ulong)i;
    }
    AssertHelper.Assert(inNeuronIds.Length == inNeuronCount,
      "Unexpected input neuron id count");

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = new ulong[outNeuronCount];
    for (int i = 0; i < outNeuronCount; i++) {
      outNeuronIds[i] = (ulong)(inNeuronCount + i);
    }
    AssertHelper.Assert(outNeuronIds.Length == outNeuronCount,
      "Unexpected output neuron id count");
  }

  public CommonGenotype() {
    var genotype = new List<float[]>(neuronCount + synapseCount);

    // subpopulation(s) for input neurons (a, b, c, d)
    // subpopulation(s) for output neurons (a, b, c, d)
    foreach (var _ in Enumerable.Range(0, neuronCount)) {
      genotype.Add(
        new float[]{Random.value, Random.value, Random.value, Random.value}
      );
    }

    // subpopulation(s) for synapses (w)
    foreach (var _ in Enumerable.Range(0, synapseCount)) {
      genotype.Add(new float[]{Random.value});
    }

    this.genotype = genotype.ToArray();
  }

  public CommonGenotype(float[][] genotype) {
    this.genotype = genotype;
  }

  public IEnumerator<float[]> GetEnumerator() {
    foreach (var chromosome in genotype) {
      yield return chromosome;
    }
  }

  System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
    return this.GetEnumerator();
  }

  // Responsible for relaying the genotype structure to the neural network.
  public Neural.Network GetPhenotype() {
    var network = new Neural.Network(MAX_DELAY);

    int inNeuronCount = inNeuronIds.Length;
    int outNeuronCount = outNeuronIds.Length;
    int genotypeOffset = 0;

    for (int i = 0; i < inNeuronCount; i++) {
      float[] chromosome = genotype[i];
      float a = chromosome[0]; // 0.1
      float b = chromosome[1]; // 0.2
      float c = chromosome[2]; // -65.0
      float d = chromosome[3]; // 2.0
	    network.AddNeuron(a, b, c, d);
    }
    genotypeOffset += inNeuronCount;

    for (int i = 0; i < outNeuronCount; i++) {
      float[] chromosome = genotype[genotypeOffset + i];
      float a = NumberHelper.Scale(chromosome[0], 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(chromosome[1], 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(chromosome[2], -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(chromosome[3], 0.05f, 8.0f); // 2.0
	    network.AddNeuron(a, b, c, d);
    }
    genotypeOffset += outNeuronCount;

    float min = -20.0f;
    float max = 20.0f;

    // Connect each input neuron to the output neuron.
    for (int i = 0; i < inNeuronCount; i++) {
      for (int j = 0; j < outNeuronCount; j++) {
        float[] chromosome = genotype[genotypeOffset + (i * outNeuronCount) + j];
        float w = NumberHelper.Scale(chromosome[0], min, max);
        network.AddSynapse(inNeuronIds[i], outNeuronIds[j], w, min, max);
      }
    }

    AssertHelper.Assert(CommonGenotype.neuronCount == (int)network.NeuronCount,
      "Incorrect neuron count");
    AssertHelper.Assert(CommonGenotype.synapseCount == (int)network.SynapseCount,
      "Incorrect synapse count");

    return network;
  }
}

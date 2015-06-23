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
    inNeuronIds = Enumerable.Range(0, inNeuronCount).Select((i) => {
      return (ulong)i;
    }).ToArray();
    // Debug.Log(string.Join(",", inNeuronIds.Select(v => v.ToString()).ToArray()));

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = Enumerable.Range(0, outNeuronCount).Select((i) => {
      return (ulong)(inNeuronCount + i);
    }).ToArray();
    // Debug.Log(string.Join(",", outNeuronIds.Select(v => v.ToString()).ToArray()));
  }

  public static CommonGenotype FromJSON(string data) {
    var obj = (List<object>)JSON.Deserialize(data);
    var genotypes = obj.Select(o => {
      var l = (List<object>)o;
      return l.Select(v => {
        return (float)(double)v;
      }).ToArray();
    }).ToArray();
    return new CommonGenotype(genotypes);
  }

  public CommonGenotype() {
    var genotype = new List<float[]>(neuronCount + synapseCount);

    var neuronChromosomes = Enumerable.Range(0, neuronCount).Select((i) => {
      return new float[]{
        Random.value,
        Random.value,
        Random.value,
        Random.value
      };
    });

    // subpopulation(s) for synapses (w)
    var synapseChromosomes = Enumerable.Range(0, synapseCount).Select((i) => {
      return new float[]{
        Random.value
      };
    });

    genotype.AddRange(neuronChromosomes);
    genotype.AddRange(synapseChromosomes);

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
      // Debug.LogFormat("a: {0} => {1}, b: {2} => {3}, c: {4} => {5}, d: {6} => {7}",
      //   a, chromosome[0],
      //   b, chromosome[1],
      //   c, chromosome[2],
      //   d, chromosome[3]);
	    network.AddNeuron(a, b, c, d);
    }
    genotypeOffset += outNeuronCount;

    float min = -40.0f;
    float max = 40.0f;

    // Connect each input neuron to the output neuron.
    for (int i = 0; i < inNeuronCount; i++) {
      for (int j = 0; j < outNeuronCount; j++) {
        float[] chromosome = genotype[genotypeOffset + (i * outNeuronCount) + j];
        float w = NumberHelper.Scale(chromosome[0], min, max);
        // Debug.LogFormat("w: {0} => {1}", w, chromosome[0]);
        network.AddSynapse(inNeuronIds[i], outNeuronIds[j], w, min, max);
      }
    }

    AssertHelper.Assert(CommonGenotype.neuronCount == (int)network.NeuronCount,
      "Incorrect neuron count");
    AssertHelper.Assert(CommonGenotype.synapseCount == (int)network.SynapseCount,
      "Incorrect synapse count");

    return network;
  }

  public override string ToString() {
    return JSON.Serialize(this.genotype);
  }
}

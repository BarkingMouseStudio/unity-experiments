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

  public static readonly int inNeuronCount;
  public static readonly int outNeuronCount;
  public static readonly int hiddenNeuronCount;

  private static readonly ulong MAX_DELAY = 20;
  private readonly float[][] genotype;

  static CommonGenotype() {
    inNeuronCount = (NetworkIO.angularRanges.Length * 2) +
      (NetworkIO.linearRanges.Length * 2);
    outNeuronCount = NetworkIO.speeds.Length;

    hiddenNeuronCount = 6;

    neuronCount = inNeuronCount + outNeuronCount + hiddenNeuronCount;
    synapseCount = neuronCount * neuronCount;

    // Set up input neuron ids by order
    inNeuronIds = Enumerable.Range(0, inNeuronCount)
      .Select(i => (ulong)i)
      .ToArray();
    // Debug.Log(string.Join(",", inNeuronIds.Select(v => v.ToString()).ToArray()));

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = Enumerable.Range(0, outNeuronCount)
      .Select(i => (ulong)(inNeuronCount + i))
      .ToArray();
    // Debug.Log(string.Join(",", outNeuronIds.Select(v => v.ToString()).ToArray()));
  }

  public static CommonGenotype FromJSON(string data) {
    var obj = (List<object>)JSON.Deserialize(data);
    var genotypes = obj.Select(o => {
      var list = (List<object>)o;
      return list.Select(v => (float)(double)v).ToArray();
    }).ToArray();
    return new CommonGenotype(genotypes);
  }

  public CommonGenotype() {
    var genotype = new List<float[]>(neuronCount + synapseCount);

    // subpopulation(s) for synapses (w)
    var chromosomes = Enumerable.Range(0, neuronCount).Select((i) => {
      var neuronGenes = Enumerable.Range(0, 4)
        .Select(_ => Random.value);

      var synapseGenes = Enumerable.Range(0, neuronCount)
        .Select((_) => Random.value);

      var genes = neuronGenes.ToList();
      genes.AddRange(synapseGenes);
      return genes.ToArray();
    });

    genotype.AddRange(chromosomes);

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

    for (int i = 0; i < neuronCount; i++) {
      float[] chromosome = genotype[i];
      float a = NumberHelper.Scale(chromosome[0], 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(chromosome[1], 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(chromosome[2], -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(chromosome[3], 0.05f, 8.0f); // 2.0
	    network.AddNeuron(a, b, c, d);
    }

    float min = -40.0f;
    float max = 40.0f;

    // Connect each input neuron to the output neuron.
    for (int i = 0; i < neuronCount; i++) {
      float[] chromosome = genotype[i];
      // Debug.LogFormat("i: {0}, len:", i, );
      // Debug.LogFormat("{0} {1}", chromosome.Length, string.Join(",", chromosome.Select(x => x.ToString()).ToArray()));
      for (int j = 0; j < neuronCount; j++) {
        // Debug.LogFormat("j: {0}, len: {1}", j, chromosome[j].Length);
        float w = NumberHelper.Scale(chromosome[4 + j], min, max);
        network.AddSynapse((ulong)i, (ulong)j, w, min, max);
      }
    }

    AssertHelper.Assert(neuronCount == (int)network.NeuronCount,
      "Incorrect neuron count");
    AssertHelper.Assert(synapseCount == (int)network.SynapseCount,
      "Incorrect synapse count");

    return network;
  }

  public override string ToString() {
    return JSON.Serialize(this.genotype);
  }
}

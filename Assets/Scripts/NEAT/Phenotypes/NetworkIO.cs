using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Responsible for marshalling input/output data to/from the neural network.
public class NetworkIO {

  static readonly Range[] angularRanges = Range.From(new double[]{
    -180.0, -90.0, -45.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
  });

  static readonly Range[] linearRanges = Range.From(new double[]{
    -6.0, -3.0, -1.0, 0.0, 1.0, 3.0, 6.0,
  });

  static readonly double[] speeds = new double[]{
    -250.0, -200.0, -150.0, -100.0, -50.0, -25.0, -10.0, -5.0, -1.0, -0.1,
    0.1, 1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 150.0, 200.0, 250.0
  };

  static readonly Range[] inputRanges;

  static readonly ulong[] inNeuronIds;
  static readonly ulong[] outNeuronIds;

  public static readonly int inNeuronCount;
  public static readonly int outNeuronCount;

  public static int InitialNeuronCount {
    get {
      return inNeuronCount + outNeuronCount;
    }
  }

  static NetworkIO() {
    inNeuronCount = (NetworkIO.angularRanges.Length * 2) +
      (NetworkIO.linearRanges.Length * 1);
    outNeuronCount = NetworkIO.speeds.Length;

    // Set up input neuron ids by order
    inNeuronIds = Enumerable.Range(0, inNeuronCount)
      .Select(i => (ulong)i)
      .ToArray();

    // Set up output neuron ids by order _after_ input neuron ids
    outNeuronIds = Enumerable.Range(0, outNeuronCount)
      .Select(i => (ulong)(inNeuronCount + i))
      .ToArray();

    var inputRanges = new List<Range>(
      angularRanges.Length * 2 +
      linearRanges.Length * 1
    );
    inputRanges.AddRange(angularRanges); // theta lower
    // inputRanges.AddRange(angularRanges); // theta dot lower
    inputRanges.AddRange(angularRanges); // theta upper
    // inputRanges.AddRange(angularRanges); // theta dot upper
    inputRanges.AddRange(linearRanges); // x
    // inputRanges.AddRange(linearRanges); // x dot
    NetworkIO.inputRanges = inputRanges.ToArray();

    Assert.AreEqual(inputRanges.Count, inNeuronCount,
      "Must produce correct number of input ranges");
    Assert.AreEqual(inNeuronIds.Length, inNeuronCount,
      "Must produce correct number of input neuron ids");
    Assert.AreEqual(outNeuronIds.Length, outNeuronCount,
      "Must produce correct number of output neuron ids");
  }

  readonly Neural.Network network;
  readonly int neuronCount;

  double[] input;
  double[] output;
  float[] worldData;

  // Responsible for relaying the genotype structure to the neural network.
  public static NetworkIO FromGenotype(NEAT.Genotype genotype) {
    var neuronGenes = genotype.NeuronGenes.ToList();
    var synapseGenes = genotype.SynapseGenes.ToList();

    var network = new Neural.Network();

    foreach (var neuronGene in neuronGenes) {
      float a = NumberHelper.Scale(neuronGene.a, 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(neuronGene.b, 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(neuronGene.c, -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(neuronGene.d, 0.05f, 8.0f); // 2.0

      try {
	      network.AddNeuron(a, b, c, d);
      } catch (Exception e) {
        Debug.LogException(e);
      }
    }

    // Connect each input neuron to the output neuron.
    foreach (var synapseGene in synapseGenes) {
      if (!synapseGene.isEnabled) {
        continue;
      }

      var fromNeuronId = neuronGenes.FindIndex(n => n.InnovationId == synapseGene.fromNeuronId);
      var toNeuronId = neuronGenes.FindIndex(n => n.InnovationId == synapseGene.toNeuronId);

      Assert.AreNotEqual(fromNeuronId, -1, "Must find from-neuron id");
      Assert.AreNotEqual(toNeuronId, -1, "Must find to-neuron id");

      float weight = NumberHelper.Scale(synapseGene.weight, -40.0f, 40.0f);

      try {
        network.AddSynapse((ulong)fromNeuronId, (ulong)toNeuronId, weight, -40.0f, 40.0f);
      } catch (Exception e) {
        Debug.LogException(e);
      }
    }

    return new NetworkIO(network);
  }

  public NetworkIO(Neural.Network network) {
    this.network = network;
    this.neuronCount = (int)network.NeuronCount;

    this.input = new double[this.neuronCount];
    this.output = new double[this.neuronCount];
    this.worldData = new float[inNeuronCount];
  }

  public static void PopulateWorldData(float[] worldData, float thetaLower, float thetaDotLower, float thetaUpper, float thetaDotUpper, float x, float xDot) {
    var aR = angularRanges.Length;
    var aR2 = aR * 2;
    // var aR3 = aR * 3;
    // var aR4 = aR * 4;
    var lR = linearRanges.Length;
    // var lR2 = lR * 2;

    // Project world data
    for (int i = 0; i < worldData.Length; i++) {
      if (i < aR) {
        worldData[i] = thetaLower;
      // } else if (i >= aR && i < aR2) {
      //   worldData[i] = thetaDotLower;
      } else if (i >= aR && i < aR2) {
        worldData[i] = thetaUpper;
      // } else if (i >= aR3 && i < aR4) {
      //   worldData[i] = thetaDotUpper;
      } else if (i >= aR2 && i < aR2 + lR) {
        worldData[i] = x;
      // } else if (i >= aR4 + lR && i < aR4 + lR2) {
      //   worldData[i] = xDot;
      }
    }
  }

  public static void MapInput(double[] input, float[] worldData) {
    // Filter world data by ranges
    Range range;
    float data;
    for (int i = 0; i < inNeuronIds.Length; i++) {
      range = inputRanges[i];
      data = worldData[i];
      if (range.Contains(data)) {
        input[inNeuronIds[i]] = 30.0;
      }
    }
  }

  public static float MapOutput(double[] output) {
    // Read out neuron V for speed
    float speed = 0.0f;
    for (int i = 0; i < outNeuronIds.Length; i++) {
      speed += (float)((output[outNeuronIds[i]] / 30.0) * speeds[i]);
    }
    return speed;
  }

  public float Send(float thetaLower, float thetaDotLower, float thetaUpper, float thetaDotUpper, float x, float xDot) {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
    Array.Clear(worldData, 0, worldData.Length);

    PopulateWorldData(worldData, thetaLower, thetaDotLower, thetaUpper, thetaDotUpper, x, xDot);
    MapInput(input, worldData);

    // Receive output
    network.Tick(20ul, input, ref output); // TODO: fixedDeltaTime?
    return MapOutput(output);
  }
}

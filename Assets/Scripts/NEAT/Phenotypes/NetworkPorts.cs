using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Something like a blackboard: IO is written to arrays that Network reads and writes
public class NetworkPorts {

  public readonly NetworkInputPort UpperTheta;
  public readonly NetworkInputPort LowerTheta;
  public readonly NetworkInputPort Position;

  public readonly NetworkSumOutputPort Speed;

  private readonly double[] input;
  private double[] output;

  public static readonly int inputNeuronCount;
  public static readonly int outputNeuronCount;
  public static readonly int initialNeuronCount;

  static IReceptiveField[] rotation;
  static IReceptiveField[] position;

  static IReceptiveField[] IntervalHelper(double[] intervals) {
    var rfs = new List<IReceptiveField>();
    for (int i = 0; i < intervals.Length - 1; i++) {
      var start = intervals[i];
      var end = intervals[i + 1];

      var sigma = Math.Abs(start - end);
      var mean = start + (sigma / 2);

      rfs.Add(new SignReceptiveField(mean, sigma));
    }
    return rfs.ToArray();
  }

  static readonly double[] speeds = new double[]{
    -250.0, -200.0, -150.0, -100.0, -50.0, -25.0, -10.0, -5.0, -1.0, -0.1, 0.1, 1.0, 5.0, 10.0, 25.0, 50.0, 100.0, 150.0, 200.0, 250.0
  };

  static NetworkPorts() {
    rotation = IntervalHelper(new double[]{
      -180.0, -150.0, -120.0, -90.0, -75.0, -60.0, -45.0, -30.0, -15.0, -5.0, -1.0, 0.0, 1.0, 5.0, 15.0, 30.0, 45.0, 60.0, 75.0, 90.0, 120.0, 150.0, 180.0,
    });

    position = IntervalHelper(new double[]{
      -6.0, -5.0, -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 5.0, 6.0,
    });

    inputNeuronCount = rotation.Length * 2 + position.Length;
    outputNeuronCount = speeds.Length;
    initialNeuronCount = inputNeuronCount + outputNeuronCount;
  }

  private readonly Neural.Network network;

  public NetworkPorts(Neural.Network network) {
    if (network == null) {
      return;
    }

    this.network = network;

    this.input = new double[network.NeuronCount];
    this.output = new double[network.NeuronCount];

    var inputs = new Slicer<double>(input);
    LowerTheta = new NetworkInputPort(inputs, rotation);
    UpperTheta = new NetworkInputPort(inputs, rotation);
    Position = new NetworkInputPort(inputs, position);

    var outputs = new Slice<double>(output, inputNeuronCount, outputNeuronCount);
    Speed = new NetworkSumOutputPort(outputs, speeds);
  }

  public void Clear() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
  }

  public void Tick() {
    network.Tick(20ul, input, ref output);
  }

  public static NetworkPorts FromGenotype(NEAT.Genotype genotype) {
    var neuronGenes = genotype.NeuronGenes.ToList();
    var synapseGenes = genotype.SynapseGenes.ToList();

    var network = new Neural.Network(20ul);

    foreach (var neuronGene in neuronGenes) {
      float a = NumberHelper.Scale(neuronGene.a, 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(neuronGene.b, 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(neuronGene.c, -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(neuronGene.d, 0.05f, 8.0f); // 2.0

      try {
	      network.AddNeuron(Neural.IzhikevichConfig.Of(a, b, c, d));
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
        network.AddSynapse((ulong)fromNeuronId, (ulong)toNeuronId, Neural.STDPConfig.Of(weight, -40.0f, 40.0f));
      } catch (Exception e) {
        Debug.LogException(e);
      }
    }

    return new NetworkPorts(network);
  }
}

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Something like a blackboard: IO is written to arrays that Network reads and writes
public class NetworkPorts {

  public readonly NetworkInputPort UpperTheta;
  // public readonly NetworkInputPort UpperThetaDot;

  public readonly NetworkInputPort LowerTheta;
  // public readonly NetworkInputPort LowerThetaDot;

  public readonly NetworkInputPort Position;
  // public readonly NetworkInputPort Velocity;

  public readonly NetworkSpikeRatePort FastForward;
  public readonly NetworkSpikeRatePort FastBackward;
  public readonly NetworkSpikeRatePort MediumForward;
  public readonly NetworkSpikeRatePort MediumBackward;
  public readonly NetworkSpikeRatePort SlowForward;
  public readonly NetworkSpikeRatePort SlowBackward;

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
      var a = intervals[i];
      var b = intervals[i + 1];
      var w = Math.Abs(a - b);
      var rf = new SignReceptiveField(a + (w / 2), w);
      rfs.Add(rf);
    }
    return rfs.ToArray();
  }

  static NetworkPorts() {
    rotation = IntervalHelper(new double[]{
      -180.0, -150.0, -120.0, -90.0, -75.0, -60.0, -45.0, -30.0, -15.0, -5.0, -1.0,
      0.0, 1.0, 5.0, 15.0, 30.0, 45.0, 60.0, 75.0, 90.0, 120.0, 150.0, 180.0,
    });
    position = EnumerableHelper.Range(-6.0, 6.0, 1.0f)
      .Select(m => new SignReceptiveField(m, 1.0))
      .Cast<IReceptiveField>()
      .ToArray();

    inputNeuronCount =
      rotation.Length * 2 + position.Length;
    outputNeuronCount = 6;

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
    var outputs = new Slicer<double>(output);

    UpperTheta = new NetworkInputPort(inputs, rotation);
    LowerTheta = new NetworkInputPort(inputs, rotation);
    Position = new NetworkInputPort(inputs, position);

    FastForward = new NetworkSpikeRatePort(outputs, 1);
    FastBackward = new NetworkSpikeRatePort(outputs, 1);
    MediumForward = new NetworkSpikeRatePort(outputs, 1);
    MediumBackward = new NetworkSpikeRatePort(outputs, 1);
    SlowForward = new NetworkSpikeRatePort(outputs, 1);
    SlowBackward = new NetworkSpikeRatePort(outputs, 1);
  }

  public void Tick() {
    network.Tick(20ul, input, ref output);

    FastForward.Tick();
    FastBackward.Tick();
    MediumForward.Tick();
    MediumBackward.Tick();
    SlowForward.Tick();
    SlowBackward.Tick();
  }

  public static NetworkPorts FromGenotype(NEAT.Genotype genotype) {
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

    return new NetworkPorts(network);
  }
}
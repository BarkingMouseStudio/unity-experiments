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

  private readonly Neural.Network network;

  public NetworkPorts(Neural.Network network,
    IDictionary<int, IReceptiveField> upperNeurons,
    IDictionary<int, IReceptiveField> lowerNeurons,
    IDictionary<int, IReceptiveField> positionNeurons,
    IDictionary<int, IReceptiveField> speedNeurons
  ) {
    this.network = network;

    this.input = new double[network.NeuronCount];
    this.output = new double[network.NeuronCount];

    UpperTheta = new NetworkInputPort(input, upperNeurons);
    LowerTheta = new NetworkInputPort(input, lowerNeurons);
    Position = new NetworkInputPort(input, positionNeurons);

    Speed = new NetworkSumOutputPort(output, speedNeurons);
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

    var upperNeurons = new Dictionary<int, IReceptiveField>();
    var lowerNeurons = new Dictionary<int, IReceptiveField>();
    var positionNeurons = new Dictionary<int, IReceptiveField>();
    var speedNeurons = new Dictionary<int, IReceptiveField>();

    foreach (var neuronGene in neuronGenes) {
      float a = NumberHelper.Scale(neuronGene.a, 0.02f, 0.1f); // 0.1
      float b = NumberHelper.Scale(neuronGene.b, 0.2f, 0.25f); // 0.2
      float c = NumberHelper.Scale(neuronGene.c, -65.0f, -50.0f); // -65.0
      float d = NumberHelper.Scale(neuronGene.d, 0.05f, 8.0f); // 2.0

      try {
	      var id = (int)network.AddNeuron(Neural.IzhikevichConfig.Of(a, b, c, d));

        var mean = 0.0f;
        var sigma = 0.0f;
        switch (neuronGene.type) {
          case NeuronType.UpperNeuron:
            mean = NumberHelper.Scale(neuronGene.mean, -180.0f, 180.0f);
            sigma = NumberHelper.Scale(neuronGene.sigma, 0.0f, 180.0f);
            upperNeurons[id] = new SignReceptiveField(mean, sigma);
            break;
          case NeuronType.LowerNeuron:
            mean = NumberHelper.Scale(neuronGene.mean, -180.0f, 180.0f);
            sigma = NumberHelper.Scale(neuronGene.sigma, 0.0f, 180.0f);
            lowerNeurons[id] = new SignReceptiveField(mean, sigma);
            break;
          case NeuronType.PositionNeuron:
            mean = NumberHelper.Scale(neuronGene.mean, -12.0f, 12.0f);
            sigma = NumberHelper.Scale(neuronGene.sigma, 0.0f, 12.0f);
            positionNeurons[id] = new SignReceptiveField(mean, sigma);
            break;
          case NeuronType.SpeedNeuron:
            mean = NumberHelper.Scale(neuronGene.mean, -1.0f, 1.0f);
            sigma = NumberHelper.Scale(neuronGene.sigma, 0.0f, 1000.0f);
            speedNeurons[id] = new MulReceptiveField(mean, sigma);
            break;
        }
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
        network.AddSynapse((ulong)fromNeuronId, (ulong)toNeuronId, Neural.SymConfig.Of(weight, -40.0f, 40.0f));
      } catch (Exception e) {
        Debug.LogException(e);
      }
    }

    return new NetworkPorts(network, upperNeurons, lowerNeurons, positionNeurons, speedNeurons);
  }
}

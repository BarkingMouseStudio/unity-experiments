using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActuatorPorts {

  public readonly double[] input;
  public double[] output;

  private readonly Neural.Network network;

  public PopulationPort shoulderProprioception;
  public PopulationPort elbowProprioception;

  public PopulationPort targetDirection;

  public PopulationPort shoulderMotorCommand;
  public PopulationPort elbowMotorCommand;

  public const int inputLayerSize = 100;
  public const int outputLayerSize = 100;

  private int neuronCount;
  private int synapseCount;
  private double[] weights;
  private float w_min, w_max;

  public ActuatorPorts(float w_min, float w_max, float sigma, float F_max) {
    this.w_min = w_min;
    this.w_max = w_max;

    network = new Neural.Network(20ul);

    // Proprioception: angles of each respective joint (shoulder x 3: roll,
    // pitch, yaw; and elbow x 1).
    var L_input_1 = CreateLayer(inputLayerSize);
    var L_input_2 = CreateLayer(inputLayerSize);

    // Spatial direction `target` should move at the next time step
    // each layer encodes the projection of the 3D directional vector
    // to one of the world axes.
    var L_input_3 = CreateLayer(inputLayerSize);

    // Firing pattern of each output layer representing the motor command that
    // is provided to a single joint.
    var L_output_1 = CreateLayer(outputLayerSize);
    var L_output_2 = CreateLayer(outputLayerSize);

    // Each neuron in the input layers is connected with an excitatory and
    // inhibitory synapse to each output neuron.
    // (Torque or angular velocity to apply to each joint.)
    Connect(L_input_1, L_output_1);
    Connect(L_input_1, L_output_2);

    Connect(L_input_2, L_output_1);
    Connect(L_input_2, L_output_2);

    Connect(L_input_3, L_output_1);
    Connect(L_input_3, L_output_2);

    neuronCount = (int)network.NeuronCount;
    synapseCount = (int)network.SynapseCount;
    Debug.LogFormat("Neuron Count: {0}, Synapse Count: {1}", neuronCount, synapseCount);

    this.input = new double[neuronCount];
    this.output = new double[neuronCount];
    this.weights = new double[synapseCount];

    shoulderProprioception = new PopulationPort(new ArraySegment<double>(input, 0, inputLayerSize), sigma, F_max);
    elbowProprioception = new PopulationPort(new ArraySegment<double>(input, inputLayerSize, inputLayerSize), sigma, F_max);
    targetDirection = new PopulationPort(new ArraySegment<double>(input, inputLayerSize * 2, inputLayerSize), sigma, F_max);
    shoulderMotorCommand = new PopulationPort(new ArraySegment<double>(output, inputLayerSize * 3, outputLayerSize), sigma, F_max);
    elbowMotorCommand = new PopulationPort(new ArraySegment<double>(output, inputLayerSize * 3 + outputLayerSize, outputLayerSize), sigma, F_max);
  }

  private List<int> CreateLayer(int layerSize) {
    var layer = new List<int>(layerSize);
    for (var i = 0; i < layerSize; i++) {
      var config = Neural.IzhikevichConfig.Of(0.02f, 0.2f, -65.0f, 2.0f);
      var id = (int)network.AddNeuron(config);
      layer.Add(id);
    }
    return layer;
  }

  private void Connect(List<int> L_input, List<int> L_output) {
    foreach (var inputId in L_input) {
      foreach (var outputId in L_output) {
        network.AddSynapse((ulong)inputId, (ulong)outputId,
          Neural.SymConfig.Of(RandomHelper.NextGaussian(), w_min, w_max));
      }
    }
  }

  public double[] DumpWeights() {
    Array.Clear(weights, 0, synapseCount);
    network.DumpWeights(ref weights);
    return weights;
  }

  public void Clear() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
  }

  public void Noise(float v) {
    for (var i = 0; i < input.Length; ++i) {
      input[i] = UnityEngine.Random.value * v;
    }
  }

  public void Tick() {
    network.Tick(20ul, input, ref output);
  }
}

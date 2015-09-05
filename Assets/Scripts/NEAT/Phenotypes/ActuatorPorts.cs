using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActuatorPorts {

  public readonly double[] input;
  public double[] output;
  public double[] rate;
  public double[] weights;

  public double totalRate = 0.0;
  public double averageRate = 0.0;

  public PopulationPort shoulderProprioception;
  public PopulationPort elbowProprioception;
  public PopulationPort targetDirection;
  public PopulationPort shoulderMotorCommand;
  public PopulationPort elbowMotorCommand;

  readonly Neural.Network network;

  double[,] spikeTimes;
  int spikeIndex;

  int neuronCount;
  int synapseCount;

  int inputLayerSize = 200;
  int outputLayerSize = 200;
  // int hiddenLayerSize = 10;

  int spikeWindow = 1; // Multiples of delta-time (0.02s)
  int ticksPerFrame = 20;

  float peakV = 30.0f; // Voltage returned as a spike
  float spikeV = 120.0f; // Voltage required to elicit a spike
	float sigma = 3.0f;
	float F_max = 100.0f;
	float w_min = -5.0f;
	float w_max = 5.0f;

  public ActuatorPorts() {
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

    // Hidden layer to circumvent binning. Basically a combination of input + hidden allows differentiating "like" inputs to produce unique outputs.
    // var L_hidden_1 = CreateLayer(hiddenLayerSize);

    // Each neuron in the input layers is connected with an excitatory and
    // inhibitory synapse to each output neuron.
    // (Torque or angular velocity to apply to each joint.)
    Connect(L_input_1, L_output_1, w_min, 0);
    Connect(L_input_1, L_output_1, 0, w_max);

    Connect(L_input_1, L_output_2, w_min, 0);
    Connect(L_input_1, L_output_2, 0, w_max);

    Connect(L_input_2, L_output_1, w_min, 0);
    Connect(L_input_2, L_output_1, 0, w_max);

    Connect(L_input_2, L_output_2, w_min, 0);
    Connect(L_input_2, L_output_2, 0, w_max);

    Connect(L_input_3, L_output_1, w_min, 0);
    Connect(L_input_3, L_output_1, 0, w_max);

    Connect(L_input_3, L_output_2, w_min, 0);
    Connect(L_input_3, L_output_2, 0, w_max);

    // Connect hidden
    // Connect(L_input_1, L_hidden_1, w_min, 0);
    // Connect(L_input_1, L_hidden_1, 0, w_max);

    // Connect(L_input_2, L_hidden_1, w_min, 0);
    // Connect(L_input_2, L_hidden_1, 0, w_max);

    // Connect(L_input_3, L_hidden_1, w_min, 0);
    // Connect(L_input_3, L_hidden_1, 0, w_max);

    neuronCount = (int)network.NeuronCount;
    synapseCount = (int)network.SynapseCount;

    Debug.LogFormat("Neuron Count: {0}, Synapse Count: {1}", neuronCount, synapseCount);

    this.input = new double[neuronCount * ticksPerFrame];
    this.output = new double[neuronCount];
    this.spikeTimes = new double[neuronCount,spikeWindow];
    this.rate = new double[neuronCount];
    this.weights = new double[synapseCount];

    shoulderProprioception = new PopulationPort(
      input, rate, 0,
      inputLayerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    elbowProprioception = new PopulationPort(
      input, rate, inputLayerSize,
      inputLayerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    targetDirection = new PopulationPort(
      input, rate, inputLayerSize * 2,
      inputLayerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    shoulderMotorCommand = new PopulationPort(
      input, rate, inputLayerSize * 3,
      outputLayerSize, neuronCount,
      sigma, F_max, spikeV, -5, 5);

    elbowMotorCommand = new PopulationPort(
      input, rate, inputLayerSize * 3 + outputLayerSize,
      outputLayerSize, neuronCount,
      sigma, F_max, spikeV, -5, 5);
  }

  private List<int> CreateLayer(int layerSize) {
    var layer = new List<int>(layerSize);
    for (var i = 0; i < layerSize; i++) {
      var config = Neural.IzhikevichConfig.Of(0.1f, 0.2f, -65.0f, 2.0f);
      var id = (int)network.AddNeuron(config);
      layer.Add(id);
    }
    return layer;
  }

  private void Connect(List<int> L_input, List<int> L_output, float min, float max) {
    foreach (var inputId in L_input) {
      foreach (var outputId in L_output) {
        network.AddSynapse((ulong)inputId, (ulong)outputId,
          Neural.SymConfig.Of(RandomHelper.NextGaussian(), min, max));
      }
    }
  }

  public double[] DumpWeights() {
    Array.Clear(weights, 0, synapseCount);
    network.DumpWeights(weights);
    return weights;
  }

  public void Clear() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
    Array.Clear(rate, 0, rate.Length);
  }

  public void Noise(float rate, float v) {
    if (rate > 0.0f && v > 0.0f) {
      for (var i = 0; i < neuronCount; i++) {
        for (var t = 0; t < ticksPerFrame; t++) {
          input[t * neuronCount + i] += RandomHelper.PoissonInput(rate, v);
        }
      }
    }
  }

  public void Tick() {
    network.Tick((ulong)ticksPerFrame, input, output);

    totalRate = 0.0;
    for (var i = 0; i < output.Length; i++) {
      spikeTimes[i,spikeIndex] = output[i] / peakV; // 20ms
      for (var j = 0; j < spikeWindow; j++) {
        rate[i] += ((float)spikeTimes[i,j]);
      }
      rate[i] *= (1000f / (spikeWindow * ticksPerFrame));
      totalRate += rate[i];
    }

    spikeIndex = (spikeIndex + 1) % spikeWindow;
    averageRate = totalRate / neuronCount;
  }
}

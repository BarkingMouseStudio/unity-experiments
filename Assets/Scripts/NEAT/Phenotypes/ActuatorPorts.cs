using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ActuatorPorts {

  public float[] input;
  public float[] output;
  public float[] rate;
  public float[] weights;

  public float totalRate = 0.0f;
  public float averageRate = 0.0f;

  public PopulationPort shoulderProprioception;
  public PopulationPort elbowProprioception;
  public PopulationPort targetDirection;
  public PopulationPort shoulderMotorCommand;
  public PopulationPort elbowMotorCommand;

  Neural.Network network;
  int neuronCount, synapseCount;
  const int ticksPerFrame = 20;
  float peakV = 30.0f; // voltage returned as a spike

  float[,] spikeTimes;
  int spikeIndex;
  const int spikeWindow = 2; // multiples of delta-time (0.02s)
  float rateMultiplier = 1000f / (spikeWindow * ticksPerFrame);

  const int layerSize = 100;
  float sigma = layerSize * 0.02f;
  float spikeV = 120.0f; // voltage required to elicit a spike

	float F_max = 500.0f;
  float w_min = -4;
  float w_max = +4;

  public ActuatorPorts() {
    network = new Neural.Network(20ul);

    // Proprioception: angles of each respective joint (shoulder x 3: roll,
    // pitch, yaw; and elbow x 1).
    var L_input_1 = CreateLayer(layerSize);
    var L_input_2 = CreateLayer(layerSize);

    // Spatial direction `target` should move at the next time step
    // each layer encodes the projection of the 3D directional vector
    // to one of the world axes.
    var L_input_3 = CreateLayer(layerSize);

    // Firing pattern of each output layer representing the motor command that
    // is provided to a single joint.
    var L_output_1 = CreateLayer(layerSize);
    var L_output_2 = CreateLayer(layerSize);

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

    neuronCount = (int)network.NeuronCount;
    synapseCount = (int)network.SynapseCount;

    Debug.LogFormat("Neuron Count: {0}, Synapse Count: {1}", neuronCount, synapseCount);

    this.input = new float[neuronCount * ticksPerFrame];
    this.output = new float[neuronCount];
    this.spikeTimes = new float[neuronCount,spikeWindow];
    this.rate = new float[neuronCount];
    this.weights = new float[synapseCount];

    shoulderProprioception = new PopulationPort(input, rate,
      0, layerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    elbowProprioception = new PopulationPort(input, rate,
      layerSize, layerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    targetDirection = new PopulationPort(input, rate,
      layerSize * 2, layerSize, neuronCount,
      sigma, F_max, spikeV, -180, 180);

    shoulderMotorCommand = new PopulationPort(input, rate,
      layerSize * 3, layerSize, neuronCount,
      sigma, F_max, spikeV, -5, 5);

    elbowMotorCommand = new PopulationPort(input, rate,
      layerSize * 3 + layerSize, layerSize, neuronCount,
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
        var config = Neural.SymConfig.Of(RandomHelper.NextGaussianRange(min, max), min, max);
        // config.a_sym = 0.05;
        network.AddSynapse((ulong)inputId, (ulong)outputId,
          config);
      }
    }
  }

  public float[] DumpWeights() {
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
    if (rate > 0 && v > 0) {
      for (var i = 0; i < neuronCount; i++) {
        for (var t = 0; t < ticksPerFrame; t++) {
          input[t * neuronCount + i] += RandomHelper.PoissonInput(rate, v);
        }
      }
    }
  }

  public void ToggleTraining(bool enabled) {
    if (enabled) {
      network.ToggleTransmission(false); // Disable synaptic transmission
      network.ToggleLearning(true); // Enable synaptic learning
    } else {
      network.ToggleTransmission(true); // Enable synaptic transmission
      network.ToggleLearning(false); // Disable synaptic learning
    }
  }

  public void Tick() {
    network.Tick((ulong)ticksPerFrame, input, output);

    totalRate = 0.0f;
    for (var i = 0; i < output.Length; i++) {
      spikeTimes[i,spikeIndex] = output[i] / peakV; // 20ms
      for (var j = 0; j < spikeWindow; j++) {
        rate[i] += spikeTimes[i,j];
      }
      rate[i] *= rateMultiplier;
      totalRate += rate[i];
    }
    averageRate = totalRate / neuronCount;

    spikeIndex = (spikeIndex + 1) % spikeWindow;
  }
}

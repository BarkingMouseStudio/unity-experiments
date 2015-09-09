using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ChannelBehaviour : MonoBehaviour {

  public bool isTraining = true;

	public bool saveWeights = false;
	public float saveWeightsInterval = 10.0f;
	float nextSaveWeights = 0.0f;
  StreamWriter weightsLog;

  public float noiseV = 30f;
  public float noiseRate = 3f;

  const int layerSize = 100;
  float spikeV = 120f;
  float sigma = layerSize * 0.02f;
  float minValue = -1;
  float maxValue = 1;

  float F_max = 500f;
  float w_min = -4;
  float w_max = +4;

  float[] input;
  float[] output;
  float[] rate;
  float[] weights;

  Neural.Network network;
  int neuronCount, synapseCount;
  const int ticksPerFrame = 20; // fixed delta time = 0.02s (20ms)
  float peakV = 30.0f; // voltage returned as a spike

  float[,] spikeTimes;
  int spikeIndex = 0;
  const int spikeWindow = 2; // 1 : 20ms
  float rateMultiplier = 1000f / (spikeWindow * ticksPerFrame);

  #pragma warning disable 0414
  float averageRate = 0.0f;
  #pragma warning restore 0414

  PopulationPort inputPopulation, outputPopulation;
  float expectedInputValue, expectedOutputValue;

  #pragma warning disable 0414
  float inputValue, outputValue;
  #pragma warning restore 0414

  #pragma warning disable 0414
  float errValue;
  #pragma warning restore 0414

  private List<ulong> CreateLayer(int layerSize) {
    var layer = new List<ulong>(layerSize);
    for (var i = 0; i < layerSize; i++) {
      var config = Neural.IzhikevichConfig.Of(0.1f, 0.2f, -65.0f, 2.0f);
      var id = network.AddNeuron(config);
      layer.Add(id);
    }
    return layer;
  }

  private void ConnectLayers(List<ulong> L_input, List<ulong> L_output, float min, float max) {
    foreach (var inputId in L_input) {
      foreach (var outputId in L_output) {
        var weight = RandomHelper.NextGaussianRange(min, max);
        var config = Neural.SymConfig.Of(weight, min, max);
        // config.a_sym = 0.01f;
        network.AddSynapse(inputId, outputId, config);
      }
    }
  }

	void Awake() {
    // Setup logging
    var logPath = string.Format("logs_{0}", DateTime.Now.Ticks);
    Debug.LogFormat("Logging to {0}", logPath);
    if (!Directory.Exists(logPath)) {
      Directory.CreateDirectory(logPath);
    }
    weightsLog = File.CreateText(Path.Combine(logPath, "weights.csv"));

    // Setup network
    network = new Neural.Network(20ul);

    var inputLayer = CreateLayer(layerSize);
    var outputLayer = CreateLayer(layerSize);

    ConnectLayers(inputLayer, outputLayer, w_min, 0);
    ConnectLayers(inputLayer, outputLayer, 0, w_max);

    neuronCount = (int)network.NeuronCount;
    synapseCount = (int)network.SynapseCount;

    Debug.LogFormat("Neuron Count: {0}, Synapse Count: {1}", neuronCount, synapseCount);

    input = new float[neuronCount * ticksPerFrame];
    output = new float[neuronCount];
    rate = new float[neuronCount];
    spikeTimes = new float[neuronCount,spikeWindow];
    weights = new float[synapseCount];

    inputPopulation = new PopulationPort(input, rate,
      0, layerSize, neuronCount,
      sigma, F_max, spikeV, minValue, maxValue);

    outputPopulation = new PopulationPort(input, rate,
      layerSize, layerSize, neuronCount,
      sigma, F_max, spikeV, minValue, maxValue);
	}

	void FixedUpdate() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
    Array.Clear(rate, 0, rate.Length);

    if (isTraining && noiseRate > 0 && noiseV > 0) {
      for (var i = 0; i < neuronCount; i++) {
        for (var t = 0; t < ticksPerFrame; t++) {
          input[t * neuronCount + i] += RandomHelper.PoissonInput(noiseRate, noiseV);
        }
      }
    }

    expectedInputValue = Mathf.Sin(Time.fixedTime);
	  inputPopulation.Set(expectedInputValue);

    expectedOutputValue = -1f * expectedInputValue;
    // expectedOutputValue = Mathf.Cos(Time.fixedTime);
    // expectedOutputValue = expectedInputValue;
    if (isTraining) {
      network.ToggleTransmission(false); // Disable synaptic transmission
      network.ToggleLearning(true); // Enable synaptic learning
      outputPopulation.Set(expectedOutputValue);
    } else {
      network.ToggleTransmission(true); // Enable synaptic transmission
      network.ToggleLearning(false); // Disable synaptic learning
    }

    network.Tick((ulong)ticksPerFrame, input, output);

    var totalRate = 0.0f;
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

		inputPopulation.TryGet(out inputValue);

		if (outputPopulation.TryGet(out outputValue)) {
      errValue = Mathf.Abs(expectedOutputValue - outputValue);
    } else {
      errValue = 1f;
    }

		if (saveWeights) {
			if (Time.time >= nextSaveWeights) {
				Debug.Log("Saving weights");
        Array.Clear(weights, 0, synapseCount);
        network.DumpWeights(weights);
				weightsLog.WriteLine(weights.Stringify());
				weightsLog.Flush();
				nextSaveWeights += saveWeightsInterval;
			}
		}
	}

  void OnApplicationQuit() {
    weightsLog.Close();
  }
}

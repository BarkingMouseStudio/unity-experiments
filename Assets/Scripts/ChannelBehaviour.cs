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

  int layerSize = 100;
  float spikeV = 120f;
  float sigma = 3f;
  float minValue = -1;
  float maxValue = 1;

  float F_max = 500f;
  float w_min = -5;
  float w_max = +5;

  float[] input;
  float[] output;
  float[] rate;
  float[] weights;

  Neural.Network network;
  int neuronCount, synapseCount;
  int ticksPerFrame = 20; // fixed delta time = 0.02s (20ms)
  float peakV = 30.0f; // voltage returned as a spike

  float[,] spikeTimes;
  int spikeIndex = 0;
  int spikeWindow = 1; // 1 : 20ms

  PopulationPort inputPopulation, outputPopulation;
  float inputValue, outputValue;
  float expectedInputValue, expectedOutputValue;

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
    var hiddenLayer = CreateLayer(layerSize);
    var outputLayer = CreateLayer(layerSize);

    ConnectLayers(inputLayer, hiddenLayer, w_min, 0);
    ConnectLayers(inputLayer, hiddenLayer, 0, w_max);

    ConnectLayers(hiddenLayer, outputLayer, w_min, 0);
    ConnectLayers(hiddenLayer, outputLayer, 0, w_max);

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

    if (noiseRate > 0 && noiseV > 0) {
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
      outputPopulation.Set(expectedOutputValue);
    }

    network.Tick((ulong)ticksPerFrame, input, output);

    for (var i = 0; i < output.Length; i++) {
      spikeTimes[i,spikeIndex] = output[i] / peakV; // 20ms
      for (var j = 0; j < spikeWindow; j++) {
        rate[i] += ((float)spikeTimes[i,j]);
      }
      rate[i] *= (1000f / (spikeWindow * ticksPerFrame));
    }

    spikeIndex = (spikeIndex + 1) % spikeWindow;

		inputPopulation.TryGet(out inputValue);
		outputPopulation.TryGet(out outputValue);
		errValue = Mathf.Abs(expectedOutputValue - outputValue);

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

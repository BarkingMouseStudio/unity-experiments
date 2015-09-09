using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

// Demonstrates firing a neuron at a particular rate
public class RateBehaviour : MonoBehaviour {

  public float rate = 0.0f;
  public float v = 0.0f;

  float[] input;
  float[] output;

  Neural.Network network;

  float instantRateA = 0.0f;
  float instantRateB = 0.0f;
  float instantRateC = 0.0f;

  #pragma warning disable 0414
  float spikeRateA = 0.0f; // Hz
  float spikeRateB = 0.0f; // Hz
  float spikeRateC = 0.0f; // Hz
  #pragma warning restore 0414

  float inputRateA = 0.0f;
  float inputRateB = 0.0f;
  float inputRateC = 0.0f;

  float[][] spikes;
  int spikeIndex = 0;
  int spikeWindow = 10;

  int neuronCount = 0;

	void Awake() {
    network = new Neural.Network(20ul);
    for (var i = 0; i < 3; i++) {
      network.AddNeuron(Neural.IzhikevichConfig.Of(0.1f, 0.2f, -65.0f, 2.0f));
    }

    neuronCount = (int)network.NeuronCount;

    input = new float[neuronCount * 20];
    output = new float[neuronCount];

    spikes = new float[neuronCount][];
    for (var i = 0; i < neuronCount; i++) {
      spikes[i] = new float[spikeWindow];
    }
	}

	void FixedUpdate() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);

    for (var i = 0; i < neuronCount; i++) {
      for (var t = 0; t < 20; t++) {
        input[t * neuronCount + i] += RandomHelper.PoissonInput(rate, v);
      }
    }

    inputRateA = 0.0f;
    inputRateB = 0.0f;
    inputRateC = 0.0f;

    for (var t = 0; t < 20; t++) {
      inputRateA += input[t * neuronCount + 0];
      inputRateB += input[t * neuronCount + 1];
      inputRateC += input[t * neuronCount + 2];
    }

    network.Tick(20ul, input, output);

    instantRateA = output[0] / 30.0f;
    instantRateB = output[1] / 30.0f;
    instantRateC = output[2] / 30.0f;

		spikes[0][spikeIndex] = instantRateA;
		spikes[1][spikeIndex] = instantRateB;
		spikes[2][spikeIndex] = instantRateC;

		spikeIndex = (spikeIndex + 1) % spikeWindow;

		spikeRateA = spikes[0].Aggregate(0.0f, (sum, s) => sum + s, (sum) => sum * 5.0f);
		spikeRateB = spikes[1].Aggregate(0.0f, (sum, s) => sum + s, (sum) => sum * 5.0f);
		spikeRateC = spikes[2].Aggregate(0.0f, (sum, s) => sum + s, (sum) => sum * 5.0f);
	}
}

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

public class RateBehaviour : MonoBehaviour {

  public float rate = 0.0f;
  public float v = 0.0f;

  double[] input;
  double[] output;

  Neural.Network network;

  double instantRateA = 0.0;
  double instantRateB = 0.0;
  double instantRateC = 0.0;

  #pragma warning disable 0414
  double spikeRateA = 0.0; // Hz
  double spikeRateB = 0.0; // Hz
  double spikeRateC = 0.0; // Hz
  #pragma warning restore 0414

  double inputRateA = 0.0;
  double inputRateB = 0.0;
  double inputRateC = 0.0;

  double[][] spikes;
  int spikeIndex = 0;
  int spikeWindow = 10;

  int neuronCount = 0;

	void Awake() {
    network = new Neural.Network(20ul);
    for (var i = 0; i < 3; i++) {
      network.AddNeuron(Neural.IzhikevichConfig.Of(0.1f, 0.2f, -65.0f, 2.0f));
    }

    neuronCount = (int)network.NeuronCount;

    input = new double[neuronCount * 20];
    output = new double[neuronCount];

    spikes = new double[neuronCount][];
    for (var i = 0; i < neuronCount; i++) {
      spikes[i] = new double[spikeWindow];
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

    inputRateA = 0.0;
    inputRateB = 0.0;
    inputRateC = 0.0;

    for (var t = 0; t < 20; t++) {
      inputRateA += input[t * neuronCount + 0];
      inputRateB += input[t * neuronCount + 1];
      inputRateC += input[t * neuronCount + 2];
    }

    network.Tick(20ul, input, output);

    instantRateA = output[0] / 30.0;
    instantRateB = output[1] / 30.0;
    instantRateC = output[2] / 30.0;

		spikes[0][spikeIndex] = instantRateA;
		spikes[1][spikeIndex] = instantRateB;
		spikes[2][spikeIndex] = instantRateC;

		spikeIndex = (spikeIndex + 1) % spikeWindow;

		spikeRateA = spikes[0].Aggregate(0.0, (sum, s) => sum + s, (sum) => sum * 5.0f);
		spikeRateB = spikes[1].Aggregate(0.0, (sum, s) => sum + s, (sum) => sum * 5.0f);
		spikeRateC = spikes[2].Aggregate(0.0, (sum, s) => sum + s, (sum) => sum * 5.0f);
	}
}

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

public class PopulationBehaviour : MonoBehaviour {

  double[] input;
  double[] output;

  Neural.Network network;
  int neuronCount = 0;
  int ticksPerFrame = 20; // fixed delta time = 0.02s (20ms)

  double[,] spikeTimes;
  int spikeIndex = 0;
  int spikeWindow = 1; // 1 : 20ms
  double[] rate;

  PopulationPort population;
  float inputValue, outputValue;

  #pragma warning disable 0414
  float errValue;
  #pragma warning restore 0414

	void Awake() {
    network = new Neural.Network(20ul);

    for (var i = 0; i < 100; i++) {
      network.AddNeuron(Neural.IzhikevichConfig.Of(0.1, 0.2, -65.0, 2.0));
    }

    neuronCount = (int)network.NeuronCount;

    input = new double[neuronCount * ticksPerFrame];
    output = new double[neuronCount];

    spikeTimes = new double[neuronCount,spikeWindow];
    rate = new double[neuronCount];

    population = new PopulationPort(input, rate, 0, neuronCount, neuronCount, 3.0f, 100.0f, 120.0f);
	}

	void FixedUpdate() {
    Array.Clear(input, 0, input.Length);
    Array.Clear(output, 0, output.Length);
    Array.Clear(rate, 0, rate.Length);

    inputValue = NumberHelper.Normalize(Mathf.Sin(Time.fixedTime), -1f, 1f);
	  population.Set(inputValue);

    network.Tick((ulong)ticksPerFrame, input, output);

    for (var i = 0; i < output.Length; i++) {
      spikeTimes[i,spikeIndex] = output[i] / 30.0; // 20ms

      for (var j = 0; j < spikeWindow; j++) {
        rate[i] += ((float)spikeTimes[i,j]);
      }
      rate[i] *= (1000f / (spikeWindow * ticksPerFrame));
    }

    spikeIndex = (spikeIndex + 1) % spikeWindow;

		population.TryGet(out outputValue);
		errValue = Mathf.Abs(inputValue - outputValue);
	}
}

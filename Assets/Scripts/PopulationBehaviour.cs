using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

// Demonstrates a population of neurons representing a value
public class PopulationBehaviour : MonoBehaviour {

  public float noiseV = 30f;
  public float noiseRate = 3f;

  float[] input;
  float[] output;

  Neural.Network network;
  int neuronCount = 0;
  int ticksPerFrame = 20; // fixed delta time = 0.02s (20ms)

  float[,] spikeTimes;
  int spikeIndex = 0;
  int spikeWindow = 1; // 1 : 20ms
  float[] rate;

  PopulationPort population;
  float inputValue, outputValue;

  #pragma warning disable 0414
  float errValue;
  #pragma warning restore 0414

	void Awake() {
    network = new Neural.Network(20ul);

    for (var i = 0; i < 100; i++) {
      network.AddNeuron(Neural.IzhikevichConfig.Of(0.1f, 0.2f, -65.0f, 2.0f));
    }

    neuronCount = (int)network.NeuronCount;

    input = new float[neuronCount * ticksPerFrame];
    output = new float[neuronCount];
    spikeTimes = new float[neuronCount,spikeWindow];
    rate = new float[neuronCount];

    population = new PopulationPort(input, rate, 0, neuronCount, neuronCount, 3.0f, 500.0f, 120.0f, -1, 1);
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

    inputValue = Mathf.Sin(Time.fixedTime);
	  population.Set(inputValue);

    network.Tick((ulong)ticksPerFrame, input, output);

    for (var i = 0; i < output.Length; i++) {
      spikeTimes[i,spikeIndex] = output[i] / 30.0f; // 20ms

      for (var j = 0; j < spikeWindow; j++) {
        rate[i] += spikeTimes[i,j];
      }
      rate[i] *= 1000f / (spikeWindow * ticksPerFrame);
    }

    spikeIndex = (spikeIndex + 1) % spikeWindow;

		population.TryGet(out outputValue);
		errValue = Mathf.Abs(inputValue - outputValue);
	}
}

// The Spike-count rate, also referred to as temporal average, is obtained by counting the number of spikes that appear during a trial and dividing by the duration of trial. The length T of the time window is set by experimenter and depends on the type of neuron recorded from and the stimulus. In practice, to get sensible averages, several spikes should occur within the time window. Typical values are T = 100 ms or T = 500 ms, but the duration may also be longer or shorter.[19]
// https://en.wikipedia.org/wiki/Neural_coding#Spike-count_rate
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NetworkSpikeRatePort {

  float windowSize = 0.1f; // 100ms

  int[] spikes = new int[5]; // 20ms per bucket
  int T = 0;

  double[] output;

  double spikeRate = 0.0f;

  public double Rate {
    get { return spikeRate / 1000.0f; }
  }

  public NetworkSpikeRatePort(double[] output) {
    this.output = output;
  }

  public void Tick() {
    spikes[T] = output.Sum(v => (int)(v / 30.0)); // count ticks
    T = (T + 1) % 5; // 20ms per bucket
    spikeRate = spikes.Sum() / windowSize; // count spikes; divide by `windowSize`
  }
}

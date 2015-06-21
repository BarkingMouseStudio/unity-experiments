using System.Collections;
using System.Collections.Generic;
using Neural;

// Responsible for taking the genotype representation and creating the
// phenotypical network.
public class CommonGenotype : IEnumerable<float> {

  private const MAX_DELAY = 20;
  private float[][] genotype;

  ulong[] inNeuronIds = new ulong[]{0, 1, 2, 3, 4, 5};
  ulong[] outNeuronIds = new ulong[]{6, 7, 8, 9};

  public CommonGenotype(float[][] genotype) {
    this.genotype = genotype;
  }

  public IEnumerator<float> GetEnumerator() {
    return this.genotype.GetEnumerator();
  }

  IEnumerator GetEnumerator() {
    return this.GetEnumerator();
  }

  public Network ToPhenotype() {
    var network = new Neural.Network(MAX_DELAY);

    int inNeuronCount = inNeuronIds.Length;
    int outNeuronCount = outNeuronIds.Length;
    int genotypeOffset = 0;

    for (int i = 0; i < inNeuronCount; i++) {
      float[] chromosome = genotype[i];
      double a = chromosome[0]; // 0.1
      double b = chromosome[1]; // 0.2
      double c = chromosome[2]; // -65.0
      double d = chromosome[3]; // 2.0
	    network.AddNeuron(a, b, c, d);
    }
    genotypeOffset += inNeuronCount;

    for (int i = 0; i < outNeuronCount; i++) {
      float[] chromosome = genotype[genotypeOffset + i];
      double a = chromosome[0]; // 0.1
      double b = chromosome[1]; // 0.2
      double c = chromosome[2]; // -65.0
      double d = chromosome[3]; // 2.0
	    network.AddNeuron(a, b, c, d);
    }
    genotypeOffset += outNeuronCount;

    // Connect each input neuron to the output neuron.
    for (var i = 0; i < inNeuronCount; i++) {
      for (var j = 0; j < outNeuronCount; j++) {
        float[] chromosome = genotype[genotypeOffset + (i * outNeuronCount) + j];
        double w = chromosome[0];
        network.AddSynapse(inNeuronIds[i], outNeuronIds[j], w, -40.0f, 40.0f);
      }
    }

    int neuronCount = (int)network.NeuronCount;
    AssertHelper.Assert(neuronCount == inNeuronIds.Length + outNeuronIds.Length,
      "Incorrect neuron count");

    return network;
  }
}

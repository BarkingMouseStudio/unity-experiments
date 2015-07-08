using UnityEngine;

namespace NEAT {

  public struct SynapseGene : IHistoricalGene {

    private int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public int fromNeuronId;
    public int toNeuronId;
    public bool isEnabled;
    public float weight;

    public static SynapseGene Random(int innovationId, int fromNeuronId, int toNeuronId, bool isEnabled) {
      return new SynapseGene(innovationId, fromNeuronId, toNeuronId, isEnabled, UnityEngine.Random.value);
    }

    public SynapseGene(int innovationId, int fromNeuronId, int toNeuronId, bool isEnabled) {
      this.innovationId = innovationId;
      this.fromNeuronId = fromNeuronId;
      this.toNeuronId = toNeuronId;
      this.isEnabled = isEnabled;
      this.weight = 0.5f;
    }

    public SynapseGene(int innovationId, int fromNeuronId, int toNeuronId, bool isEnabled, float weight) {
      this.innovationId = innovationId;
      this.fromNeuronId = fromNeuronId;
      this.toNeuronId = toNeuronId;
      this.isEnabled = isEnabled;
      this.weight = weight;
    }

    public SynapseGene Randomize(float toggleProbability) {
      var synapseGene = this;
      if (UnityEngine.Random.value < toggleProbability) {
        synapseGene.isEnabled = RandomHelper.NextBool();
      } else {
        synapseGene.weight = UnityEngine.Random.value;
      }
      return synapseGene;
    }

    public SynapseGene Perturb(float mutationScale, float toggleProbability) {
      var synapseGene = this;
      if (UnityEngine.Random.value < toggleProbability) {
        synapseGene.isEnabled = RandomHelper.NextBool();
      } else {
        synapseGene.weight = Mathf.Clamp01(RandomHelper.NextCauchy(synapseGene.weight, mutationScale));
      }
      return synapseGene;
    }
  }
}

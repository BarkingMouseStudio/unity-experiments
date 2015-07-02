using UnityEngine;

namespace NEAT {

  public struct SynapseGene : IHistoricalGene {

    const float mutationScale = 0.1f;
    const float toggleProbability = 0.2f;

    public int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public int fromId;
    public int toId;
    public bool isEnabled;
    public float weight;

    public SynapseGene(int innovationId, int fromId, int toId, bool isEnabled) {
      this.innovationId = innovationId;
      this.fromId = fromId;
      this.toId = toId;
      this.isEnabled = isEnabled;
      this.weight = Mathf.Clamp01(RandomHelper.NextCauchy(0.5f, mutationScale));
    }

    public SynapseGene(int innovationId, int fromId, int toId, bool isEnabled, float weight) {
      this.innovationId = innovationId;
      this.fromId = fromId;
      this.toId = toId;
      this.isEnabled = isEnabled;
      this.weight = weight;
    }

    public SynapseGene Randomize() {
      var synapseGene = this;
      synapseGene.weight = Mathf.Clamp01(RandomHelper.NextCauchy(0.5f, mutationScale));
      synapseGene.isEnabled = RandomHelper.NextBool();
      return synapseGene;
    }

    public SynapseGene Perturb() {
      var synapseGene = this;
      if (Random.value < toggleProbability) {
        synapseGene.isEnabled = RandomHelper.NextBool();
      } else {
        synapseGene.weight = Mathf.Clamp01(RandomHelper.NextCauchy(synapseGene.weight, mutationScale));
      }
      return synapseGene;
    }
  }
}

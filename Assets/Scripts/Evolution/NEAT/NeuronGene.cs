using UnityEngine;

namespace NEAT {

  public struct NeuronGene : IHistoricalGene {

    const float mutationScale = 0.1f;

    public int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public float a;
    public float b;
    public float c;
    public float d;

    public NeuronGene(int innovationId) {
      this.innovationId = innovationId;

      a = RandomHelper.NextCauchy(0.5f, mutationScale);
      b = RandomHelper.NextCauchy(0.5f, mutationScale);
      c = RandomHelper.NextCauchy(0.5f, mutationScale);
      d = RandomHelper.NextCauchy(0.5f, mutationScale);
    }

    public NeuronGene(int innovationId, float a, float b, float c, float d) {
      this.innovationId = innovationId;
      this.a = a;
      this.b = b;
      this.c = c;
      this.d = d;
    }

    /// Create a new NeuronGene with randomized properties.
    public NeuronGene Randomize() {
      return new NeuronGene(innovationId);
    }

    public NeuronGene Perturb() {
      return new NeuronGene(innovationId,
        RandomHelper.NextCauchy(a, mutationScale),
        RandomHelper.NextCauchy(b, mutationScale),
        RandomHelper.NextCauchy(c, mutationScale),
        RandomHelper.NextCauchy(d, mutationScale)
      );
    }
  }
}

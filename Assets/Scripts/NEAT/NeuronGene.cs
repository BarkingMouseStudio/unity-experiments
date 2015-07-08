using UnityEngine;

namespace NEAT {

  public struct NeuronGene : IHistoricalGene {

    private int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public float a;
    public float b;
    public float c;
    public float d;

    public static NeuronGene Random(int innovationId) {
      return new NeuronGene(innovationId,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value);
    }

    public NeuronGene(int innovationId) {
      this.innovationId = innovationId;
      this.a = 0.5f;
      this.b = 0.5f;
      this.c = 0.5f;
      this.d = 0.5f;
    }

    public NeuronGene(int innovationId, float a, float b, float c, float d) {
      this.innovationId = innovationId;
      this.a = a;
      this.b = b;
      this.c = c;
      this.d = d;
    }

    public NeuronGene Randomize() {
      return new NeuronGene(innovationId,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value
      );
    }

    public NeuronGene Perturb(float mutationScale) {
      return new NeuronGene(innovationId,
        RandomHelper.NextCauchy(a, mutationScale),
        RandomHelper.NextCauchy(b, mutationScale),
        RandomHelper.NextCauchy(c, mutationScale),
        RandomHelper.NextCauchy(d, mutationScale)
      );
    }
  }
}

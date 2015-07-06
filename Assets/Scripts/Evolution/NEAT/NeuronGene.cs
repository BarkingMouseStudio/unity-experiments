using UnityEngine;

namespace NEAT {

  public struct NeuronGene : IHistoricalGene {

    const float mutationScale = 0.1f;

    private int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public int id;

    public float a;
    public float b;
    public float c;
    public float d;

    public NeuronGene(int innovationId, int id) {
      this.innovationId = innovationId;
      this.id = id;
      this.a = 0.5f;
      this.b = 0.5f;
      this.c = 0.5f;
      this.d = 0.5f;
    }

    public NeuronGene(int innovationId, int id, float a, float b, float c, float d) {
      this.innovationId = innovationId;
      this.id = id;
      this.a = a;
      this.b = b;
      this.c = c;
      this.d = d;
    }

    public NeuronGene Randomize() {
      return new NeuronGene(innovationId, id,
        Random.value,
        Random.value,
        Random.value,
        Random.value
      );
    }

    public NeuronGene Perturb() {
      return new NeuronGene(innovationId, id,
        RandomHelper.NextCauchy(a, mutationScale),
        RandomHelper.NextCauchy(b, mutationScale),
        RandomHelper.NextCauchy(c, mutationScale),
        RandomHelper.NextCauchy(d, mutationScale)
      );
    }
  }
}

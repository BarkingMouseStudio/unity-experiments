using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public struct NeuronGene : IHistoricalGene {

    private int innovationId;
    public int InnovationId {
      get {
        return innovationId;
      }
    }

    public NeuronType type;
    public float mean;
    public float sigma;

    public float a;
    public float b;
    public float c;
    public float d;

    public static NeuronGene Random(int innovationId) {
      return new NeuronGene(
        innovationId,
        EnumHelper.Random<NeuronType>(),
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value,
        UnityEngine.Random.value);
    }

    public static NeuronGene FromPrototype(NeuronGene other) {
      return NeuronGene.Random(other.InnovationId);
    }

    public NeuronGene(int innovationId, NeuronType type) {
      this.type = type;
      this.innovationId = innovationId;
      this.a = 0.5f;
      this.b = 0.5f;
      this.c = 0.5f;
      this.d = 0.5f;
      this.mean = 0.5f;
      this.sigma = 1.0f;
    }

    public NeuronGene(int innovationId, NeuronType type, float a, float b, float c, float d, float mean, float sigma) {
      this.innovationId = innovationId;
      this.type = type;
      this.a = a;
      this.b = b;
      this.c = c;
      this.d = d;
      this.mean = mean;
      this.sigma = sigma;
    }

    public NeuronGene(NeuronGene other) {
      this.type = other.type;
      this.innovationId = other.innovationId;
      this.a = other.a;
      this.b = other.b;
      this.c = other.c;
      this.d = other.d;
      this.mean = other.mean;
      this.sigma = other.sigma;
    }

    public override string ToString() {
      return string.Format("<NeuronGene:{0}>", innovationId);
    }

    public IDictionary<string, object> ToJSON() {
      return new Dictionary<string, object>(){
        { "innovation", innovationId },
        { "type", (int)type },
        { "mean", mean },
        { "sigma", sigma },
        { "a", a },
        { "b", b },
        { "c", c },
        { "d", d },
      };
    }

    public static NeuronGene FromJSON(object obj) {
      var data = (Dictionary<string, object>)obj;
      var innovationId = (int)(long)data["innovation"];
      var type = (NeuronType)(int)(long)data["type"];
      var a = System.Convert.ToSingle(data["a"]);
      var b = System.Convert.ToSingle(data["b"]);
      var c = System.Convert.ToSingle(data["c"]);
      var d = System.Convert.ToSingle(data["d"]);
      var mean = System.Convert.ToSingle(data["mean"]);
      var sigma = System.Convert.ToSingle(data["sigma"]);
      return new NeuronGene(innovationId, type, a, b, c, d, mean, sigma);
    }
  }
}

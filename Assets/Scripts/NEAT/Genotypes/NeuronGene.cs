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

    public static NeuronGene FromPrototype(NeuronGene other) {
      return NeuronGene.Random(other.InnovationId);
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

    public NeuronGene(NeuronGene other) {
      this.innovationId = other.innovationId;
      this.a = other.a;
      this.b = other.b;
      this.c = other.c;
      this.d = other.d;
    }

    public IDictionary<string, object> ToJSON() {
      return new Dictionary<string, object>(){
        { "innovation", innovationId },
        { "a", a },
        { "b", b },
        { "c", c },
        { "d", d },
      };
    }

    public static NeuronGene FromJSON(object obj) {
      var data = (Dictionary<string, object>)obj;
      var innovationId = (int)(long)data["innovation"];
      var a = (float)(double)data["a"];
      var b = (float)(double)data["b"];
      var c = (float)(double)data["c"];
      var d = (float)(double)data["d"];
      return new NeuronGene(innovationId, a, b, c, d);
    }
  }
}
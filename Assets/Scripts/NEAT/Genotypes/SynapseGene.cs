using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
      return new SynapseGene(innovationId, fromNeuronId, toNeuronId, isEnabled,
        UnityEngine.Random.value);
    }

    public static SynapseGene FromPrototype(SynapseGene other) {
      return SynapseGene.Random(other.innovationId,
        other.fromNeuronId,
        other.toNeuronId,
        other.isEnabled);
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

    public SynapseGene(SynapseGene other) {
      this.innovationId = other.innovationId;
      this.fromNeuronId = other.fromNeuronId;
      this.toNeuronId = other.toNeuronId;
      this.isEnabled = other.isEnabled;
      this.weight = other.weight;
    }

    public override string ToString() {
      return string.Format("<SynapseGene:{0}:{1}:{2}>",
        innovationId, fromNeuronId, toNeuronId);
    }

    public IDictionary<string, object> ToJSON() {
      return new Dictionary<string, object>(){
        { "innovation", innovationId },
        { "weight", weight },
        { "from", fromNeuronId },
        { "to", toNeuronId },
        { "enabled", isEnabled },
      };
    }

    public static SynapseGene FromJSON(object obj) {
      var data = (Dictionary<string, object>)obj;
      var innovationId = (int)(long)data["innovation"];
      var fromNeuronId = (int)(long)data["from"];
      var toNeuronId = (int)(long)data["to"];
      var weight = System.Convert.ToSingle(data["weight"]);
      var isEnabled = (bool)data["enabled"];
      return new SynapseGene(innovationId, fromNeuronId, toNeuronId, isEnabled, weight);
    }
  }
}

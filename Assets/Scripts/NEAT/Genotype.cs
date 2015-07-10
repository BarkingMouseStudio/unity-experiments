using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Genotype {

    public List<NeuronGene> neuronGenes;
    public List<SynapseGene> synapseGenes;

    public Genotype(List<NeuronGene> neuronGenes, List<SynapseGene> synapseGenes) {
      this.neuronGenes = neuronGenes;
      this.synapseGenes = synapseGenes;
    }

    public Genotype Clone() {
      return new Genotype(
        new List<NeuronGene>(neuronGenes.Select(g => g.Clone())),
        new List<SynapseGene>(synapseGenes.Select(g => g.Clone()))
      );
    }

    public void Randomize(float toggleProbability) {
      foreach (var neuronGene in neuronGenes) {
        neuronGene.Randomize();
      }
      foreach (var synapseGene in synapseGenes) {
        synapseGene.Randomize(toggleProbability);
      }
    }

    public bool ContainsInnovation(Innovation innov) {
      switch (innov.type) {
        case InnovationType.Neuron:
          return neuronGenes.Any(g => g.InnovationId == innov.innovationId);
        case InnovationType.Synapse:
          return synapseGenes.Any(g => g.InnovationId == innov.innovationId);
        default:
          return false;
      }
    }

    public string ToJSON() {
      var neurons = neuronGenes.Select(g => {
        var dict = new Dictionary<string, object>();
        dict["innovation"] = g.InnovationId;
        dict["a"] = g.a;
        dict["b"] = g.b;
        dict["c"] = g.c;
        dict["d"] = g.d;
        return dict;
      }).ToList();

      var synapses = synapseGenes.Select(g => {
        var dict = new Dictionary<string, object>();
        dict["innovation"] = g.InnovationId;
        dict["weight"] = g.weight;
        dict["from"] = g.fromNeuronId;
        dict["to"] = g.toNeuronId;
        dict["enabled"] = g.isEnabled;
        return dict;
      }).ToList();

      var obj = new Dictionary<string, List<Dictionary<string, object>>>();
      obj["neurons"] = neurons;
      obj["synapses"] = synapses;

      return JSON.Serialize(obj);
    }

    public static Genotype FromJSON(string data) {
      var obj = (Dictionary<string, object>)JSON.Deserialize(data);

      var neuronGenes = ((List<object>)obj["neurons"]).Select(g => {
        var neuronGene = (Dictionary<string, object>)g;
        var innovationId = (int)(long)neuronGene["innovation"];
        var a = (float)(double)neuronGene["a"];
        var b = (float)(double)neuronGene["b"];
        var c = (float)(double)neuronGene["c"];
        var d = (float)(double)neuronGene["d"];
        return new NeuronGene(innovationId, a, b, c, d);
      }).ToList();

      var synapseGenes = ((List<object>)obj["synapses"]).Select(g => {
        var synapseGene = (Dictionary<string, object>)g;
        var innovationId = (int)(long)synapseGene["innovation"];
        var fromNeuronId = (int)(long)synapseGene["from"];
        var toNeuronId = (int)(long)synapseGene["to"];
        var weight = (float)System.Convert.ToDouble(synapseGene["weight"]);
        var isEnabled = (bool)synapseGene["enabled"];
        return new SynapseGene(innovationId, fromNeuronId, toNeuronId, isEnabled, (float)weight);
      }).ToList();

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

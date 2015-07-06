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
        new List<NeuronGene>(neuronGenes),
        new List<SynapseGene>(synapseGenes)
      );
    }

    public Genotype Randomize() {
      return new Genotype(
        neuronGenes.Select(g => g.Randomize()).ToList(),
        synapseGenes.Select(g => g.Randomize()).ToList()
      );
    }

    public string ToJSON() {
      var neurons = neuronGenes.Select(g => {
        var dict = new Dictionary<string, object>();
        dict["innovation"] = g.InnovationId;
        dict["id"] = g.id;
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
        var id = (int)(long)neuronGene["id"];
        var a = (float)(double)neuronGene["a"];
        var b = (float)(double)neuronGene["b"];
        var c = (float)(double)neuronGene["c"];
        var d = (float)(double)neuronGene["d"];
        return new NeuronGene(innovationId, id, a, b, c, d);
      }).ToList();

      var synapseGenes = ((List<object>)obj["synapses"]).Select(g => {
        var synapseGene = (Dictionary<string, object>)g;
        var innovationId = (int)(long)synapseGene["innovation"];
        var fromNeuronId = (int)(long)synapseGene["from"];
        var toNeuronId = (int)(long)synapseGene["to"];
        var weight = (float)(double)synapseGene["weight"];
        var isEnabled = (bool)synapseGene["enabled"];
        return new SynapseGene(innovationId, fromNeuronId, toNeuronId, isEnabled, weight);
      }).ToList();

      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Genotype {

    private NeuronGene[] neuronGenes;
    private SynapseGene[] synapseGenes;

    public int NeuronCount {
      get {
        return neuronGenes.Length;
      }
    }

    public int SynapseCount {
      get {
        return synapseGenes.Length;
      }
    }

    public NeuronGene[] NeuronGenes {
      get {
        return neuronGenes;
      }
      set {
        neuronGenes = value;
      }
    }

    public SynapseGene[] SynapseGenes {
      get {
        return synapseGenes;
      }
      set {
        synapseGenes = value;
      }
    }

    public static Genotype FromPrototype(Genotype protoGenotype, InnovationCollection innovations) {
      var neuronGenes = protoGenotype.NeuronGenes
        .Select(g => NeuronGene.FromPrototype(g))
        .ToArray();
      var synapseGenes = protoGenotype.SynapseGenes
        .Select(g => SynapseGene.FromPrototype(g))
        .ToArray();
      return new Genotype(neuronGenes, synapseGenes);
    }

    public Genotype(NeuronGene[] neuronGenes, SynapseGene[] synapseGenes) {
      this.neuronGenes = neuronGenes;
      this.synapseGenes = synapseGenes;
    }

    public Genotype(Genotype other) {
      this.neuronGenes = other.NeuronGenes
        .Select(g => new NeuronGene(g))
        .ToArray();
      this.synapseGenes = other.SynapseGenes
        .Select(g => new SynapseGene(g))
        .ToArray();
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

    public IDictionary<string, object> ToJSON() {
      return new Dictionary<string, object>(){
        { "neurons", neuronGenes.Select(g => g.ToJSON()).ToList() },
        { "synapses", synapseGenes.Select(g => g.ToJSON()).ToList() },
      };
    }

    public static Genotype FromJSON(object obj) {
      var data = (Dictionary<string, object>)obj;
      var neuronGenes = ((List<object>)data["neurons"]).Select(g =>
        NeuronGene.FromJSON(g)).ToArray();
      var synapseGenes = ((List<object>)data["synapses"]).Select(g =>
        SynapseGene.FromJSON(g)).ToArray();
      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

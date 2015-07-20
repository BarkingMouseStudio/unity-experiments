using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Genotype {

    private GeneList<NeuronGene> neuronGenes;
    private GeneList<SynapseGene> synapseGenes;

    public int NeuronCount {
      get {
        return neuronGenes.Count;
      }
    }

    public int SynapseCount {
      get {
        return synapseGenes.Count;
      }
    }

    public GeneList<NeuronGene> NeuronGenes {
      get {
        return neuronGenes;
      }
      set {
        neuronGenes = value;
      }
    }

    public GeneList<SynapseGene> SynapseGenes {
      get {
        return synapseGenes;
      }
      set {
        synapseGenes = value;
      }
    }

    public int Complexity {
      get {
        return neuronGenes.Count + synapseGenes.Count;
      }
    }

    public static Genotype FromPrototype(Genotype protoGenotype) {
      var neuronGenes = protoGenotype.NeuronGenes
        .Select(g => NeuronGene.FromPrototype(g))
        .ToGeneList();
      var synapseGenes = protoGenotype.SynapseGenes
        .Select(g => SynapseGene.FromPrototype(g))
        .ToGeneList();
      return new Genotype(neuronGenes, synapseGenes);
    }

    public Genotype(GeneList<NeuronGene> neuronGenes, GeneList<SynapseGene> synapseGenes) {
      this.neuronGenes = neuronGenes;
      this.synapseGenes = synapseGenes;
    }

    public Genotype(Genotype other) {
      this.neuronGenes = other.NeuronGenes
        .Select(g => new NeuronGene(g))
        .ToGeneList();
      this.synapseGenes = other.SynapseGenes
        .Select(g => new SynapseGene(g))
        .ToGeneList();
    }

    public bool ContainsInnovation(Innovation innov) {
      switch (innov.type) {
        case InnovationType.Neuron:
          return neuronGenes.Contains(innov.innovationId);
        case InnovationType.Synapse:
          return synapseGenes.Contains(innov.innovationId);
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
      var neuronGenes = ((List<object>)data["neurons"])
        .Select(g => NeuronGene.FromJSON(g))
        .ToGeneList();
      var synapseGenes = ((List<object>)data["synapses"])
        .Select(g => SynapseGene.FromJSON(g))
        .ToGeneList();
      return new Genotype(neuronGenes, synapseGenes);
    }
  }
}

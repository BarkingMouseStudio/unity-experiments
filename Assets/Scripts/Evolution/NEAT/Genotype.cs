using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Genotype {

    // Distance coefficients
    const float c1 = 2.0f;
    const float c2 = 2.0f;
    const float c3 = 3.0f;

    public List<NeuronGene> neuronGenes;
    public List<SynapseGene> synapseGenes;

    public Genotype(int minimalNeuronCount) {
      // The starting genotype neurons use their index as the innovation id
      this.neuronGenes = Enumerable.Range(0, minimalNeuronCount)
        .Select(i => new NeuronGene(i))
        .ToList();
      this.synapseGenes = new List<SynapseGene>();
    }

    public Genotype(List<NeuronGene> neuronGenes, List<SynapseGene> synapseGenes) {
      this.neuronGenes = neuronGenes;
      this.synapseGenes = synapseGenes;
    }

    // Clone
    public Genotype(Genotype other) {
      this.neuronGenes = new List<NeuronGene>(other.neuronGenes);
      this.synapseGenes = new List<SynapseGene>(other.synapseGenes);
    }

    // new GeneticMeasurement(c1, c2, c3): GeneticMeasurement.Measure(a, b)
    public static float Distance(Genotype a, Genotype b) {
      var E = 0.0f;
      var D = 0.0f;
      var W = 0.0f;
      var N = 0.0f;

      var L = 0.0f;
      L += Mathf.Max(a.neuronGenes.Count, b.neuronGenes.Count);
      L += Mathf.Max(a.synapseGenes.Count, b.synapseGenes.Count);
      if (L == 0.0f) { // Don't divide by zero
        L = 1.0f;
      }

      foreach (var t in Iterator.IterGenes(a.neuronGenes, b.neuronGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            N += Mathf.Abs(a.neuronGenes[t.Second].a - b.neuronGenes[t.Third].a) / 2;
            N += Mathf.Abs(a.neuronGenes[t.Second].b - b.neuronGenes[t.Third].b) / 2;
            N += Mathf.Abs(a.neuronGenes[t.Second].c - b.neuronGenes[t.Third].c) / 2;
            N += Mathf.Abs(a.neuronGenes[t.Second].d - b.neuronGenes[t.Third].d) / 2;
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.DisjointB:
            D++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            E++;
            break;
        }
      }

      foreach (var t in Iterator.IterGenes(a.synapseGenes, b.synapseGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            W += Mathf.Abs(a.synapseGenes[t.Second].weight - b.synapseGenes[t.Third].weight) / 2;
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.DisjointB:
            D++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            E++;
            break;
        }
      }

      return ((c1 * E) / L) + ((c2 * D) / L) + (c3 * (W + N));
    }
  }
}

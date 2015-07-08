using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class MultipointCrossover : ICrossover {

    static List<T> CrossoverGenes<T>(List<T> a, List<T> b, float fitnessA, float fitnessB) where T : IHistoricalGene {
      var newGenes = new List<T>(System.Math.Max(a.Count, b.Count));
      foreach (var t in new GeneEnumerable<T>(a, b)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned: // Pick random aligned gene
            newGenes.Add(Random.value < 0.5f ? a[t.Second] : b[t.Third]);
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.ExcessA:
            if (fitnessA >= fitnessB) newGenes.Add(a[t.Second]);
            break;
          case HistoricalGeneTypes.DisjointB:
          case HistoricalGeneTypes.ExcessB:
            if (fitnessB >= fitnessA) newGenes.Add(b[t.Third]);
            break;
        }
      }
      return newGenes;
    }

    public Genotype Crossover(Phenotype a, Phenotype b) {
      var newNeuronGenes = CrossoverGenes<NeuronGene>(a.genotype.neuronGenes, b.genotype.neuronGenes, a.adjustedFitness, b.adjustedFitness);
      var newSynapseGenes = CrossoverGenes<SynapseGene>(a.genotype.synapseGenes, b.genotype.synapseGenes, a.adjustedFitness, b.adjustedFitness);
      return new Genotype(newNeuronGenes, newSynapseGenes);
    }
  }
}

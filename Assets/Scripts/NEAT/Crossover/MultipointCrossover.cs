using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class MultipointCrossover : ICrossover {

    static GeneList<T> CrossoverGenes<T>(GeneList<T> a, GeneList<T> b, float fitnessA, float fitnessB) where T : IHistoricalGene {
      var newGenes = new GeneList<T>(System.Math.Max(a.Count, b.Count));

      foreach (var t in new GeneEnumerable<T>(a, b)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned: // Pick random aligned gene
            newGenes.Add(Random.value >= 0.5f ? a[t.Second] : b[t.Third]);
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
      if (a == b) {
        return new Genotype(a.Genotype);
      }

      var aGenotype = new Genotype(a.Genotype);
      var bGenotype = new Genotype(b.Genotype);
      var newNeuronGenes = CrossoverGenes<NeuronGene>(aGenotype.NeuronGenes, bGenotype.NeuronGenes, a.AdjustedFitness, b.AdjustedFitness);
      var newSynapseGenes = CrossoverGenes<SynapseGene>(aGenotype.SynapseGenes, bGenotype.SynapseGenes, a.AdjustedFitness, b.AdjustedFitness);
      return new Genotype(newNeuronGenes, newSynapseGenes);
    }
  }
}

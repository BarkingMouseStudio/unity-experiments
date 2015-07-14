using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class MultipointCrossover : ICrossover {

    static T[] CrossoverGenes<T>(T[] a, T[] b, float fitnessA, float fitnessB) where T : IHistoricalGene {
      var newGenes = new List<T>(System.Math.Max(a.Length, b.Length));

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

      var sorted = IsSorted(newGenes);
      Assert.IsTrue(sorted, "Gene innovations must occur in ascending order");
      if (!sorted) {
        Debug.LogFormat("a: {0}", a.Stringify());
        Debug.LogFormat("b: {0}", b.Stringify());
        Debug.LogFormat("offspring: {0}", newGenes.Stringify());
      }

      return newGenes.ToArray();
    }

    static bool IsSorted<T>(List<T> genes) where T : IHistoricalGene {
      int count = genes.Count;
      if (count == 0) {
        return true;
      }

      int prev = genes[0].InnovationId;
      for (int i = 1; i < count; i++) {
        if (genes[i].InnovationId <= prev) {
          return false;
        }
      }

      return true;
    }

    public Genotype Crossover(Phenotype a, Phenotype b) {
      var aGenotype = new Genotype(a.Genotype);
      var bGenotype = new Genotype(b.Genotype);
      var newNeuronGenes = CrossoverGenes<NeuronGene>(aGenotype.NeuronGenes, bGenotype.NeuronGenes, a.AdjustedFitness, b.AdjustedFitness);
      var newSynapseGenes = CrossoverGenes<SynapseGene>(aGenotype.SynapseGenes, bGenotype.SynapseGenes, a.AdjustedFitness, b.AdjustedFitness);
      return new Genotype(newNeuronGenes, newSynapseGenes);
    }
  }
}

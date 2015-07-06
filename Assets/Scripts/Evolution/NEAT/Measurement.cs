using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Measurement {

    // Measurement coefficients
    readonly float c1;
    readonly float c2;
    readonly float c3;

    public Measurement(float c1, float c2, float c3) {
      this.c1 = c1;
      this.c2 = c2;
      this.c3 = c3;
    }

    public float Distance(Genotype a, Genotype b) {
      var Nc = (float)Mathf.Max(a.neuronGenes.Count, b.neuronGenes.Count);
      if (Nc == 0.0f) { // Don't divide by zero
        Nc = 1.0f;
      }

      var N = 0.0f;
      var Ne = 0.0f;
      var Nd = 0.0f;
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
            Nd++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            Ne++;
            break;
        }
      }

      var Sc = (float)Mathf.Max(a.synapseGenes.Count, b.synapseGenes.Count);
      if (Sc == 0.0f) { // Don't divide by zero
        Sc = 1.0f;
      }

      var S = 0.0f;
      var Se = 0.0f;
      var Sd = 0.0f;
      foreach (var t in Iterator.IterGenes(a.synapseGenes, b.synapseGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            S += Mathf.Abs(a.synapseGenes[t.Second].weight - b.synapseGenes[t.Third].weight) / 2;
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.DisjointB:
            Sd++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            Se++;
            break;
        }
      }

      return ((c1 * Ne) / Nc) + ((c2 * Nd) / Nc) + (c3 * N) +
             ((c1 * Se) / Sc) + ((c2 * Sd) / Sc) + (c3 * S);
    }
  }
}

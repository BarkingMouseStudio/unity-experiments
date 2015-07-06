using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Measurement {

    // Measurement coefficients
    readonly float excessCoeff;
    readonly float disjointCoeff;
    readonly float mutationCoeff;

    public Measurement(float excessCoeff, float disjointCoeff, float mutationCoeff) {
      this.excessCoeff = excessCoeff;
      this.disjointCoeff = disjointCoeff;
      this.mutationCoeff = mutationCoeff;
    }

    public float Distance(Genotype a, Genotype b) {
      var neuronCount = (float)Mathf.Max(a.neuronGenes.Count, b.neuronGenes.Count);
      if (neuronCount == 0.0f) { // Don't divide by zero
        neuronCount = 1.0f;
      }

      var neuronDiff = 0.0f;
      var neuronExcess = 0.0f;
      var neuronDisjoint = 0.0f;
      foreach (var t in Iterator.IterGenes(a.neuronGenes, b.neuronGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            neuronDiff += Mathf.Abs(a.neuronGenes[t.Second].a - b.neuronGenes[t.Third].a) / 2;
            neuronDiff += Mathf.Abs(a.neuronGenes[t.Second].b - b.neuronGenes[t.Third].b) / 2;
            neuronDiff += Mathf.Abs(a.neuronGenes[t.Second].c - b.neuronGenes[t.Third].c) / 2;
            neuronDiff += Mathf.Abs(a.neuronGenes[t.Second].d - b.neuronGenes[t.Third].d) / 2;
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.DisjointB:
            neuronDisjoint++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            neuronExcess++;
            break;
        }
      }

      var synapseCount = (float)Mathf.Max(a.synapseGenes.Count, b.synapseGenes.Count);
      if (synapseCount == 0.0f) { // Don't divide by zero
        synapseCount = 1.0f;
      }

      var synapseDiff = 0.0f;
      var synapseExcess = 0.0f;
      var synapseDisjoint = 0.0f;
      foreach (var t in Iterator.IterGenes(a.synapseGenes, b.synapseGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            synapseDiff += Mathf.Abs(a.synapseGenes[t.Second].weight - b.synapseGenes[t.Third].weight) / 2;
            break;
          case HistoricalGeneTypes.DisjointA:
          case HistoricalGeneTypes.DisjointB:
            synapseDisjoint++;
            break;
          case HistoricalGeneTypes.ExcessA:
          case HistoricalGeneTypes.ExcessB:
            synapseExcess++;
            break;
        }
      }

      return ((excessCoeff * neuronExcess) / neuronCount) +
             ((excessCoeff * synapseExcess) / synapseCount) +
             ((disjointCoeff * neuronDisjoint) / neuronCount) +
             ((disjointCoeff * synapseDisjoint) / synapseCount) +
             (mutationCoeff * neuronDiff) +
             (mutationCoeff * synapseDiff);
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class DistanceMetric : IDistanceMetric {

    // DistanceMetric coefficients
    readonly float disjointCoeff;
    readonly float excessCoeff;
    readonly float mutationCoeff;

    public DistanceMetric() : this(1.0f, 1.0f, 1.0f) {}

    public DistanceMetric(float disjointCoeff, float excessCoeff, float mutationCoeff) {
      this.disjointCoeff = disjointCoeff;
      this.excessCoeff = excessCoeff;
      this.mutationCoeff = mutationCoeff;
    }

    public bool MeasureDistance(Genotype a, Genotype b, float threshold) {
      return MeasureDistance(a, b) <= threshold;
    }

    public float MeasureDistance(Genotype a, Genotype b) {
      var neuronCount = Mathf.Max(a.NeuronCount, b.NeuronCount);
      if (neuronCount == 0) { // Don't divide by zero
        neuronCount = 1;
      }

      var neuronDiff = 0.0f;
      var neuronExcess = 0.0f;
      var neuronDisjoint = 0.0f;
      foreach (var t in new GeneEnumerable<NeuronGene>(a.NeuronGenes, b.NeuronGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            neuronDiff += Mathf.Abs(a.NeuronGenes[t.Second].a - b.NeuronGenes[t.Third].a);
            neuronDiff += Mathf.Abs(a.NeuronGenes[t.Second].b - b.NeuronGenes[t.Third].b);
            neuronDiff += Mathf.Abs(a.NeuronGenes[t.Second].c - b.NeuronGenes[t.Third].c);
            neuronDiff += Mathf.Abs(a.NeuronGenes[t.Second].d - b.NeuronGenes[t.Third].d);
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

      var synapseCount = Mathf.Max(a.SynapseCount, b.SynapseCount);
      if (synapseCount == 0) { // Don't divide by zero
        synapseCount = 1;
      }

      var synapseDiff = 0.0f;
      var synapseExcess = 0.0f;
      var synapseDisjoint = 0.0f;
      foreach (var t in new GeneEnumerable<SynapseGene>(a.SynapseGenes, b.SynapseGenes)) {
        switch (t.First) {
          case HistoricalGeneTypes.Aligned:
            synapseDiff += Mathf.Abs(a.SynapseGenes[t.Second].weight - b.SynapseGenes[t.Third].weight);
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

      neuronDiff /= (float)neuronCount;
      synapseDiff /= (float)synapseCount;

      return (excessCoeff * neuronExcess) +
             (excessCoeff * synapseExcess) +
             (disjointCoeff * neuronDisjoint) +
             (disjointCoeff * synapseDisjoint) +
             (mutationCoeff * neuronDiff) +
             (mutationCoeff * synapseDiff);
    }
  }
}

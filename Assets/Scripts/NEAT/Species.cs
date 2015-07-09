using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Species {

    public int speciesId;
    public Genotype representative;

    public List<Phenotype> phenotypes;
    public List<Phenotype> elites;

    public int Size {
      get {
        return phenotypes.Count;
      }
    }

    public float AverageFitness {
      get {
        return phenotypes.Aggregate(0.0f,
          (total, pt) => total + pt.adjustedFitness,
          (total) => total / phenotypes.Count);
      }
    }

    public Species(int speciesId, Genotype representative) {
      this.speciesId = speciesId;
      this.representative = representative;
      this.phenotypes = new List<Phenotype>();
    }

    public float Distance(Phenotype candidate, Measurement measurement) {
      return measurement.Distance(representative, candidate.genotype);
    }

    public void AdjustFitness(int populationSize) {
      var multiplier = 1.0f + ((float)phenotypes.Count / (float)populationSize);
      foreach (var pt in phenotypes) {
        pt.adjustedFitness = pt.fitness * multiplier;
      }
    }

    public void Sort() {
      phenotypes.Sort((a, b) => a.adjustedFitness.CompareTo(b.adjustedFitness));
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Species {

    public int speciesId;
    public Genotype representative;

    public List<Phenotype> phenotypes;

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

    public Species Next() {
      return new Species(speciesId, representative);
    }

    public float Distance(Phenotype candidate) {
      return Genotype.Distance(representative, candidate.genotype);
    }

    public float AdjustFitness() {
      var sumFitness = 0.0f;
      foreach (var genotype in phenotypes) {
        genotype.adjustedFitness = genotype.fitness / phenotypes.Count;
        sumFitness += genotype.adjustedFitness;
      }
      return sumFitness / phenotypes.Count;
    }
  }
}

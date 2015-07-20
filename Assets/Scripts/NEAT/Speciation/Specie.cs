using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Specie : List<Phenotype> {

    private readonly int speciesId;
    private readonly Genotype representative;

    public int SpeciesId {
      get {
        return speciesId;
      }
    }

    public float MeanComplexity {
      get {
        return this.Aggregate(0.0f,
          (sum, pt) => sum + (float)pt.Genotype.Complexity,
          (sum) => sum / (float)this.Count);
      }
    }

    public float BestFitness {
      get {
        return this.Min(pt => pt.Fitness);
      }
    }

    public float MeanFitness {
      get {
        return this.Aggregate(0.0f,
          (sum, pt) => sum + pt.Fitness,
          (sum) => sum / (float)this.Count);
      }
    }

    public float MeanAdjustedFitness {
      get {
        return this.Aggregate(0.0f,
          (sum, pt) => sum + pt.AdjustedFitness,
          (sum) => sum / (float)this.Count);
      }
    }

    public Genotype Representative {
      get {
        return representative;
      }
    }

    public Specie(int speciesId, Genotype representative) : base() {
      this.speciesId = speciesId;
      this.representative = representative;
    }
  }
}

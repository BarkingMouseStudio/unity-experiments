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

    public float MeanFitness {
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

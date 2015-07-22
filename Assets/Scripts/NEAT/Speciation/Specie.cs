using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Specie : List<Phenotype> {

    private readonly int speciesId;
    private readonly Genotype representative;
    private int age;
    private int bestAge;
    private float bestMeanAdjustedFitness;

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

    public int Age {
      get {
        return age;
      }
    }

    public int BestAge {
      get {
        return bestAge;
      }
    }

    public float BestMeanAdjustedFitness {
      get {
        return bestMeanAdjustedFitness;
      }
    }

    public Specie(int speciesId, Genotype representative, int age, int bestAge, float bestMeanAdjustedFitness) : base() {
      this.speciesId = speciesId;
      this.representative = representative;
      this.age = age;
      this.bestAge = bestAge;
      this.bestMeanAdjustedFitness = bestMeanAdjustedFitness;
    }

    public Genotype ProduceOffspring(ICrossover crossover) {
      // Tournament selection
      // (This should be equivalent or better than truncating lower percent.)
      var parent1 = this[Random.Range(0, this.Count)];
      var parent2 = this.Sample(2)
        .OrderByDescending(pt => pt.AdjustedFitness)
        .First();
      return crossover.Crossover(parent1, parent2);
    }
  }
}

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
    private float pastBestAdjustedFitness;

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

    public float BestAdjustedFitness {
      get {
        return this.Max(pt => pt.AdjustedFitness);
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

    public float PastBestAdjustedFitness {
      get {
        return pastBestAdjustedFitness;
      }
    }

    public Specie(int speciesId, Genotype representative, int age, int bestAge, float pastBestAdjustedFitness) : base() {
      this.speciesId = speciesId;
      this.representative = representative;
      this.age = age;
      this.bestAge = bestAge;
      this.pastBestAdjustedFitness = pastBestAdjustedFitness;
    }

    public Genotype ProduceOffspring(ICrossover crossover) {
      // Truncate lower 20%
      var candidates = this.OrderByDescending(pt => pt.AdjustedFitness)
        .Take(Mathf.CeilToInt(this.Count * 0.8f))
        .ToList();

      // Tournament selection
      var parent1 = candidates[Random.Range(0, candidates.Count)];
      var parent2 = candidates.Sample(2)
        .OrderByDescending(pt => pt.AdjustedFitness)
        .First();
      return crossover.Crossover(parent1, parent2);
    }
  }
}

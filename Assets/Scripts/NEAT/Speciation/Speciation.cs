using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Speciation {

    private int nextSpeciesId = 0;

    private readonly DistanceMetric distanceMetric;

    private readonly int desiredSpeciesCount;
    private readonly float distanceThresholdAdjustment;

    private float distanceThreshold;

    public float DistanceThreshold {
      get {
        return distanceThreshold;
      }
    }

    public Speciation(int desiredSpeciesCount, float distanceThreshold, float distanceThresholdAdjustment, DistanceMetric distanceMetric) {
      this.desiredSpeciesCount = desiredSpeciesCount;
      this.distanceThreshold = distanceThreshold;
      this.distanceThresholdAdjustment = distanceThresholdAdjustment;
      this.distanceMetric = distanceMetric;
    }

    public Specie[] Speciate(Specie[] species, Phenotype[] phenotypes) {
      // Begin new species
      var speciesListNext = species.Select(sp => {
        if (sp.MeanAdjustedFitness >= sp.BestMeanAdjustedFitness) {
          return new Specie(sp.SpeciesId, sp.Representative, sp.Age + 1,
            sp.Age, sp.MeanAdjustedFitness);
        } else {
          return new Specie(sp.SpeciesId, sp.Representative, sp.Age + 1,
            sp.BestAge, sp.BestMeanAdjustedFitness);
        }
      }).ToList();

      // Place genotypes
      foreach (var phenotype in phenotypes) {
        bool foundSpecies = false;

        foreach (var specie in speciesListNext) {
          // Check if the phenotype belong to this species
          var distance = distanceMetric.MeasureDistance(phenotype.Genotype, specie.Representative);
          if (distance <= distanceThreshold) {
            specie.Add(phenotype);
            foundSpecies = true;
            break;
          }
        }

        // Create a new species for the phenotype if necessary
        if (!foundSpecies) {
          var spNext = new Specie(nextSpeciesId, phenotype.Genotype, 0, 0, 0.0f);
          nextSpeciesId++;

          spNext.Add(phenotype);
          speciesListNext.Add(spNext);
        }
      }

      // Adjust threshold to accomodate the correct number of species
      if (speciesListNext.Count < desiredSpeciesCount) { // Too few
        distanceThreshold -= distanceThresholdAdjustment; // Decrease threshold
      } else if (speciesListNext.Count > desiredSpeciesCount) { // To many
        distanceThreshold += distanceThresholdAdjustment; // Increase threshold
      }

      // Prune empty species
      var dead = speciesListNext.Where(sp => sp.Count == 0);
      var speciesNext = speciesListNext.Except(dead);

      // Adjust each species' phenotypes' fitness
      foreach (var specie in speciesNext) {
        foreach (var phenotype in specie) {
          var adjustedFitness = phenotype.Fitness;

          var stagnation = specie.Age - specie.BestAge;
          if (stagnation >= 10 && stagnation < 100) {
            // age: 10, penalty = 0.9x
            // age: 25, penalty = 0.75x
            // age: 50, penalty = 0.5x
            // age: 75, penalty = 0.25x
            // age: 90, penalty = 0.1x
            adjustedFitness *= 1.0f - (stagnation / 100.0f); // Up to 100% penalty
          }

	        if (specie.Age < 10) {
            // age: 0, reward = 2x
            // age: 2, reward = 1.8x
            // age: 5, reward = 1.5x
            // age: 8, reward = 1.2x
            // age: 10, reward = 1x
            adjustedFitness *= 1.0f + (1.0f - (specie.Age / 10.0f)); // Up to 100% bonus
          }

          adjustedFitness /= (float)specie.Count;

          phenotype.AdjustedFitness = adjustedFitness;
        }
      }

      return speciesNext.OrderByDescending(sp => sp.MeanAdjustedFitness)
        .ToArray();
    }
  }
}

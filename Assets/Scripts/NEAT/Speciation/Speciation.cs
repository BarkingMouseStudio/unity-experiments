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
      var speciesListNext = species.Select(sp =>
        new Specie(sp.SpeciesId, sp.Representative)).ToList();

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
          var spNext = new Specie(nextSpeciesId, phenotype.Genotype);
          nextSpeciesId++;

          spNext.Add(phenotype);
          speciesListNext.Add(spNext);
        }
      }

      // Adjust threshold to accomodate the correct number of species
      if (speciesListNext.Count < desiredSpeciesCount) { // Too few
        distanceThreshold -= distanceThreshold * distanceThresholdAdjustment; // Decrease threshold
      } else if (speciesListNext.Count > desiredSpeciesCount) { // To many
        distanceThreshold += distanceThreshold * distanceThresholdAdjustment; // Increase threshold
      }

      // Prune empty species
      var dead = speciesListNext.Where(sp => sp.Count == 0);
      var speciesNext = speciesListNext.Except(dead);

      // Adjust each species' phenotypes' fitness
      foreach (var specie in speciesNext) {
        var multiplier = 1.0f + ((float)specie.Count / (float)phenotypes.Length);
        Assert.IsTrue(multiplier >= 1.0f,
          "Must penalize phenotypes of large species");

        foreach (var phenotype in specie) {
          phenotype.AdjustedFitness = phenotype.Fitness * multiplier;
          Assert.IsTrue(phenotype.AdjustedFitness >= phenotype.Fitness,
            string.Format("Must penalize phenotypes of large species.\n" +
            "Adjusted Fitness: {0}, Fitness: {1}, Species Size: {2}, Multiplier: {3}",
            phenotype.AdjustedFitness, phenotype.Fitness, specie.Count, multiplier));
        }
      }

      return speciesNext.OrderBy(sp => sp.MeanFitness).ToArray();
    }
  }
}

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
          if (distanceMetric.MeasureDistance(phenotype.Genotype, specie.Representative, distanceThreshold)) {
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
      var specieNext = speciesListNext.Except(dead).ToArray();

      // Adjust each species' phenotypes' fitness
      foreach (var specie in specieNext) {
        foreach (var phenotype in specie) {
          phenotype.AdjustedFitness = phenotype.Fitness / specie.Count;
        }
      }

      return specieNext;
    }
  }
}

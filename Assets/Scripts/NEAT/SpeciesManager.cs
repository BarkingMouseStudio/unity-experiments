using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class SpeciesManager {

    List<Species> spp = new List<Species>();
    int nextSpeciesId = 0;

    int desiredSpeciesCount;
    float distanceThreshold;
    float distanceThresholdAdjustment;

    public List<Species> Species {
      get {
        return spp;
      }
    }

    public float TotalAverageFitness {
      get {
        return spp.Aggregate(0.0f,
          (total, sp) => total + sp.AverageFitness);
      }
    }

    public SpeciesManager(int desiredSpeciesCount, float distanceThreshold, float distanceThresholdAdjustment) {
      this.desiredSpeciesCount = desiredSpeciesCount;
      this.distanceThreshold = distanceThreshold;
      this.distanceThresholdAdjustment = distanceThresholdAdjustment;
    }

    public void AdjustFitness(int populationSize) {
      foreach (var sp in spp) {
        sp.AdjustFitness(populationSize);
      }
    }

    public void Sort() {
      foreach (var sp in spp) {
        sp.Sort();
      }
      spp.Sort((a, b) => a.AverageFitness.CompareTo(b.AverageFitness));
    }

    // Update existing species or create a new species using the original, if any
    public void Speciate(List<Phenotype> phenotypes, Measurement measurement) {
      // Begin new species
      var sppNext = spp.Select(sp => new Species(sp.speciesId, sp.representative)).ToList();

      // Place genotypes
      foreach (var phenotype in phenotypes) {
        bool foundSpecies = false;

        foreach (var sp in sppNext) {
          var distance = sp.Distance(phenotype, measurement);
          if (distance < distanceThreshold) { // Does the phenotype belong to this species
            sp.phenotypes.Add(phenotype);
            foundSpecies = true;
            break;
          }
        }

        // Create a new species for the phenotype if necessary
        if (!foundSpecies) {
          var spNext = new Species(nextSpeciesId, phenotype.genotype);
          nextSpeciesId++;

          spNext.phenotypes.Add(phenotype);
          sppNext.Add(spNext);
        }
      }

      if (spp.Count < desiredSpeciesCount) { // Too few
        distanceThreshold -= distanceThreshold * distanceThresholdAdjustment; // Decrease threshold
      } else if (spp.Count > desiredSpeciesCount) { // To many
        distanceThreshold += distanceThreshold * distanceThresholdAdjustment; // Increase threshold
      }

      Debug.LogFormat("Species Count: {0}, Distance Threshold: {1}",
        sppNext.Count, distanceThreshold);

      // Prune empty species
      var dead = sppNext.Where(sp => sp.phenotypes.Count == 0);
      this.spp = sppNext.Except(dead).ToList();
    }
  }
}

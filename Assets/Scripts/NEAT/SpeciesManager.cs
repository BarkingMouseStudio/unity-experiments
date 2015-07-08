using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class SpeciesManager {

    public List<Species> spp = new List<Species>();
    int nextSpeciesId = 0;

    public int desiredSpeciesCount;
    public float distanceThreshold;
    public float distanceThresholdAdjustment;

    // Update existing species or create a new species using the original, if any
    public List<Species> Speciate(List<Species> spp, List<Phenotype> phenotypes, Measurement measurement) {
      // Begin new species
      var sppNext = spp.Select(sp => sp.Next()).ToList();

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

      // Prune empty species
      var dead = sppNext.Where(sp => sp.phenotypes.Count == 0);
      return sppNext.Except(dead).ToList();
    }
  }
}

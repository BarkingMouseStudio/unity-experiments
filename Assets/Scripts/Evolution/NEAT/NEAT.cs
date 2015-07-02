using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class NEAT {

    const int populationSize = 100;

    public Population population;
    public List<Species> spp;
    public int generation;

    int nextSpeciesId = 0;

    public int desiredSpeciesCount = 10;
    public float distanceThreshold = 0.3f;
    public float distanceThresholdAdjustment = 0.1f;

    // Update existing species or create a new species using the original, if any
    public List<Species> Speciate(List<Species> spp, List<Phenotype> phenotypes) {
      // Begin new species
      var sppNext = spp.Select(sp => sp.Next()).ToList();

      // Place genotypes
      foreach (var phenotype in phenotypes) {
        bool foundSpecies = false;

        foreach (var sp in sppNext) {
          if (sp.Distance(phenotype) < distanceThreshold) { // Does the phenotype belong to this species
            sp.phenotypes.Add(phenotype);
            foundSpecies = true;
            break;
          }
        }

        // Create a new species for the phenotype if necessary
        if (!foundSpecies) {
          sppNext.Add(new Species(nextSpeciesId++, phenotype.genotype));
        }
      }

      // Prune empty species
      var dead = sppNext.Where(sp => sp.phenotypes.Count == 0);
      return sppNext.Except(dead).ToList();
    }

    public NEAT() {
      // 1. Create a random minimal population
      population = new Population(populationSize);
    }

    public List<Genotype> Sample() {
      return population.genotypes;
    }

    // Evaluate population
    public void Update(List<Phenotype> phenotypes) {
      // Update species and distance threshold
      spp = Speciate(spp, phenotypes);

      if (spp.Count < desiredSpeciesCount) { // Too few
        distanceThreshold += distanceThresholdAdjustment; // Increase threshold
      } else if (spp.Count > desiredSpeciesCount) { // To many
        distanceThreshold -= distanceThresholdAdjustment; // Decrease threshold
      }

      // Adjust fitnesses (do this inside a species when it's created from phenotypes)
      var totalFitness = spp.Aggregate(0.0f, (total, sp) => total + sp.AdjustFitness());

      this.population = spp.Aggregate(new List<Genotype>(populationSize), (genotypes, sp) => {
        var offspringCount = Mathf.FloorToInt(
          (sp.AverageFitness / totalFitness) * (float)populationSize
        );
        genotypes.AddRange(population.Reproduce(sp.phenotypes, offspringCount));
        return genotypes;
      }, (offspring) => {
        return new Population(offspring, population.innovations);
      });
    }
  }
}

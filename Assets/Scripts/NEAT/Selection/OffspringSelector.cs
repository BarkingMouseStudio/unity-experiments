using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class OffspringSelector : ISelector {

    private readonly ICrossover crossover;

    public OffspringSelector(ICrossover crossover) {
      this.crossover = crossover;
    }

    private static float GetSumMeanFitness(Specie[] species) {
      return species.Aggregate(0.0f,
        (sum, sp) => sum + sp.MeanFitness);
    }

    private IEnumerable<Genotype> GetOffspring(Specie specie, int totalOffspringCount, float sumMeanFitness) {
      int speciesOffspringCount = Mathf.CeilToInt((specie.MeanFitness / sumMeanFitness) * (float)totalOffspringCount);

      // Shuffle the species
      specie.Shuffle();

      return Enumerable.Range(0, speciesOffspringCount).Select(_ => {
        Phenotype parent1 = specie[Random.Range(0, specie.Count)];
        Phenotype parent2 = null;

        // Search for a distinct second parent
        foreach (var candidate in specie) {
          if (candidate != parent1) {
            parent2 = candidate;
          }
        }

        // If the species is two small to find a parent phenotype then
        // just clone the genotype.
        if (parent2 == null) {
          return new Genotype(parent1.Genotype);
        }

        return crossover.Crossover(parent1, parent2);
      });
    }

    public Genotype[] Select(Specie[] species, int offspringCount) {
      var sumMeanFitness = GetSumMeanFitness(species);

      // Order by best performing => worst performing
      // Produce eager offspring (with `ceil`)
      // Take the needed amount
      var offspring = species.OrderBy(s => s.MeanFitness)
        .SelectMany(s => GetOffspring(s, offspringCount, sumMeanFitness))
        .Take(offspringCount)
        .ToArray();

      Assert.AreEqual(offspring.Length, offspringCount,
        "Must return the expected number of offspring");

      return offspring;
    }
  }
}

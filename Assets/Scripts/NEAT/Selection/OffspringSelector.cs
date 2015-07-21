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

    private static float GetSumMeanAdjustedFitness(Specie[] species) {
      return species.Aggregate(0.0f,
        (sum, sp) => sum + sp.MeanAdjustedFitness);
    }

    private IEnumerable<Genotype> GetOffspring(Specie specie, int totalOffspringCount, float sumMeanAdjustedFitness) {
      int speciesOffspringCount = Mathf.CeilToInt((specie.MeanAdjustedFitness / sumMeanAdjustedFitness) * (float)totalOffspringCount);

      return Enumerable.Range(0, speciesOffspringCount).Select(_ => {
        Phenotype parent1 = specie[Random.Range(0, specie.Count)];

        // If the species is too small to find a parent phenotype then
        // just clone the first parent genotype.
        if (specie.Count < 2) {
          return new Genotype(parent1.Genotype);
        }

        // // Tournament selection
        // var parent2 = specie.Sample(2).OrderByDescending(pt => pt.AdjustedFitness).First();
        // return crossover.Crossover(parent1, parent2);

        while (true) {
          var parent2 = specie[Random.Range(0, specie.Count)];
          if (parent2 != parent1) {
            return crossover.Crossover(parent1, parent2);
          }
        }
      });
    }

    public Genotype[] Select(Specie[] species, int offspringCount) {
      var sumMeanAdjustedFitness = GetSumMeanAdjustedFitness(species);

      // Order by best performing => worst performing
      // Produce eager offspring (with `ceil`)
      // Take the needed amount
      var offspring = species.OrderByDescending(s => s.MeanAdjustedFitness)
        .SelectMany(s => GetOffspring(s, offspringCount, sumMeanAdjustedFitness))
        .Take(offspringCount)
        .ToArray();

      Assert.AreEqual(offspringCount, offspring.Length,
        "Must return the expected number of offspring");

      return offspring;
    }
  }
}

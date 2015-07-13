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
      int speciesOffspringCount = Mathf.RoundToInt((specie.MeanFitness / sumMeanFitness) * (float)totalOffspringCount);
      return Enumerable.Range(0, speciesOffspringCount).Select(_ => {
        var parent1 = specie[Random.Range(0, specie.Count)];
        var parent2 = specie[Random.Range(0, specie.Count)];
        return crossover.Crossover(parent1, parent2);
      });
    }

    public Genotype[] Select(Specie[] species, int offspringCount) {
      float sumMeanFitness = GetSumMeanFitness(species);
      var offspring = species.SelectMany(s => GetOffspring(s, offspringCount, sumMeanFitness))
        .ToArray();
      Assert.AreEqual(offspring.Length, offspringCount,
        "Must return the expected number of offspring");
      return offspring;
    }
  }
}

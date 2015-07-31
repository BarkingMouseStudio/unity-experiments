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

    private IEnumerable<Genotype> GetOffspring(Specie specie, int offspringCount, float sumMeanAdjustedFitness) {
      var specieProportion = (specie.MeanAdjustedFitness / sumMeanAdjustedFitness) * (float)offspringCount;
      var specieOffspringCount = Mathf.RoundToInt(specieProportion);
      if (specieOffspringCount > 0) {
        return Enumerable.Range(0, specieOffspringCount)
          .Select(_ => specie.ProduceOffspring(crossover));
      } else {
        return new List<Genotype>(0);
      }
    }

    public Genotype[] Select(Specie[] species, int offspringCount) {
      var sumMeanAdjustedFitness = GetSumMeanAdjustedFitness(species);

      var orderedSpecies = species.OrderByDescending(sp => sp.MeanAdjustedFitness)
        .ToList();

      var offspring = orderedSpecies
        .SelectMany(sp => GetOffspring(sp, offspringCount, sumMeanAdjustedFitness))
        .Take(offspringCount)
        .ToList();

      // Fill gap by producing additional offspring from top performers
      var diff = offspringCount - offspring.Count;
      if (diff > 0) {
        var additionalOffspring = orderedSpecies
          .Take(diff)
          .Select(sp => sp.ProduceOffspring(crossover))
          .ToList();
        Debug.LogFormat("Producing {0} additional offspring", additionalOffspring.Count);
        offspring.AddRange(additionalOffspring);
      }

      var selectedOffspring = offspring.ToArray();
      Assert.AreEqual(offspringCount, selectedOffspring.Length,
        "Must select the correct number of offspring");
      return selectedOffspring;
    }
  }
}

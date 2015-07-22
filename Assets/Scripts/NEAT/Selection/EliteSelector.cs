using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class EliteSelector : ISelector {

    public Genotype[] Select(Specie[] species, int eliteCount) {
      var orderedSpecies = species.OrderByDescending(sp =>
        sp.MeanAdjustedFitness);

      var elites = orderedSpecies.Select(sp => {
        var orderedSpecie = sp.OrderByDescending(pt => pt.AdjustedFitness);
        return new Genotype(orderedSpecie.First().Genotype);
      }).Take(eliteCount).ToArray();

      Assert.AreEqual(eliteCount, elites.Length,
        "Must return the expected number of elites");

      return elites;
    }
  }
}

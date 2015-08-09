using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class EliteSelector {

    public List<Phenotype> Select(Specie[] species, int eliteCount) {
      var orderedSpecies = species.OrderByDescending(sp => sp.BestFitness);

      var elites = orderedSpecies.Select(sp => {
        var orderedSpecie = sp.OrderByDescending(pt => pt.Fitness);
        return orderedSpecie.First();
      }).Take(eliteCount).ToList();

      Assert.AreEqual(eliteCount, elites.Count,
        "Must return the expected number of elites");

      return elites;
    }
  }
}

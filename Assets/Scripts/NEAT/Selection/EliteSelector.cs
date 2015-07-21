using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class EliteSelector : ISelector {

    public Genotype[] Select(Specie[] species, int eliteCount) {
      var elites = species.Select(sp =>
        sp.OrderBy(pt => pt.Fitness).First().Genotype).ToArray();
      Assert.AreEqual(elites.Length, eliteCount,
        "Must return the expected number of elites");
      return elites;
    }
  }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Engine {

    List<Genotype> genotypes;
    Builder builder;

    public Engine(Builder builder) {
      this.builder = builder;
      this.genotypes = Enumerable.Range(0, builder.populationSize)
        .Select(_ => builder.protoGenotype.Randomize(builder.toggleProbability))
        .ToList();
    }

    public IEnumerator Run(MonoBehaviour parent) {
      while (true) {
        var phenotypes = new List<Phenotype>(genotypes.Count);
        yield return parent.StartCoroutine(builder.evaluation(genotypes, phenotypes));
      }
    }
  }
}

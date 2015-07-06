using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // TODO: Proto-genotype instead of `minimalNeuronCount`
  // TODO: Builder-pattern
  public class Population {

    public List<Genotype> genotypes;
    public Innovations innovations;

    public Population(int populationSize, Genotype protoGenotype, Innovations innovations) {
      this.genotypes = Enumerable.Range(0, populationSize)
        .Select(_ => protoGenotype.Randomize())
        .ToList();
      this.innovations = innovations;
    }

    public Population(List<Genotype> genotypes, Innovations innovations) {
      this.genotypes = genotypes;
      this.innovations = innovations;
      this.innovations.Prune(genotypes);
    }

    public int Size {
      get {
        return genotypes.Count;
      }
    }
  }
}

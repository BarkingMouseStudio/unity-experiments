using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Species {

    public int speciesId;
    public Genotype representative;

    public List<Phenotype> phenotypes;

    private Measurement measurement;

    public float AverageFitness {
      get {
        return phenotypes.Aggregate(0.0f,
          (total, pt) => total + pt.adjustedFitness,
          (total) => total / phenotypes.Count);
      }
    }

    public Species(int speciesId, Genotype representative) {
      this.speciesId = speciesId;
      this.representative = representative;
      this.phenotypes = new List<Phenotype>();
      this.measurement = new Measurement(1.0f, 1.0f, 1.0f);
    }

    public Species Next() {
      return new Species(speciesId, representative);
    }

    public float Distance(Phenotype candidate) {
      return measurement.Distance(representative, candidate.genotype);
    }

    public int Size {
      get {
        return phenotypes.Count;
      }
    }

    // Return offspring to be aggregated into new population
    public List<Genotype> Reproduce(int offspringCount, float elitism, MultipointCrossover crossover, IMutator[] mutators, Innovations innovations) {
      var speciesSize = phenotypes.Count();
      var eliteCount = Mathf.FloorToInt(elitism * (float)speciesSize);

      var sorted = phenotypes.OrderBy((g) => g.adjustedFitness).ToList();
      var nextPopulation = sorted.Take(eliteCount).Select(pt => pt.genotype).ToList();

      var offspring = new List<Genotype>(offspringCount);
      for (int i = 0; i < offspringCount; i++) {
        var parent1 = sorted[Random.Range(0, speciesSize)];
        var parent2 = sorted[Random.Range(0, speciesSize)];
        var child = crossover.Crossover(parent1, parent2);
        foreach (var mutator in mutators) {
          child = mutator.Mutate(child, innovations);
        }
        offspring.Add(child);
      }

      nextPopulation.AddRange(offspring);
      return nextPopulation;
    }
  }
}

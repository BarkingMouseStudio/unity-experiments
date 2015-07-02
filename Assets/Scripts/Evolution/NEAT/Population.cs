using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  // TODO: Proto-genotype instead of `minimalNeuronCount`
  // TODO: Builder-pattern
  public class Population {

    const float elitism = 0.2f;
    const int minimalNeuronCount = 46;

    public List<Genotype> genotypes;
    public Innovations innovations;

    MultipointCrossover crossover = new MultipointCrossover();

    IMutator[] mutators = new IMutator[]{
      new AddNeuronMutator(0.2f),
      new AddSynapseMutator(0.2f),
      new PerturbNeuronMutator(0.2f),
      new PerturbSynapseMutator(0.2f),
      new ReplaceNeuronMutator(0.1f),
      new ReplaceSynapseMutator(0.1f),
    };

    public Population(int populationSize) {
      this.genotypes = Enumerable.Range(0, populationSize).Select(i => {
        return new Genotype(minimalNeuronCount);
      }).ToList();
      this.innovations = new Innovations(minimalNeuronCount);
    }

    public Population(List<Genotype> genotypes, Innovations innovations) {
      this.genotypes = genotypes;

      this.innovations = innovations;
      this.innovations.Prune(genotypes);
    }

    // Return offspring to be aggregated into new population
    public List<Genotype> Reproduce(List<Phenotype> phenotypes, int offspringCount) {
      var speciesSize = phenotypes.Count();
      var sorted = phenotypes.OrderBy((g) => g.adjustedFitness).ToList();
      var eliteCount = Mathf.FloorToInt(elitism * (float)speciesSize);
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

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Engine {

    SpeciesManager species;
    int generation = 0;

    List<Genotype> population;
    Builder builder;

    public List<Species> Species {
      get {
        return species.Species;
      }
    }

    public int Generation {
      get {
        return generation;
      }
    }

    public List<Genotype> Population {
      get {
        return population;
      }
    }

    public Engine(Builder builder) {
      this.builder = builder;
      this.population = Enumerable.Range(0, builder.populationSize)
        .Select(_ => builder.protoGenotype.Randomize(builder.toggleProbability))
        .ToList();

      // TODO: Move to builder as arg
      this.species = new SpeciesManager(builder.desiredSpeciesCount,
        builder.distanceThreshold,
        builder.distanceThresholdAdjustment);
    }

    public void Sample(out List<Phenotype> phenotypes, out List<Genotype> genotypes) {
      phenotypes = new List<Phenotype>(population.Count);
      genotypes = population;
    }

    List<Genotype> Reproduce(List<Phenotype> phenotypes, int offspringCount, int populationSize) {
      var totalAverageFitness = species.TotalAverageFitness;
      var offspring = new List<Genotype>(offspringCount);
      foreach (var sp in species.Species) {
        var speciesOffspringCount = Mathf.CeilToInt(
          (sp.AverageFitness / totalAverageFitness) * (float)populationSize
        );
        for (int i = 0; i < speciesOffspringCount; i++) {
          var parent1 = phenotypes[Random.Range(0, sp.Size)];
          var parent2 = phenotypes[Random.Range(0, sp.Size)];
          var child = builder.crossover.Crossover(parent1, parent2);
          foreach (var mutator in builder.mutators) {
            child = mutator.Mutate(child, builder.innovations);
          }
          offspring.Add(child);
        }
      }
      return offspring;
    }

    public void Next(List<Phenotype> phenotypes) {
      var populationSize = population.Count;
      var nextPopulation = new List<Genotype>(population.Count);

      // Speciate phenotypes
      species.Speciate(phenotypes, builder.measurement);
      species.AdjustFitness(populationSize);
      species.Sort();

      phenotypes.Sort((a, b) => a.adjustedFitness.CompareTo(b.adjustedFitness));

      // Take elites
      var eliteCount = Mathf.FloorToInt(builder.elitism * populationSize);
      var elites = phenotypes.Take(eliteCount)
        .Select(pt => pt.genotype)
        .ToList();
      nextPopulation.AddRange(elites);

      // Produce offspring
      var offspringCount = populationSize - eliteCount;
      var offspring = Reproduce(phenotypes, offspringCount, populationSize);
      nextPopulation.AddRange(offspring);

      this.population = nextPopulation;
      generation++;
    }
  }
}

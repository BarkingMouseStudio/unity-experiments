using UnityEngine;
using UnityEngine.Assertions;
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

    List<Genotype> Reproduce(int populationSize) {
      // For each species, produce a new portion of the total offspring
      // proportional to that species' contributions to the total average
      // fitness.

      var totalAverageFitness = species.TotalAverageFitness;
      var offspring = new List<Genotype>(populationSize);
      foreach (var sp in species.Species) {
        var speciesContribution = Mathf.CeilToInt(
          (sp.AverageFitness / totalAverageFitness) * (float)populationSize
        );
        var speciesOffspring = new List<Genotype>(speciesContribution);

        var speciesEliteCount = Mathf.FloorToInt(builder.elitism * speciesContribution);
        var speciesElites = sp.phenotypes.Take(speciesEliteCount)
          .Select(pt => pt.genotype);
        speciesOffspring.AddRange(speciesElites);

        var speciesOffspringCount = speciesContribution - speciesEliteCount;
        for (int i = 0; i < speciesOffspringCount; i++) {
          var parent1 = sp.phenotypes[Random.Range(0, sp.phenotypes.Count)];
          var parent2 = sp.phenotypes[Random.Range(0, sp.phenotypes.Count)];
          var child = builder.crossover.Crossover(parent1, parent2);
          foreach (var mutator in builder.mutators) {
            child = mutator.Mutate(child, builder.innovations);
          }
          speciesOffspring.Add(child);
        }

        offspring.AddRange(speciesOffspring);
      }
      return offspring.Take(populationSize).ToList();
    }

    public void Next(List<Phenotype> phenotypes) {
      // Speciate phenotypes
      species.Speciate(phenotypes, builder.measurement);
      species.AdjustFitness(builder.populationSize);
      species.Sort();

      phenotypes.Sort((a, b) => a.adjustedFitness.CompareTo(b.adjustedFitness));
      Assert.IsTrue(phenotypes.First().adjustedFitness <= phenotypes.Last().adjustedFitness);

      // Produce offspring
      this.population = Reproduce(builder.populationSize);
      Assert.AreEqual(this.population.Count, builder.populationSize);
      generation++;
    }
  }
}

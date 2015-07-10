using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Engine {

    SpeciesManager species;
    int generation = 0;

    List<Genotype> genotypes;
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
        return genotypes;
      }
    }

    public Engine(Builder builder) {
      this.builder = builder;
      this.genotypes = Enumerable.Range(0, builder.populationSize)
        .Select(_ =>
          builder.protoGenotype.Randomize(builder.toggleProbability))
        .ToList();

      this.species = new SpeciesManager(builder.desiredSpeciesCount,
        builder.distanceThreshold,
        builder.distanceThresholdAdjustment);
    }

    List<Genotype> Reproduce(List<Phenotype> phenotypes, int offspringCount) {
      var totalAverageFitness = species.TotalAverageFitness;
      var offspring = new List<Genotype>(offspringCount);
      foreach (var sp in species.Species) {
        var speciesOffspringCount = Mathf.CeilToInt(
          (sp.AverageFitness / totalAverageFitness) * (float)offspringCount
        );
        // Debug.LogFormat("Species: {0} => {1} ({2})", sp.speciesId, speciesOffspringCount, sp.AverageFitness);
        for (int i = 0; i < speciesOffspringCount; i++) {
          var parent1 = phenotypes[Random.Range(0, sp.Size)];
          var parent2 = phenotypes[Random.Range(0, sp.Size)];
          var child = builder.crossover.Crossover(parent1, parent2);
          foreach (var mutator in builder.mutators) {
            mutator.Mutate(child, builder.innovations);
          }
          offspring.Add(child);
        }
      }
      return offspring.Take(offspringCount).ToList();
    }

    public void Next(List<Phenotype> phenotypes) {
      var populationSize = genotypes.Count;
      var nextPopulation = new List<Genotype>(genotypes.Count);

      // Speciate phenotypes
      species.Speciate(phenotypes, builder.measurement);
      species.AdjustFitness(populationSize);
      species.Sort();

      phenotypes.Sort((a, b) => a.adjustedFitness.CompareTo(b.adjustedFitness));

      // Take elites
      var eliteCount = Mathf.FloorToInt(builder.elitism * populationSize);
      var elites = phenotypes.Take(eliteCount)
        .Select(pt => pt.genotype.Clone())
        .ToList();
      nextPopulation.AddRange(elites);

      // Produce offspring
      var offspringCount = populationSize - eliteCount;
      var offspring = Reproduce(phenotypes, offspringCount);
      nextPopulation.AddRange(offspring);

      this.genotypes = nextPopulation.Take(populationSize).ToList();
      generation++;
    }
  }
}

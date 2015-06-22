using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class EnforcedSubpopulations : IEvolutionaryAlgorithm {

  public struct Phenotype {
    public float cumulative;
    public float average;
    public int trials;
    public float[] chromosome;
  }

  Phenotype[][] subpopulations;
  int generation;

  // Keep track of current sample to match up with fitnesses
  Tuple<int, float[]>[][] currentSample;

  public EnforcedSubpopulations(CommonGenotype genotype, int subpopulationSize) {
    this.subpopulations = genotype.Select((chromosome) => {
      return Enumerable.Range(0, subpopulationSize).Select((_0) => {
        return new Phenotype{
          chromosome = chromosome.Select((_1) => Random.Range(0.0f, 1.0f)).ToArray(),
          cumulative = 0.0f,
          average = 0.0f,
          trials = 0,
        };
      }).ToArray();
    }).ToArray();
  }

  public int Generation {
    get {
      return generation;
    }
  }

  public CommonGenotype[] Sample(int count) {
    currentSample = Enumerable.Range(0, count).Select((_) => {
      return this.subpopulations.Select((subpopulation) => {
        var index = Random.Range(0, subpopulation.Length);
        var pt = subpopulation[index];
        return Tuple.Of(index, pt.chromosome);
      }).ToArray();
    }).ToArray();

    // Flatten sample to common representation
    return currentSample.Select((genotype) => {
      return new CommonGenotype(genotype.Select((chromosome) => {
        return chromosome.Second;
      }).ToArray());
    }).ToArray();
  }

  public void Update(float[] fitness) {
    var total_trials = 0.0f;
    var i = 0;

    // Map population fitness to appropriate phenotype
    foreach (var genotype in currentSample) {
      var trials = 0.0f;
      int j = 0;

      foreach (var ch in genotype) {
        var subpopulation = this.subpopulations[j];

        var pt = subpopulation[ch.First];
        pt.cumulative += fitness[i];
        pt.trials += 1;
        pt.average = pt.cumulative / ((float)pt.trials);

        trials += ((float)pt.trials);
        j++;
      }

      total_trials += trials / ((float)genotype.Length);
      i++;
    }

    // Sort subpopulations by updated fitness
    foreach (var subpopulation in this.subpopulations) {
      subpopulation.OrderBy((a) => a.average);
    }

    // Divide the total number of trials by the # evaluated
    var averageTrials = total_trials / ((float)currentSample.Length);
    if (averageTrials > 10.0f) {
      var results = Recombine();
      Debug.LogFormat("[{0}] Generation complete. (Best: {1}, Offspring: {2}, Mutations: {3})", generation, fitness.Min(f => f), results.First, results.Second);
    } else {
      Debug.LogFormat("[{0}] Generation complete. (Best: {1})", generation, fitness.Min(f => f));
    }
  }

  public Tuple<int, int> Recombine() {
    var new_subpopulations = new List<Phenotype[]>(this.subpopulations.Length);

    var mutation_count = 0;
    var offspring_count = 0;
    var p = 0.125f;

    foreach (var subpopulation in this.subpopulations) {
      var quartile = Mathf.FloorToInt((float)0.25f * (subpopulation.Length));
      var half = Mathf.FloorToInt((float)0.5f * (subpopulation.Length));

      offspring_count += half;

      // Each neuron in the top quartile of its subpopulation is recombined with a
      // higher-ranking neuron in the same subpopulation using crossover and low-probability
      // mutation.
      var top_quartile = subpopulation.Take(quartile).ToArray();

      var offspring = new List<Phenotype>(half);
      foreach (var _ in Enumerable.Range(0, half)) {
        var i1 = Random.Range(0, top_quartile.Length);
        var i2 = Random.Range(0, top_quartile.Length);

        var pt1 = top_quartile[i1];
        var pt2 = top_quartile[i2];

        var x = Random.Range(0, pt1.chromosome.Length);

        // Crossover
        var ch1 = pt1.chromosome.Take(x).ToList();
        var ch2 = pt2.chromosome.Skip(x).ToList();

        ch1.AddRange(ch2);

        // Low-probability mutation
        var mutated_ch = ch1.Select((v) => {
          if (Random.value < p) {
            mutation_count += 1;
            return RandomHelper.NextCauchy(v, 0.1f);
          } else {
            return v;
          }
        }).ToArray();

        offspring.Add(new Phenotype{
          chromosome = mutated_ch,
          cumulative = 0.0f,
          average = 0.0f,
          trials = 0,
        });
      }

      // The offspring is used to replace the lowest-ranking half of the population.
      var new_population = subpopulation.Take(half).ToList();
      new_population.AddRange(offspring.ToArray());
      new_subpopulations.Add(new_population.ToArray());
    }

    this.subpopulations = new_subpopulations.ToArray();
    this.generation++;

    return Tuple.Of(offspring_count, mutation_count);
  }
}

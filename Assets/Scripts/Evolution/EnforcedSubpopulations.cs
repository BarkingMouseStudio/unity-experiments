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

  const float requiredTrials = 0.0f;
  const float mutationScale = 1.0f;
  const float mutationRate = 0.125f;

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

  public Tuple<float, int, int> Update(float[] fitness) {
    var totalTrials = 0.0f;
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
        pt.average = pt.cumulative / (float)pt.trials;
        subpopulation[ch.First] = pt; // Re-assign phenotype value-type

        trials += (float)pt.trials;
        j++;
      }

      totalTrials += trials / (float)genotype.Length;
      i++;
    }

    // Sort subpopulations by updated fitness
    foreach (var subpopulation in this.subpopulations) {
      subpopulation.OrderBy((a) => a.average);
    }

    // Divide the total number of trials by the # evaluated
    var averageTrials = totalTrials / (float)currentSample.Length;
    if (averageTrials > requiredTrials) {
      var results = Recombine();
      return Tuple.Of(averageTrials, results.First, results.Second);
    } else {
      return Tuple.Of(averageTrials, 0, 0);
    }
  }

  public Tuple<int, int> Recombine() {
    var newSubpopulations = new List<Phenotype[]>(this.subpopulations.Length);

    var mutationCount = 0;
    var offspringCount = 0;

    foreach (var subpopulation in this.subpopulations) {
      var quartile = Mathf.FloorToInt(0.25f * (float)subpopulation.Length);
      var half = Mathf.FloorToInt(0.5f * (float)subpopulation.Length);

      offspringCount += half;

      // Each neuron in the top quartile of its subpopulation is recombined with a
      // higher-ranking neuron in the same subpopulation using crossover and low-probability
      // mutation.
      var topQuartile = subpopulation.Take(quartile).ToArray();

      var offspring = new List<Phenotype>(half);
      for (var i = 0; i < half; i++) {
        var i1 = Random.Range(0, topQuartile.Length);
        var i2 = Random.Range(0, topQuartile.Length);

        var pt1 = topQuartile[i1];
        var pt2 = topQuartile[i2];

        var x = Random.Range(0, pt1.chromosome.Length);

        // Crossover
        var ch1 = pt1.chromosome.Take(x).ToList();
        var ch2 = pt2.chromosome.Skip(x).ToList();

        ch1.AddRange(ch2);

        // Low-probability mutation
        var mutatedCh = ch1.Select((v) => {
          if (Random.value < mutationRate) {
            mutationCount += 1;
            return RandomHelper.NextCauchy(v, mutationScale);
          } else {
            return v;
          }
        }).ToArray();

        AssertHelper.Assert(mutatedCh.Length == pt1.chromosome.Length,
          "Matching chromosome length");
        AssertHelper.Assert(mutatedCh.Length == pt2.chromosome.Length,
          "Matching chromosome length");

        offspring.Add(new Phenotype{
          chromosome = mutatedCh,
          cumulative = 0.0f,
          average = 0.0f,
          trials = 0,
        });
      }

      // The offspring is used to replace the lowest-ranking half of the population.
      var newPopulation = subpopulation.Take(half).ToList();
      newPopulation.AddRange(offspring.ToArray());
      newSubpopulations.Add(newPopulation.ToArray());

      AssertHelper.Assert(newPopulation.Count == subpopulation.Length,
        "Matching replacement population count");
    }

    this.subpopulations = newSubpopulations.ToArray();
    this.generation++;

    return Tuple.Of(offspringCount, mutationCount);
  }
}

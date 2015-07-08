using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class Builder {

    public int populationSize = 500;
    public float elitism = 0.2f;

    public int speciesCount = 10;
    public float distanceThreshold = 100.0f;
    public float distanceThresholdAdjustment = 0.2f;

    public float toggleProbability = 0.2f;
    public float mutationScale = 0.5f;

    public ICrossover crossover;
    public IMutator[] mutators;

    public Measurement measurement;

    public Innovations innovations;
    public Genotype protoGenotype;

    public Func<List<Genotype>, List<Phenotype>, IEnumerator> evaluation;
    public Action<Result> results;

    public Builder Innovations(Innovations innovations) {
      this.innovations = innovations;
      return this;
    }

    public Builder Results(Action<Result> results) {
      this.results = results;
      return this;
    }

    public Builder Evaluation(Func<List<Genotype>, List<Phenotype>, IEnumerator> evaluation) {
      this.evaluation = evaluation;
      return this;
    }

    public Builder ProtoGenotype(Genotype protoGenotype) {
      this.protoGenotype = protoGenotype;
      return this;
    }

    public Builder Population(int populationSize) {
      this.populationSize = populationSize;
      return this;
    }

    public Builder Elitism(float elitism) {
      this.elitism = elitism;
      return this;
    }

    public Builder Species(int speciesCount, float distanceThreshold, float distanceThresholdAdjustment) {
      this.speciesCount = speciesCount;
      this.distanceThreshold = distanceThreshold;
      this.distanceThresholdAdjustment = distanceThresholdAdjustment;
      return this;
    }

    public Builder Crossover(ICrossover crossover) {
      this.crossover = crossover;
      return this;
    }

    public Builder Mutators(IMutator[] mutators) {
      this.mutators = mutators;
      return this;
    }

    public Builder Measurement(Measurement measurement) {
      this.measurement = measurement;
      return this;
    }

    public Builder Mutation(float mutationScale, float toggleProbability) {
      this.mutationScale = mutationScale;
      this.toggleProbability = toggleProbability;
      return this;
    }

    public Engine Build() {
      Assert.IsNotNull(protoGenotype, "Must define protoGenotype");
      Assert.IsNotNull(measurement, "Must define measurement");
      Assert.IsNotNull(crossover, "Must define crossover");
      Assert.IsNotNull(mutators, "Must define mutators");
      Assert.IsNotNull(evaluation, "Must define evaluation");
      Assert.IsNotNull(results, "Must define results");
      return new Engine(this);
    }
  }
}

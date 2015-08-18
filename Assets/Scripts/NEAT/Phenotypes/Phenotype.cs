using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NEAT {

  public class Phenotype {

    private readonly List<Trial> trials = new List<Trial>(1);
    private readonly Genotype genotype;

    public int TrialCount {
      get {
        return trials.Count;
      }
    }

    public Trial CurrentTrial { get; protected set; }

    public Genotype Genotype {
      get {
        return genotype;
      }
    }

    public float BestFitness {
      get {
        return trials.Max(t => t.Fitness);
      }
    }

    public float WorstFitness {
      get {
        return trials.Min(t => t.Fitness);
      }
    }

    public float MeanFitness {
      get {
        if (trials.Count > 0) {
          return trials.Aggregate(0.0f,
            (sum, t) => sum + t.Fitness,
            (sum) => sum / trials.Count);
        } else {
          return 0.0f;
        }
      }
    }

    public float Fitness {
      get {
        return MeanFitness;
      }
    }

    public float BestDuration {
      get {
        return trials.Max(t => t.Duration);
      }
    }

    public float WorstDuration {
      get {
        return trials.Min(t => t.Duration);
      }
    }

    public float MeanDuration {
      get {
        if (trials.Count > 0) {
          return trials.Aggregate(0.0f,
            (sum, t) => sum + t.Duration,
            (sum) => sum / trials.Count);
        } else {
          return 0.0f;
        }
      }
    }

    public float AdjustedFitness { get; set; }

    public Phenotype(Genotype genotype) {
      this.genotype = genotype;
    }

    public float GetStdDevFitness(float mean) {
      return trials.Aggregate(0.0f,
        (stdev, t) => stdev + Mathf.Abs(t.Fitness - mean),
        (stdev) => stdev / trials.Count);
    }

    public void AddTrial(Trial trial) {
      trials.Add(trial);
    }
  }
}

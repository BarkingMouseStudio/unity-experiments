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

    public float AverageFitness {
      get {
        if (trials.Count > 0) {
          var average = trials.Aggregate(0.0f,
            (sum, t) => sum + t.Fitness,
            (sum) => sum / trials.Count);

          // TODO: This may not work:
          var stdevAverage = trials.Aggregate(0.0f,
            (stdev, t) => stdev + Mathf.Abs(t.Fitness - average),
            (stdev) => stdev / trials.Count);
          return average + stdevAverage;
        } else {
          return 1.0f;
        }
      }
    }

    public float AverageDuration {
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

    public Trial BeginTrial(Orientations orientation, float startTime) {
      CurrentTrial = new Trial(orientation, startTime);
      trials.Add(CurrentTrial);
      return CurrentTrial;
    }

    public void UpdateTrial(float thetaLower, float thetaDotLower, float x, float xDot) {
      CurrentTrial.Update(thetaLower, thetaDotLower, x, xDot);
    }

    public void EndTrial(float endTime) {
      CurrentTrial.End(endTime);
    }
  }
}

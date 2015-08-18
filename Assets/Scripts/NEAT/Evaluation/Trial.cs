using UnityEngine;
using System;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Trial {

  const float fitnessDurationWeight = 0.5f;
  const float fitnessHistoryWeight = 0.5f;

  private readonly Orientations orientation;

  private float startTime;
  private float endTime;

  private readonly float[] fitnessHistory = new float[100];
  private int fitnessHistoryIndex = 0;
  private int fitnessUpdateCount = 0;

  public Trial(Orientations orientation, float startTime) {
    this.orientation = orientation;
    this.startTime = startTime;
    this.fitnessHistory.Fill(0.0f);
  }

  public Orientations Orientation {
    get {
      return orientation;
    }
  }

  public float Duration {
    get {
      if (endTime == 0.0f) {
        return Time.time - startTime;
      }
      return endTime - startTime;
    }
  }

  public float[] FitnessHistory {
    get {
      return fitnessHistory;
    }
  }

  public float NormalizedFitnessDuration {
    get {
      return ((float)fitnessUpdateCount) / 1000.0f;
    }
  }

  public float NormalizedFitnessHistory {
    get {
      if (fitnessUpdateCount < fitnessHistory.Length) {
        return 0.0f;
      }

      var v = fitnessHistory.Aggregate(0.0f,
        (total, next) => total + next,
        (total) => total / fitnessHistory.Length);

      if (v == 0.0f) {
        return 0.0f;
      }

      return Mathf.Max(1.0f - v, 0.0f);
    }
  }

  public float Fitness {
    get {
      var fitnessDurationContribution = fitnessDurationWeight * NormalizedFitnessDuration;
      var fitnessHistoryContribution = fitnessHistoryWeight * NormalizedFitnessHistory;
      return fitnessDurationContribution + fitnessHistoryContribution;
    }
  }

  public void Reset(float startTime) {
    // this.startTime = startTime;
    // if (fitnessUpdateCount > 0) {
    //   fitnessUpdateCount = 0;
    //   fitnessHistoryIndex = 0;
    //   fitnessHistory.Fill(0.0f);
    // }
  }

	public void Update(float thetaLower, float thetaDotLower, float thetaUpper, float thetaDotUpper, float x, float xDot) {
    var fitness =
      Mathf.Abs(thetaLower) / 180.0f +
      Mathf.Abs(thetaDotLower) / 180.0f +
      Mathf.Abs(thetaUpper) / 180.0f +
      Mathf.Abs(thetaDotUpper) / 180.0f +
      Mathf.Abs(x) / 14.0f +
      Mathf.Abs(xDot) / 14.0f;

    fitness /= 6.0f;

    fitnessHistory[fitnessHistoryIndex] = fitness;
    fitnessHistoryIndex++;
    fitnessHistoryIndex %= fitnessHistory.Length;

    fitnessUpdateCount++;
	}

  public void End(float endTime) {
    this.endTime = endTime;
  }
}

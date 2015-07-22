using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Trial {

  const float fitnessDurationWeight = 0.1f;
  const float fitnessHistoryWeight = 0.9f;

  private readonly Orientations orientation;

  private readonly float startTime;
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

      return 0.75f / v;
    }
  }

  public float Fitness {
    get {
      if (fitnessUpdateCount < fitnessHistory.Length) {
        return 0.0f; // Worst case, didn't live long enough
      } else {
        return (fitnessDurationWeight * NormalizedFitnessDuration) +
               (fitnessHistoryWeight * NormalizedFitnessHistory);
      }
    }
  }

	public void Update(float thetaLower, float thetaDotLower, float thetaUpper, float thetaDotUpper, float x, float xDot) {
    var fitness =
      Mathf.Abs(thetaLower) * 1.0f +
      Mathf.Abs(thetaDotLower) * 1.0f +
      Mathf.Abs(thetaUpper) * 1.0f +
      Mathf.Abs(thetaDotUpper) * 1.0f +
      Mathf.Abs(x) * 1.0f +
      Mathf.Abs(xDot) * 1.0f;

    fitnessHistory[fitnessHistoryIndex] = fitness;
    fitnessHistoryIndex++;
    fitnessHistoryIndex %= fitnessHistory.Length;

    fitnessUpdateCount++;
	}

  public void End(float endTime) {
    this.endTime = endTime;
  }
}

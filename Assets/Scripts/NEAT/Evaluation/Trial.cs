using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Trial {

  // Evaluation fitness
  const int minFitnessUpdateCount = 100;
  const int fitnessHistoryLength = 1000;

  const float fitnessDurationWeight = 0.1f;
  const float fitnessHistoryWeight = 0.9f;

  private readonly Orientations orientation;

  private readonly float startTime;
  private float endTime;

  private readonly float[] fitnessHistory = new float[fitnessHistoryLength];
  private int fitnessHistoryIndex = 0;
  private int fitnessUpdateCount = 0;

  public Trial(Orientations orientation, float startTime) {
    this.orientation = orientation;
    this.startTime = startTime;
    this.fitnessHistory.Fill(1.0f);
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
			return 1.0f - ((float)fitnessUpdateCount / (float)fitnessHistoryLength);
    }
  }

  public float NormalizedFitnessHistory {
    get {
      return fitnessHistory.Aggregate(0.0f,
        (total, next) => total + next,
        (total) => total / fitnessHistory.Length);
    }
  }

  public float Fitness {
    get {
      if (fitnessUpdateCount >= minFitnessUpdateCount) {
        return (fitnessDurationWeight * NormalizedFitnessDuration) +
               (fitnessHistoryWeight * NormalizedFitnessHistory);
      } else {
        return 1.0f; // Worst case, didn't live long enough
      }
    }
  }

	public void Update(float thetaLower, float thetaDotLower, float thetaUpper, float thetaDotUpper, float x, float xDot) {
    var fitness =
      Mathf.Abs(thetaLower) * 1.0f +
      Mathf.Abs(thetaDotLower) * 1.0f +
      Mathf.Abs(thetaUpper) * 1.0f +
      Mathf.Abs(thetaDotUpper) * 1.0f +
      Mathf.Abs(x) * 30.0f +
      Mathf.Abs(xDot - 1.0f) * 30.0f;

    fitnessHistory[fitnessHistoryIndex] = fitness / (180.0f * 6.0f);
    fitnessHistoryIndex++;
    fitnessHistoryIndex %= fitnessHistory.Length;

    fitnessUpdateCount++;
	}

  public void End(float endTime) {
    this.endTime = endTime;
  }
}

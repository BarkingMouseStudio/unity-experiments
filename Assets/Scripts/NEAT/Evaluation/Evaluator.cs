using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Evaluator {

  // Evaluation fitness
  const int minFitnessUpdateCount = 100;
  const int fitnessHistoryLength = 1000;

  const float fitnessDurationWeight = 0.1f;
  const float fitnessHistoryWeight = 0.9f;

  int fitnessHistoryIndex = 0;
  int fitnessUpdateCount = 0;
  float[] fitnessHistory = new float[fitnessHistoryLength];

  public Evaluator() {
    fitnessHistory.Fill(1.0f);
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

	public void Update(float thetaLower, float thetaDotLower, float x, float xDot) {
    var fitness =
      Mathf.Abs(thetaLower) * 1.0f +
      Mathf.Abs(thetaDotLower) * 1.0f +
      Mathf.Abs(x) * 30.0f +
      Mathf.Abs(xDot) * 30.0f;

    fitnessHistory[fitnessHistoryIndex] = fitness / (180.0f * 4.0f);
    fitnessHistoryIndex++;
    fitnessHistoryIndex %= fitnessHistory.Length;

    fitnessUpdateCount++;
	}
}

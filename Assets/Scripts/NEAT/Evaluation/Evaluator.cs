using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Evaluator {

  // Evaluation fitness
  const int fitnessLength = 100;

  const float fitnessCountWeight = 0.1f;
  const float fitnessWeight = 0.9f;

  const int maximumFitnessCount = 1500; // 30s * 1000ms / 20 ticks
  const float maximumFitness = 180.0f * 4.0f;

  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  public float Fitness {
    get {
      if (fitnessCount < fitnessHistory.Length) {
        return 1.0f; // Worst case, didn't live long enough
      }

      var normalizedFitness = fitnessHistory.Aggregate(0.0f,
        (total, next) => total + next,
        (total) => total / fitnessHistory.Length);

			var normalizedFitnessCount = 1.0f -
        ((float)fitnessCount / (float)maximumFitnessCount);

      return (fitnessCountWeight * normalizedFitnessCount) +
             (fitnessWeight * normalizedFitness);
    }
  }

	public void Update(float thetaLower, float thetaDotLower, float x, float xDot) {
    var fitness =
      Mathf.Abs(thetaLower) * 1.0f +
      Mathf.Abs(thetaDotLower) * 1.0f +
      Mathf.Abs(x) * 30.0f +
      Mathf.Abs(xDot - 1.0f) * 30.0f;
    var normalizedFitness = fitness / maximumFitness;

    fitnessHistory[fitnessIndex] = normalizedFitness;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;

    fitnessCount++;
	}
}

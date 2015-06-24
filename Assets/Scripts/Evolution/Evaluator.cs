using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Evaluator {

  // Evaluation fitness
  static readonly int fitnessLength = 100;
  static readonly int maximumFitnessCount = 500; // 10s * 1000ms / 20 ticks

  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  public float Fitness {
    get {
      if (fitnessCount > fitnessHistory.Length) {
        var normalizedFitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
				var normalizedFitnessCount = 1.0f - ((float)fitnessCount / (float)maximumFitnessCount);
        return 0.5f * normalizedFitnessCount + 0.5f * normalizedFitness;
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
      Mathf.Abs(xDot - 1.0f) * 30.0f;
    var maximumFitness = 180.0f * 4.0f;
    var normalizedFitness = fitness / maximumFitness;

    fitnessHistory[fitnessIndex] = normalizedFitness;
    fitnessCount++;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;
	}
}

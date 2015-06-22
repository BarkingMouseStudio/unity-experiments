using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for measuring fitness.
public class Evaluator {

  // Evaluation fitness
  static readonly int fitnessLength = 100;

  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  float duration;

  public Evaluator(float duration) {
    this.duration = duration;
  }

  public float Fitness {
    get {
      if (fitnessCount > fitnessHistory.Length) {
        var normalizedFitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
				var normalizedFitnessCount = 1.0f - (fitnessCount / (duration * 20.0f));
        return 0.1f * normalizedFitnessCount + 0.9f * normalizedFitness;
      } else {
        return 1.0f; // Worst case, didn't live long enough
      }
    }
  }

	public void Update(float thetaLower, float thetaDotLower, float x, float xDot) {
    fitnessHistory[fitnessIndex] =
      Mathf.Abs(thetaLower / 180.0f) * 1.0f +
      Mathf.Abs(thetaDotLower / 180.0f) * 1.0f +
      Mathf.Abs(x / 30.0f) * 30.0f + Mathf.Abs(xDot / 30.0f) * 30.0f;

    fitnessCount++;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;
	}
}

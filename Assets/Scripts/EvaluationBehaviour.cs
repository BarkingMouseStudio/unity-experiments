using UnityEngine;
using System.Collections;
using System.Linq;

public class EvaluationBehaviour : MonoBehaviour {

  protected internal bool isComplete = false;

  const float duration = 10.0f;
  float now = 0.0f;

  const int fitnessLength = 100;
  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  Transform handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  public float Fitness {
    get {
      if (fitnessCount > fitnessHistory.Length) {
        var averageFitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
				var normalizedFitnessCount = 1.0f - (fitnessCount / 500.0f);
        return 0.1f * normalizedFitnessCount + 0.9f * averageFitness;
      } else {
        return 1.0f; // Worst case, didn't live long enough
      }
    }
  }

	// Use this for initialization
	void Awake() {
    handle = transform.Find("Cart/Lower/Handle");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
	}

	void OnDespawned() {
    // Clear fitness history
    fitnessHistory = new float[fitnessLength];
    fitnessIndex = 0;
    fitnessCount = 0;

    now = 0.0f;
    isComplete = false;
	}

	void Complete() {
		isComplete = true;
	}

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (duration - now <= 0) { // Elapsed
      Complete();
      return;
    }

    if (wheel.transform.position.y < -2.0) { // Fallen
      Complete();
      return;
    }

		if (handle.position.y < 0.0) { // Destabilized
			Complete();
			return;
		}

    // var thetaUpper = AngleHelper.GetAngle(upper.rotation);
    // var thetaDotUpper = AngleHelper.GetAngle(upper.angularVelocity);
    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    float fitness = Mathf.Abs(thetaLower) * 1.0f + Mathf.Abs(thetaDotLower) * 1.0f +
								// Mathf.Abs(thetaUpper) * 1.0f + Mathf.Abs(thetaDotUpper) * 1.0f +
                Mathf.Abs(x) * 30.0f + Mathf.Abs(xDot) * 30.0f;
    var maxFitness = 180.0f * 4.0f;
		var normalizedFitness = fitness / maxFitness;

    fitnessHistory[fitnessIndex] = normalizedFitness;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;
    fitnessCount++;

    now += Time.fixedDeltaTime;
	}
}

using UnityEngine;
using System.Collections;
using System.Linq;

// Responsible for initializing the test and measuring fitness.
public class EvaluationBehaviour : MonoBehaviour {

  public enum Orientations {
    Upright = 180,
    SoftLeft = 185,
    SoftRight = 175,
    MediumLeft = 195,
    MediumRight = 165,
    HardLeft = 210,
    HardRight = 150,
    VeryHardLeft = 225,
    VeryHardRight = 135,
    Random = -1,
  }

  public Orientations orientation;

  // Evaluation duration
  const float duration = 10.0f;
  float now = 0.0f;

  // Evaluation fitness
  const int fitnessLength = 100;
  int fitnessIndex = 0;
  int fitnessCount = 0;
  float[] fitnessHistory = new float[fitnessLength];

  Transform handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  public bool IsComplete {
    get {
      bool elapsed = duration - now <= 0;
      bool fallen = wheel.transform.position.y < -2.0;
      bool destabilized = handle.position.y < 0.0;
      return elapsed || fallen || destabilized;
    }
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

  public void SetOrientation() {
    Quaternion rotation;

    switch (orientation) {
      case Orientations.Random:
        rotation = Quaternion.Euler(0, 0, Random.Range(135, 225));
        break;
      default:
        rotation = Quaternion.Euler(0, 0, (int)orientation);
        break;
    }

    cart.transform.localRotation = rotation;
  }

	void Awake() {
    handle = transform.Find("Cart/Lower/Handle");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();

    SetOrientation();
	}

	void OnDespawned() {
    // Clear fitness history
    fitnessHistory = new float[fitnessLength];
    fitnessIndex = 0;
    fitnessCount = 0;

    // Reset timer
    now = 0.0f;
	}

	void FixedUpdate() {
    if (IsComplete) {
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude - 1.0f; // Velocity penalty to encourage movement

    fitnessHistory[fitnessIndex] =
      Mathf.Abs(thetaLower / 180.0f) * 1.0f +
      Mathf.Abs(thetaDotLower / 180.0f) * 1.0f +
      Mathf.Abs(x / 30.0f) * 30.0f + Mathf.Abs(xDot / 30.0f) * 30.0f;

    fitnessCount++;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;

    now += Time.fixedDeltaTime;
	}
}

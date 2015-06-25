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
    Downward = 0,
    Random = -1,
  }

  public Orientations orientation;

  // Evaluation duration
  const float duration = 10.0f;
  float now = 0.0f;

  Transform cart;
  Transform handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  Evaluator evaluator;

  private bool isComplete = false;
  public bool IsComplete {
    get {
      return isComplete;
    }
  }

  public float Now {
    get {
      return now;
    }
  }

  public float Fitness {
    get {
      return evaluator.Fitness;
    }
  }

  public float Angle {
    get {
      return (float)orientation;
    }
  }

	void Awake() {
    cart = transform.Find("Cart");
    handle = transform.Find("Cart/Lower/Handle");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();

    evaluator = new Evaluator();

    SetOrientation();
	}

  void OnSpawned() {
    SetOrientation();
  }

	void OnDespawned() {
    evaluator = new Evaluator();

    // Reset status
    isComplete = false;

    // Reset timer
    now = 0.0f;
	}

  public void SetOrientation() {
    if (orientation == Orientations.Random) {
      orientation = Random.value > 0.5f ?
        Orientations.HardLeft : Orientations.HardRight;
    }
    cart.localRotation = Quaternion.Euler(0, 0, (int)orientation);
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (wheel.transform.position.y < -2.0) {
      isComplete = true;
      return;
    }

    if (handle.position.y < 0.0) {
      isComplete = true;
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude; // Velocity penalty to encourage movement

    evaluator.Update(thetaLower, thetaDotLower, x, xDot);

    now += Time.fixedDeltaTime;
	}
}

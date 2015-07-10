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

  // float[] angles = new float[]{135f, 225f}; // , 150f, 165f, 175f, 185f, 195f, 210f
  public Orientations orientation;

  public Orientations orientation;

  Transform cart;
  Transform handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  Evaluator evaluator;

  float startTime;
  float endTime;

  public NEAT.Genotype genotype;

  private bool isComplete = false;
  public bool IsComplete {
    get {
      return isComplete;
    }
  }

  public float Now {
    get {
      return endTime - startTime;
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

    SetRotation();
	}

  void OnSpawned() {
    SetRotation();
    startTime = Time.time;
  }

	void OnDespawned() {
    evaluator = new Evaluator();

    // Reset status
    isComplete = false;
	}

  void SetRotation() {
    float angle;
    if (orientation == Orientations.Random) {
      angle = 135.0f; // angles[Random.Range(0, angles.Length)];
    } else {
      angle = (float)orientation;
    }
    cart.localRotation = Quaternion.Euler(0f, 0f, angle);
  }

  void Complete() {
    isComplete = true;
    endTime = Time.time;
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (wheel.transform.position.y < -2.0f) {
      Complete();
      return;
    }

    if (handle.position.y < 0.0f) {
      Complete();
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude; // Velocity penalty to encourage movement

    evaluator.Update(thetaLower, thetaDotLower, x, xDot);
	}
}

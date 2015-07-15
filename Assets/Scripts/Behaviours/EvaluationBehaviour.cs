using UnityEngine;
using System;
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

  // static readonly float[] angles = new float[]{
  //   135f,
  //   225f,
  // };

  Transform cart;
  Transform handle;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  // TODO: Attach Evalutor (EvaluationInfo) to Phenotype
  Evaluator evaluator;

  float startTime;
  float endTime;

  Vector3 startPosition;

  // TODO: Use Phenotype (not Genotype)
  public NEAT.Genotype Genotype { get; set; }

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

  void Init() {
    SetRotation();

    startPosition = transform.position;

    // TODO: Move time, angle and fitness accessors to Evaluator (=> EvaluationInfo)
    evaluator = new Evaluator();
    startTime = Time.time;
  }

	void Awake() {
    cart = transform.Find("Cart");
    handle = transform.Find("Cart/Lower/Handle");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();

    Init();
	}

  void OnSpawned() {
    Init();
  }

	void OnDespawned() {
    // Reset status
    isComplete = false;
	}

  void SetRotation() {
    float angle = 0.0f;
    if (orientation == Orientations.Random) {
      angle = 195f; // angles[UnityEngine.Random.Range(0, angles.Length)];
    } else {
      angle = (float)orientation;
    }
    cart.localRotation = Quaternion.Euler(0f, 0f, angle);
  }

  void Complete() {
    wheel.isKinematic = true; // Freeze the wheel
    lower.isKinematic = true;
    isComplete = true;
    endTime = Time.time;
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (wheel.transform.position.y - startPosition.y < -2.0f) {
      Complete();
      return;
    }

    if (handle.position.y - startPosition.y < 0.0f) {
      Complete();
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    evaluator.Update(thetaLower, thetaDotLower, x, xDot);
	}
}

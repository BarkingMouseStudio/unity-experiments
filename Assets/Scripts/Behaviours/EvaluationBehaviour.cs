using UnityEngine;
using System;
using System.Collections;
using System.Linq;

// Responsible for initializing the test and measuring fitness.
public class EvaluationBehaviour : MonoBehaviour {

  public Orientations orientation;

  Transform cart;
  Rigidbody2D lower;
  Rigidbody2D upper;
  Rigidbody2D wheel;
  // ControllerBehaviour controllerBehaviour;

  Trial currentTrial;

  public Trial CurrentTrial {
    get { return currentTrial; }
  }

  public Rigidbody2D Wheel {
    get { return wheel; }
  }

  Vector3 startPosition;
  float startTime;

  bool dying;
  float dyingStart;
  int dyingCount;

  public NEAT.Phenotype Phenotype { get; set; }

  private bool isComplete = false;
  public bool IsComplete {
    get {
      return isComplete;
    }
  }

	void Awake() {
    cart = transform.Find("Cart");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    upper = transform.Find("Cart/Upper").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    // controllerBehaviour = GetComponent<ControllerBehaviour>();
	}

  void Start() {
    SetRotation(orientation);
    startPosition = transform.position;

    BeginTrial(orientation, Time.time);
  }

  void OnSpawned() {
    startPosition = transform.position;
  }

	void OnDespawned() {
    wheel.isKinematic = false;
    upper.isKinematic = false;
    lower.isKinematic = false;
    isComplete = false;
	}

  void SetRotation(Orientations orientation) {
    if (orientation == Orientations.Random) {
      var orientations = Enum.GetValues(typeof(Orientations));
      cart.localRotation = Quaternion.Euler(0f, 0f,
        (float)(Orientations)orientations.GetValue(UnityEngine.Random.Range(0, orientations.Length)));
    } else {
      cart.localRotation = Quaternion.Euler(0f, 0f, (float)orientation);
    }
  }

  public void BeginTrial(Orientations orientation, float startTime) {
    this.startTime = startTime;
    this.orientation = orientation;
    this.dying = false;
    this.dyingCount = 0;

    SetRotation(orientation);

    currentTrial = new Trial(orientation, startTime);

    if (Phenotype != null) {
      Phenotype.AddTrial(currentTrial);
    }
  }

  void EndTrial() {
    currentTrial.End(Time.time);

    if (Phenotype != null) {
      wheel.isKinematic = true;
      upper.isKinematic = true;
      lower.isKinematic = true;
      isComplete = true;
    }
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    // Consider 30 seconds to be solved
    var duration = Time.time - startTime;
    if (duration > 30.0f) {
      EndTrial();
    }

    // End if it fell off the edge
    if (wheel.transform.position.y - startPosition.y < -2.0f) {
      EndTrial();
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var thetaUpper = AngleHelper.GetAngle(upper.rotation);
    var thetaDotUpper = AngleHelper.GetAngle(upper.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude; // controllerBehaviour.Speed;

    // End if it went out of bounds
    if (Mathf.Abs(x) > 14.0f) {
      EndTrial();
      return;
    }

    var upperFell = NumberHelper.Between(Mathf.Abs(thetaUpper), 110.0f, 250.0f);
    var fellOver = upperFell;

    // If it fell over, start the dying clock
    if (!dying && fellOver) {
      currentTrial.Reset(Time.time);

      dyingStart = Time.time;
      dyingCount++;
      dying = true;
    }

    if (dying && !fellOver) {
      dying = false;
    }

    // If it's dying and has been for 5 seconds, end the trial
    if (dying) {
      var dyingDuration = Time.time - dyingStart;
      if (dyingDuration > 5.0f || dyingCount >= 6) {
        EndTrial();
        return;
      }
    }

    currentTrial.Update(thetaLower, thetaDotLower,
      thetaUpper, thetaDotUpper,
      x, xDot);
	}
}

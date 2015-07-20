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

  Vector3 startPosition;
  float startTime;

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
	}

  void Start() {
    SetRotation(orientation);
    startPosition = transform.position;
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

    SetRotation(orientation);
    Phenotype.BeginTrial(orientation, startTime);
  }

  void EndTrial() {
    wheel.isKinematic = true;
    upper.isKinematic = true;
    lower.isKinematic = true;
    isComplete = true;

    if (Phenotype != null) {
      Phenotype.EndTrial(Time.time);
    }
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    // Consider 30 seconds to be solved
    if (Time.time - startTime > 30.0f) {
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
    var xDot = wheel.velocity.magnitude;

    // End if it fell over
    if (NumberHelper.Between(thetaLower, -190.0f, -170.0f)) {
      EndTrial();
      return;
    }

    if (Phenotype != null) {
      Phenotype.UpdateTrial(thetaLower, thetaDotLower,
        thetaUpper, thetaDotUpper,
        x, xDot);
    }
	}
}

using UnityEngine;
using System;
using System.Collections;
using System.Linq;

// Responsible for initializing the test and measuring fitness.
public class EvaluationBehaviour : MonoBehaviour {

  public Orientations orientation;

  Transform cart;
  Transform handle;
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
    handle = transform.Find("Cart/Upper/Handle");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    upper = transform.Find("Cart/Upper").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();

    SetRotation(orientation);
    startPosition = transform.position;
	}

  void OnSpawned() {
    startPosition = transform.position;
  }

	void OnDespawned() {
    wheel.isKinematic = false;
    lower.isKinematic = false;
    isComplete = false;
	}

  void SetRotation(Orientations orientation) {
    if (orientation == Orientations.Random) {
      cart.localRotation = Quaternion.Euler(0f, 0f,
        UnityEngine.Random.value > 0.5f ? 210f : 150f);
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
    lower.isKinematic = true;
    isComplete = true;
    Phenotype.EndTrial(Time.time);
  }

	void FixedUpdate() {
    if (Phenotype == null) {
      Debug.LogWarning("Phenotype is null");
      return;
    }

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

    // End if it fell over
    if (handle.position.y - startPosition.y < 0.0f) {
      EndTrial();
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var thetaUpper = AngleHelper.GetAngle(upper.rotation);
    var thetaDotUpper = AngleHelper.GetAngle(upper.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    Phenotype.UpdateTrial(thetaLower, thetaDotLower,
      thetaUpper, thetaDotUpper,
      x, xDot);
	}
}

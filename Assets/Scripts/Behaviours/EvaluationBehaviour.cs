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

  Trial currentTrial;

  public Trial CurrentTrial {
    get { return currentTrial; }
  }

  public Rigidbody2D Wheel {
    get { return wheel; }
  }

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

    SetRotation(orientation);

    currentTrial = new Trial(orientation, startTime);

    if (Phenotype != null) {
      Phenotype.AddTrial(currentTrial);
    }
  }

  void EndTrial() {
    wheel.isKinematic = true;
    upper.isKinematic = true;
    lower.isKinematic = true;
    isComplete = true;

    currentTrial.End(Time.time);
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
    var xDot = wheel.velocity.magnitude; // speed?

    // End if it went out of bounds
    if (Mathf.Abs(x) > 14.0f) {
      EndTrial();
      return;
    }

    var upperFell = NumberHelper.Between(Mathf.Abs(thetaUpper), 110.0f, 250.0f);
    var lowerFell = NumberHelper.Between(Mathf.Abs(thetaLower), 110.0f, 250.0f);
    if (upperFell || lowerFell) {
      EndTrial();
      return;
    }

    currentTrial.Update(thetaLower, thetaDotLower,
      thetaUpper, thetaDotUpper,
      x, xDot);
	}
}

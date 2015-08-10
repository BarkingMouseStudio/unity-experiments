using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Responsible for sending inputs and applying torque,
// controlled by the neural network.
[RequireComponent(typeof(EvaluationBehaviour))]
public class ControllerBehaviour : MonoBehaviour {

  public TextAsset json;

  private NetworkIO networkIO;
  public NetworkIO Network {
    get {
      return networkIO;
    }
    set {
      networkIO = value;
    }
  }

  WheelJoint2D wheelJoint;
  Rigidbody2D upper;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  EvaluationBehaviour evaluation;

  void Awake() {
    upper = transform.Find("Cart/Upper").GetComponent<Rigidbody2D>();
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();
    evaluation = GetComponent<EvaluationBehaviour>();

    if (json != null) {
      var genotype = NEAT.Genotype.FromJSON(JSON.Deserialize(json.text));
      Assert.AreEqual(JSON.Serialize(genotype.ToJSON()), json.text.Trim(),
        "JSON should be compatible round-trip");

      networkIO = NetworkIO.FromGenotype(genotype);
    }
  }

  void OnDespawned() {
    this.speed = 0.0f;

    // Reset motor speed
    SetMotorSpeed(this.speed);
  }


  void SetMotorSpeed(float speed) {
    var motor = wheelJoint.motor;
	  motor.motorSpeed = speed;
  	wheelJoint.motor = motor;
  }

  public float speed = 0.0f;

	void FixedUpdate() {
    if (evaluation.IsComplete) {
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var thetaUpper = AngleHelper.GetAngle(upper.rotation);
    var thetaDotUpper = AngleHelper.GetAngle(upper.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    this.speed = 0.0f;
    if (networkIO != null) {
      this.speed = networkIO.Send(
        thetaLower, thetaDotLower,
        thetaUpper, thetaDotUpper,
        x, xDot);
    }

    // Update motor speed
    SetMotorSpeed(this.speed);
	}
}

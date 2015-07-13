using UnityEngine;
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
  Rigidbody2D lower;
  Rigidbody2D wheel;

  EvaluationBehaviour evaluation;

  void Awake() {
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();
    evaluation = GetComponent<EvaluationBehaviour>();

    if (json != null) {
      var genotype = NEAT.Genotype.FromJSON(json.text);
      networkIO = NetworkIO.FromGenotype(genotype);
    }
  }

  void OnDespawned() {
    // Reset motor speed
    SetMotorSpeed(0);
  }

  void SetMotorSpeed(float speed) {
    var motor = wheelJoint.motor;
	  motor.motorSpeed = speed;
  	wheelJoint.motor = motor;
  }

	void FixedUpdate() {
    if (evaluation.IsComplete) {
      return;
    }

    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    // var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    // var xDot = wheel.velocity.magnitude;

    float speed = 0.0f;
    if (networkIO != null) {
      speed = networkIO.Send(thetaLower, x);
    }

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

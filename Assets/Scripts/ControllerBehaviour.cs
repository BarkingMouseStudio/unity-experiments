using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Responsible for sending inputs and applying torque,
// controlled by the neural network.
public class ControllerBehaviour : MonoBehaviour {

  public NetworkIO networkIO;
  public TextAsset json;

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
      networkIO = new NetworkIO(Reifier.Reify(NEAT.Genotype.FromJSON(json.text)));
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

    var speed = networkIO.Send(thetaLower, x);

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

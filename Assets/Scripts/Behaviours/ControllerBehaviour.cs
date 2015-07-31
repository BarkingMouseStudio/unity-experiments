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

  public NetworkPorts Network { get; set; }

  float fastSpeed = 250.0f;
  float mediumSpeed = 50.0f;
  float slowSpeed = 15.0f;

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
      Network = NetworkPorts.FromGenotype(genotype);
    }
  }

  float speed = 0.0f;

  public float Speed {
    get {
      return speed;
    }
  }

  void OnDespawned() {
    speed = 0.0f;

    // Reset motor speed
    SetMotorSpeed(speed);
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

    speed = 0.0f;

    if (Network != null) {
      Network.UpperTheta.Set(AngleHelper.GetAngle(upper.rotation));
      Network.LowerTheta.Set(AngleHelper.GetAngle(lower.rotation));
      Network.Position.Set(wheel.transform.localPosition.x);

      Network.Tick();

      speed += (float)Network.SlowForward.Rate * slowSpeed;
      speed += (float)Network.MediumForward.Rate * mediumSpeed;
      speed += (float)Network.FastForward.Rate * fastSpeed;
      speed -= (float)Network.FastBackward.Rate * fastSpeed;
      speed -= (float)Network.MediumBackward.Rate * mediumSpeed;
      speed -= (float)Network.SlowBackward.Rate * slowSpeed;

      speed = Mathf.Clamp(speed, -fastSpeed, +fastSpeed);
    }

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

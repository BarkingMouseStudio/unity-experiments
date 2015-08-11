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

  WheelJoint2D wheelJoint;
  Rigidbody2D upper;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  EvaluationBehaviour evaluation;

  private float speed = 0.0f;
  public float Speed {
    get { return speed; }
  }

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

  void OnDespawned() {
    // Reset motor speed
    this.speed = 0.0f;
    SetMotorSpeed(this.speed);
  }

  void SetMotorSpeed(float localSpeed) {
    var motor = wheelJoint.motor;
	  motor.motorSpeed = localSpeed;
  	wheelJoint.motor = motor;
  }

	void FixedUpdate() {
    if (evaluation.IsComplete) {
      return;
    }

    if (Network != null) {
      var upperTheta = AngleHelper.GetAngle(upper.rotation);
      var lowerTheta = AngleHelper.GetAngle(lower.rotation);
      var position = wheel.transform.localPosition.x;

      Network.Clear();

      Network.UpperTheta.Set(upperTheta);
      Network.LowerTheta.Set(lowerTheta);
      Network.Position.Set(position);

      Network.Tick();

      this.speed = (float)Network.Speed.Get();

      // Update motor speed
      SetMotorSpeed(this.speed);
    }
	}
}

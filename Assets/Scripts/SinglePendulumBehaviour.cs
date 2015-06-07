using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SinglePendulumController : MonoBehaviour {

  static readonly double[][] thetaTable = new double[]{
    new double[]{-180.0, -90.0},
    new double[]{-45.0, -15.0},
    new double[]{-15.0, -5.0},
    new double[]{-5.0, -1.0},
    new double[]{-1.0, 0.0},
    new double[]{0.0, 1.0},
    new double[]{1.0, 5.0},
    new double[]{5.0, 15.0},
    new double[]{15.0, 45.0},
    new double[]{45.0, 90.0},
    new double[]{90.0, 180.0},
  };

  static readonly double[][] distanceTable = new double[]{
    new double[]{-6.0, -3.0},
    new double[]{-3.0, -1.0},
    new double[]{-1.0, 0.0},
    new double[]{0.0, 1.0},
    new double[]{1.0, 3.0},
    new double[]{3.0, 6.0},
  };

  static readonly double[] speedTable = new double[]{
    -1000.0,
    -100.0,
    -10.0,
    -1.0,
    1.0,
    10.0,
    100.0,
    1000.0,
  };

  WheelJoint2D wheelJoint;
  Rigidbody2D parent;
  Rigidbody2D lower;
  Rigidbody2D wheel;

  Neural.Network network;

  ulong[] inNeuronIds = new ulong[]{
    // 11,
    // 11,
    // 11,
    // 11,
    // 6
  };

  ulong[] outNeuronIds = new ulong[]{
    8
  };

  ulong neuronCount;

  double[] inputs;
  double[] outputs;

  void Awake() {
    parent = transform.Find("Cart").GetComponent<Rigidbody2D>();
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();
  }

  void Start() {
    // Choose random angle
    parent.transform.localRotation = Quaternion.Euler(0, 0, Random.value > 0.5 ? 185 : 175);
  }

  public void SetGenotype(List<List<double>> genotype) {
    network = new Neural.Network(20);

    var chromosomeN = genotype[0];
    double a, b, c, d;
    for (int i = 0; i < Mathf.FloorToInt(chromosomeN.Count / 4); i++) {
      a = chromosomeN[(i * 4) + 0];
      b = chromosomeN[(i * 4) + 1];
      c = chromosomeN[(i * 4) + 2];
      d = chromosomeN[(i * 4) + 3];
	    network.AddNeuron(a, b, c, d);
    }

    var chromosomeS = genotype[1];
    var synapseConfigs = chromosomeS.GetEnumerator();

    // Connect each input neuron to the outputs neuron.
    for (var i = 0; i < inNeuronIds.Length; i++) {
      for (var j = 0; j < outNeuronIds.Length; j++) {
        if (synapseConfigs.MoveNext()) {
          network.AddSynapse(inNeuronIds[i], outNeuronIds[j], synapseConfigs.Current, -20.0f, 20.0f);
        } else {
          UnityEngine.Debug.LogWarning("Ran out of synapses for the outputs neuron.");
        }
      }
    }

    while (synapseConfigs.MoveNext()) {
      UnityEngine.Debug.LogWarning("Unused synapse configs.");
    }

    neuronCount = network.NeuronCount;
  }

  void OnSpawned() {
    // Choose random angle
    parent.transform.localRotation = Quaternion.Euler(0, 0, Random.value > 0.5 ? 185 : 175);

    // Reset motor speed
    SetMotorSpeed(0);
  }

  void SetMotorSpeed(float speed) {
    var motor = wheelJoint.motor;
  	motor.motorSpeed = speed;
  	wheelJoint.motor = motor;
  }

	void FixedUpdate() {
    if (duration - now <= 0) {
      Complete();
      return;
    }

    if (wheel.transform.position.y < -2.0) {
      Complete();
      return;
    }

    // Send inputs
    var data = new float[]{
      AngleHelper.GetAngle(upper.rotation),
      AngleHelper.GetAngle(lower.rotation),
      AngleHelper.GetAngle(upper.angularVelocity),
      AngleHelper.GetAngle(lower.angularVelocity)
    };

    inputs = new double[neuronCount];

    for (var j = 0; j < data.Length; j++) {
      double angle = data[j];
      for (var i = 0; i < thetaTable.Length; i++) {
        double start = thetaTable[i][0];
        double end = thetaTable[i][1];
        if (angle >= start && angle < end) {
          inputs[i] = 20.0f * ((angle - start) / (end - start));
          break;
        }
      }
    }

    var wheelPosition = wheel.transform.localPosition.x;
    for (var i = 0; i < distanceTable.Length; i++) {
      double start = distanceTable[i][0];
      double end = distanceTable[i][1];
      if (wheelPosition >= start && wheelPosition < end) {
        inputs[i] = 20.0f * ((upperAngle - start) / (end - start));
        break;
      }
    }

    // Receive outputs
    var ticks = Time.fixedDeltaTime * 1000.0f;
    outputs = new double[neuronCount];
    network.Tick(ticks, inputs, ref outputs);

    // Read out neuron V for speed
    float speed = 0.0f;
    for (var i = 0; i < speedTable.Length; i++) {
      // 1. For each output neuron
      int outputNeuronId = outNeuronIds[i];
      // 2. Count their spikes
      int spikeCount = outputs[outputNeuronId] / 30.0f;
      // 3. Multiply by the speed
      speed += (float)(spikeCount * speedTable[outputNeuronId]);
    }

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

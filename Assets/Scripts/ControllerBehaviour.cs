using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ControllerBehaviour : MonoBehaviour {

  public enum Orientations {
    Upright = 0,
    SoftLeft = 1,
    SoftRight = 2,
    MediumLeft = 3,
    MediumRight = 4,
    HardLeft = 5,
    HardRight = 6,
    VeryHardLeft = 7,
    VeryHardRight = 8,
    Random = 9,
  }

  public struct Range {
    double start;
    double end;

    public Range(double start, double end) {
      this.start = start;
      this.end = end;
    }

    public bool Contains(double val) {
      return val >= start && val < end;
    }

    public double Scale(double val) {
      return (val - start) / (end - start);
    }

    public static Range Of(double start, double end) {
      return new Range(start, end);
    }
  }

  static readonly Range[] angularRanges = new Range[]{
    Range.Of(-180.0, -90.0),
    Range.Of(-90.0, -45.0),
    Range.Of(-45.0, -15.0),
    Range.Of(-15.0, -5.0),
    Range.Of(-5.0, -1.0),
    Range.Of(-1.0, 0.0),
    Range.Of(0.0, 1.0),
    Range.Of(1.0, 5.0),
    Range.Of(5.0, 15.0),
    Range.Of(15.0, 45.0),
    Range.Of(45.0, 90.0),
    Range.Of(90.0, 180.0),
  };

  static readonly Range[] linearRanges = new Range[]{
    Range.Of(-6.0, -3.0),
    Range.Of(-3.0, -1.0),
    Range.Of(-1.0, 0.0),
    Range.Of(0.0, 1.0),
    Range.Of(1.0, 3.0),
    Range.Of(3.0, 6.0),
  };

  static readonly double[] speeds = new double[]{
    -50.0, // 1000
    -5.0, // 100
    -0.5, // 10
    -0.05, // 1
    0.05,
    0.5,
    5.0,
    50.0,
  };

  public Orientations orientation;

  Range[] inputRanges;

  WheelJoint2D wheelJoint;
  Rigidbody2D lower;
  Rigidbody2D wheel;
  Transform cart;

  Neural.Network network;

  ulong[] inIds;
  ulong[] outIds;

  int neuronCount;

  double[] data;
  double[] input;
  double[] output;

  EvaluationBehaviour evaluation;

  void Awake() {
    cart = transform.Find("Cart");
    lower = transform.Find("Cart/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("Cart/Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();

    evaluation = GetComponent<EvaluationBehaviour>();

    // Set up receiving data
    data = new double[angularRanges.Length * 2 + linearRanges.Length * 2];

    // Set up input neuron ids by order
    inIds = new ulong[data.Length];
    for (var i = 0; i < inIds.Length; i++) {
      inIds[i] = (ulong)i;
    }
    AssertHelper.Assert(inIds.Length == 36, "Unexpected input neuron id count");
    // Debug.LogFormat("inIds: {0}", string.Join(", ", inIds.Select(x => x.ToString()).ToArray()));

    // Set up output neuron ids by order _after_ input neuron ids
    outIds = new ulong[speeds.Length];
    for (var i = 0; i < outIds.Length; i++) {
      outIds[i] = (ulong)(inIds.Length + i);
    }
    AssertHelper.Assert(outIds.Length == 8, "Unexpected output neuron id count");
    AssertHelper.Assert(outIds[0] == 36, "Unexpected first output neuron id");
    // Debug.LogFormat("outIds: {0}", string.Join(", ", outIds.Select(x => x.ToString()).ToArray()));

    List<Range> inputRanges = new List<Range>();
    inputRanges.AddRange(angularRanges); // theta lower
    inputRanges.AddRange(angularRanges); // theta dot lower
    inputRanges.AddRange(linearRanges); // x
    inputRanges.AddRange(linearRanges); // x dot
    this.inputRanges = inputRanges.ToArray();
  }

  void Start() {
    UpdateOrientation();
  }

  public void SetGenotype(Tuple<int, float[]>[] genotype) {
    network = new Neural.Network(20);

    // Input neurons
    for (int i = 0; i < inIds.Length; i++) {
      double a = genotype[i].Second[0]; // 0.1
      double b = genotype[i].Second[1]; // 0.2
      double c = genotype[i].Second[2]; // -65.0
      double d = genotype[i].Second[3]; // 2.0
	    network.AddNeuron(a, b, c, d);
    }

    // Output neurons
    for (int i = 0; i < outIds.Length; i++) {
      double a = genotype[inIds.Length + i].Second[0]; // 0.1
      double b = genotype[inIds.Length + i].Second[1]; // 0.2
      double c = genotype[inIds.Length + i].Second[2]; // -65.0
      double d = genotype[inIds.Length + i].Second[3]; // 2.0
	    network.AddNeuron(a, b, c, d);
    }

    // Connect each input neuron to the output neuron.
    for (var i = 0; i < inIds.Length; i++) {
      for (var j = 0; j < outIds.Length; j++) {
        double w = genotype[inIds.Length + outIds.Length + (i * outIds.Length) + j].Second[0];
        network.AddSynapse(inIds[i], outIds[j], w, -40.0f, 40.0f);
      }
    }

    neuronCount = (int)network.NeuronCount;
    AssertHelper.Assert(neuronCount == inIds.Length + outIds.Length,
      "Incorrect neuron count");
  }

  public void UpdateOrientation() {
    Quaternion rotation;

    switch (orientation) {
      case Orientations.SoftLeft:
        rotation = Quaternion.Euler(0, 0, 185);
        break;
      case Orientations.SoftRight:
        rotation = Quaternion.Euler(0, 0, 175);
        break;
      case Orientations.MediumLeft:
        rotation = Quaternion.Euler(0, 0, 195);
        break;
      case Orientations.MediumRight:
        rotation = Quaternion.Euler(0, 0, 165);
        break;
      case Orientations.HardLeft:
        rotation = Quaternion.Euler(0, 0, 210);
        break;
      case Orientations.HardRight:
        rotation = Quaternion.Euler(0, 0, 150);
        break;
      case Orientations.VeryHardLeft:
        rotation = Quaternion.Euler(0, 0, 225);
        break;
      case Orientations.VeryHardRight:
        rotation = Quaternion.Euler(0, 0, 135);
        break;
      case Orientations.Random:
        rotation = Quaternion.Euler(0, 0, Random.Range(135, 225));
        break;
      default:
        rotation = Quaternion.Euler(0, 0, 180);
        break;
    }

    cart.transform.localRotation = rotation;
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
    if (evaluation.isComplete) {
      return;
    }

    // Send input
    var thetaLower = AngleHelper.GetAngle(lower.rotation);
    var thetaDotLower = AngleHelper.GetAngle(lower.angularVelocity);
    var x = wheel.transform.localPosition.x;
    var xDot = wheel.velocity.magnitude;

    // Lower world data into array
    int i = 0;
    int len;
    for (len = angularRanges.Length; i < len; i++) {
      data[i] = thetaLower;
    }
    for (len += angularRanges.Length; i < len; i++) {
      data[i] = thetaDotLower;
    }
    for (len += linearRanges.Length; i < len; i++) {
      data[i] = x;
    }
    for (len += linearRanges.Length; i < len; i++) {
      data[i] = xDot;
    }
    // Debug.LogFormat("data: {0}", string.Join(", ", data.Select(d => d.ToString()).ToArray()));

    // Populate input array with data and ranges
    input = new double[neuronCount];

    Range range;
    for (i = 0; i < inIds.Length; i++) {
      range = inputRanges[i];
      if (range.Contains(data[i])) {
        input[inIds[i]] = 40.0f * range.Scale(data[i]);
      }
    }
    // Debug.LogFormat("input: {0}", string.Join(", ", input.Select(d => d.ToString()).ToArray()));

    // Receive output
    var ticks = (ulong)(Time.fixedDeltaTime * 1000.0f);
    output = new double[neuronCount];
    network.Tick(ticks, input, ref output);
    // Debug.LogFormat("output: {0}", string.Join(", ", output.Select(d => d.ToString()).ToArray()));

    // Read out neuron V for speed
    float speed = 0.0f;
    for (i = 0; i < outIds.Length; i++) {
      speed += (float)((output[outIds[i]] / 30.0) * speeds[i]);
    }
    // Debug.LogFormat("speed: {0}", speed);

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

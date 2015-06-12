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

  ulong[] inputNeuronIds;
  ulong[] outputNeuronIds;

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
    inputNeuronIds = new ulong[data.Length];
    for (var i = 0; i < inputNeuronIds.Length; i++) {
      inputNeuronIds[i] = (ulong)i;
    }
    AssertHelper.Assert(inputNeuronIds.Length == 36, "Unexpected input neuron id count");
    // Debug.LogFormat("inputNeuronIds: {0}", string.Join(", ", inputNeuronIds.Select(x => x.ToString()).ToArray()));

    // Set up output neuron ids by order _after_ input neuron ids
    outputNeuronIds = new ulong[speeds.Length];
    for (var i = 0; i < outputNeuronIds.Length; i++) {
      outputNeuronIds[i] = (ulong)(inputNeuronIds.Length + i);
    }
    AssertHelper.Assert(outputNeuronIds.Length == 8, "Unexpected output neuron id count");
    AssertHelper.Assert(outputNeuronIds[0] == 36, "Unexpected first output neuron id");
    // Debug.LogFormat("outputNeuronIds: {0}", string.Join(", ", outputNeuronIds.Select(x => x.ToString()).ToArray()));

    List<Range> inputRanges = new List<Range>();
    inputRanges.AddRange(angularRanges); // theta lower
    inputRanges.AddRange(angularRanges); // theta dot lower
    inputRanges.AddRange(linearRanges); // x
    inputRanges.AddRange(linearRanges); // x dot
    this.inputRanges = inputRanges.ToArray();

    SetGenotype(new List<List<double>>(){
      new List<double>(){-0.8294217414222658,0.19477468775585294,-2.2393326507881284,14.125457955524325,0.18598063988611102,0.7808822970837355,-6.874485593289137,14.719350757077336},
      new List<double>(){15.026902928948402,-4.589374614879489,19.551734989508986,-13.1247285194695,17.903378726914525,-19.633415192365646,2.3412004951387644,11.681403573602438,11.464269263669848,14.770395085215569,19.108972437679768,-13.618495902046561,-4.95998858474195,1.7212302796542645,15.524215931072831,-9.709464432671666,-11.28869871608913,-2.169554093852639,-6.323215430602431,-19.63263388723135,6.7388927191495895,19.868283569812775,18.41708991676569,-19.64711120352149,-2.880794433876872,-1.844737520441413,9.393150266259909,-2.1276062447577715,8.997647687792778,10.6798707973212,-8.67728192359209,7.005672603845596,16.76449529826641,14.566777441650629,-13.347157454118133,-3.1616279389709234,18.39989905245602,-18.350949669256806,-5.163826541975141,-4.684916138648987,-5.938819739967585,-5.126056428998709,10.259007550776005,-17.00067039579153,-5.946580562740564,17.60454404167831,8.573411330580711,-15.121607203036547,2.376664187759161,19.81023959815502,-2.3302238434553146,-10.878609474748373,14.673322020098567,-0.4401272349059582,-2.7651420142501593,-8.581447340548038,-5.729043772444129,-9.596073841676116,6.081902449950576,10.93519707210362,-16.401997404173017,4.137511011213064,-4.274160759523511,-17.407671930268407,-9.019048288464546,-6.47965251468122,-7.852118536829948,1.015173215419054,-9.25021130591631,11.687344461679459,-0.2377329021692276,19.18282525613904,11.70422401279211,-0.026919646188616753,-13.042672676965594,-13.06014934554696,-8.97200801409781,13.509885612875223,-17.439322154968977,15.986323589459062,-12.265309998765588,0.26805889792740345,18.30341306515038,-19.561660885810852,-1.498699663206935,8.466864405199885,-1.0622533597052097,-9.355549663305283,1.6469855234026909,13.637971729040146,7.642083754763007,4.364407630637288,-8.670940147712827,-3.225478446111083,19.79666404426098,19.590100720524788,16.310540223494172,11.430759113281965,2.6485189516097307,1.4315262623131275,-0.0075445231050252914,-19.012819910421968,19.90164987742901,13.49631043151021,-14.753486290574074,8.980680368840694,14.876499120146036,-0.9698146488517523,7.680529160425067,-17.072746930643916,-2.7136523742228746,3.6295463237911463,0.28455362655222416,3.8649035152047873,-13.839026493951678,4.573090933263302,-0.3498487360775471,5.055547310039401,-14.602320529520512,12.559417467564344,15.841611064970493,-7.151195118203759,10.830410225316882,-18.399907695129514,-19.51075816527009,-6.03701182641089,-4.885838218033314,4.495709910988808,-14.278874834999442,3.4689952805638313,-16.166550694033504,-12.904295036569238,-5.839744992554188,10.513354251161218,-6.417839163914323,15.195480566471815,-12.267581326887012,-4.563954472541809,0.36513213999569416,-6.229577613994479,7.475737035274506,5.2211017813533545,10.03000414930284,3.5270716063678265,10.141691379249096,4.045182345435023,-0.9688844252377748,15.813241945579648,-3.9588757790625095,-19.2277274094522,12.959998855367303,10.889434795826674,-10.95611859112978,1.9341415632516146,15.080159483477473,2.152215614914894,15.097439382225275,-5.503891594707966,-7.553933812305331,3.929266408085823,-4.964345199987292,8.543688729405403,-7.909182971343398,-3.1737975031137466,8.00970665179193,4.908337723463774,8.713432950899005,-13.482087543234229,-0.7500438205897808,14.836499691009521,-8.624397478997707,7.319318354129791,18.244274370372295,-19.43218607455492,2.910859165713191,-11.403329893946648,-15.40592210367322,-0.47013789415359497,2.353373095393181,0.08324580267071724,3.0343623645603657,6.934674838557839,-4.865828873589635,5.007232530042529,11.902392599731684,9.57733646966517,9.897709032520652,-15.449582282453775,13.609048509970307,-1.314011299982667,-2.938853604719043,12.583019435405731,14.058565432205796,15.076417038217187,18.615143252536654,3.7271882873028517,-9.295427529141307,-12.874953290447593,11.418339610099792,0.2985726483166218,-17.445293935015798,-19.22669457271695,-19.647515946999192,15.469988379627466,2.8728964552283287,15.692071113735437,17.674280228093266,6.043451149016619,-4.097034856677055,17.480915868654847,-14.960805019363761,-10.760030122473836,14.824406681582332,5.52517713047564,6.250005066394806,13.36677584797144,17.013816749677062,17.899128049612045,-9.301121011376381,10.69981419481337,-5.731492610648274,-8.831863170489669,9.57963396795094,8.728574877604842,15.490896264091134,-10.01468832604587,1.07184499502182,-12.628683215007186,0.10223033837974072,-15.401202160865068,11.961391372606158,5.723661053925753,-9.83807978220284,-7.2946100030094385,-17.27401544339955,-7.364470837637782,-10.783808510750532,-18.023066390305758,-13.01197899505496,-9.277124600484967,-2.9534996580332518,-17.308006677776575,9.28096099756658,-18.58274122700095,5.323491748422384,-3.5392283834517,11.08022628352046,-15.83462637849152,16.424998622387648,7.156095709651709,7.389671020209789,-3.6127053014934063,-17.851354824379086,2.9678019136190414,-13.738042758777738,4.560221908614039,5.465769115835428,1.5515632927417755,-2.090086741372943,7.141147498041391,14.556793766096234,-5.630377633497119,3.6335250549018383,-2.457541488111019,13.643534649163485,-13.125612838193774,-6.049984954297543,2.682983484119177,-18.877082988619804,16.60742837935686,-18.958854349330068,5.993896815925837,2.299280194565654,-19.574167551472783,-0.17298911698162556,-7.3200015258044,-3.00315298140049,-0.1516117248684168,-12.346358047798276,-7.573939263820648,-6.026311861351132,-0.8043278101831675,-15.69757541641593,2.5259019527584314,-7.8762332908809185,-2.5920370873063803,-3.2159026339650154,-0.06121549755334854},
    });
  }

  void Start() {
    UpdateOrientation();
  }

  public void SetGenotype(List<List<double>> genotype) {
    network = new Neural.Network(20);

    var chromosomeN = genotype[0];

    // Input neurons
    double a = chromosomeN[0]; // 0.1
    double b = chromosomeN[1]; // 0.2
    double c = chromosomeN[2]; // -65.0
    double d = chromosomeN[3]; // 2.0

    for (int i = 0; i < inputNeuronIds.Length; i++) {
	    network.AddNeuron(a, b, c, d);
    }

    // Output neurons
    a = chromosomeN[4];
    b = chromosomeN[5];
    c = chromosomeN[6];
    d = chromosomeN[7];

    for (int i = 0; i < outputNeuronIds.Length; i++) {
	    network.AddNeuron(a, b, c, d);
    }

    var chromosomeS = genotype[1];
    var synapseConfigs = chromosomeS.GetEnumerator();

    // Connect each input neuron to the output neuron.
    for (var i = 0; i < inputNeuronIds.Length; i++) {
      for (var j = 0; j < outputNeuronIds.Length; j++) {
        // Debug.LogFormat("{0} => {1}", inputNeuronIds[i], outputNeuronIds[j]);
        if (synapseConfigs.MoveNext()) {
          network.AddSynapse(inputNeuronIds[i], outputNeuronIds[j], synapseConfigs.Current, -40.0f, 40.0f);
        } else {
          UnityEngine.Debug.LogWarning("Ran out of synapses for the output neuron.");
        }
      }
    }

    while (synapseConfigs.MoveNext()) {
      UnityEngine.Debug.LogWarning("Unused synapse configs.");
    }

    neuronCount = (int)network.NeuronCount;
    AssertHelper.Assert(neuronCount == inputNeuronIds.Length + outputNeuronIds.Length,
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
    for (i = 0; i < inputNeuronIds.Length; i++) {
      range = inputRanges[i];
      if (range.Contains(data[i])) {
        input[inputNeuronIds[i]] = 40.0f * range.Scale(data[i]);
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
    for (i = 0; i < outputNeuronIds.Length; i++) {
      speed += (float)((output[outputNeuronIds[i]] / 30.0) * speeds[i]);
    }
    // Debug.LogFormat("speed: {0}", speed);

    // Update motor speed
    SetMotorSpeed(speed);
	}
}

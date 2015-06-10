using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ControllerBehaviour : MonoBehaviour {

  public enum Orientations {
    Upright = 0,
    Left = 1,
    Right = 2,
    Random = 3,
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
      new List<double>(){-0.4708830821327865,-0.40216046338900924,-76.69602364767343,-8.750882092863321,0.5771760689094663,0.3901979881338775,-23.83536659181118,7.456475906074047},
      new List<double>(){12.981784930452704,19.83596920967102,-10.94869620166719,18.263610629364848,12.369092665612698,-14.716457724571228,8.99636160582304,-15.136631336063147,14.823012314736843,-18.000924484804273,-4.365747245028615,-16.523060062900186,-8.414026582613587,6.344278119504452,7.035438409075141,-3.7059079948812723,6.8009397853165865,-9.242418687790632,-1.7109829932451248,10.116454157978296,14.160690875723958,-3.0677882954478264,-16.375129353255033,10.962699633091688,-0.7808986492455006,12.851855782791972,1.9601234421133995,-18.704237639904022,12.692171223461628,-5.111449547111988,3.124598925933242,16.123148296028376,-17.11714716628194,-14.525202987715602,11.649622926488519,-13.192135216668248,-11.125060673803091,-14.149053739383817,14.692730596289039,-4.4577790424227715,-11.503340331837535,-6.544873071834445,-1.9105644337832928,3.8521732855588198,-8.663147184997797,-18.380087576806545,-16.89868589863181,8.612017137929797,-2.1058174781501293,-0.8669088315218687,-3.358630472794175,-9.37170997262001,-8.97026733495295,7.997209196910262,9.08013571985066,-0.8489534631371498,14.348811246454716,0.1379324123263359,-1.0079401638358831,-19.065455431118608,-19.531346391886473,14.576801471412182,-5.0198146142065525,-3.40638211928308,-11.612771023064852,-2.631100360304117,1.953293802216649,14.295939709991217,-9.388042483478785,14.822945715859532,12.44913692586124,5.3611003793776035,5.443616406992078,-10.884165139868855,-3.536100247874856,-17.674404187127948,-2.653828663751483,16.451113475486636,10.408133827149868,-9.663627296686172,-16.243740152567625,18.161671655252576,-11.650601085275412,-8.100301623344421,-19.678783742710948,9.771759305149317,-1.1156737431883812,-19.298893390223384,-11.143109137192369,-19.28712318651378,-2.032769899815321,9.70837702974677,3.8972044456750154,19.895509826019406,-11.645348947495222,12.759953616186976,-5.165298571810126,-18.31638745032251,-16.01599410176277,15.801236387342215,8.786557707935572,-3.243932342156768,10.075316978618503,-13.51489401422441,8.52297406643629,-18.34098138846457,-18.317718356847763,-1.9956345204263926,10.308887232095003,18.414802309125662,13.075983542948961,-19.553720727562904,-17.968014013022184,-11.690770210698247,5.730726756155491,-2.5975889060646296,-19.798493376001716,-14.67272199690342,3.040083721280098,-16.803404251113534,-5.554998824372888,6.702911602333188,5.769342323765159,-14.832174954935908,8.095011785626411,-8.082207273691893,0.4152420349419117,-12.66839250922203,-5.023017916828394,13.23218772187829,-10.93230877071619,-2.4480253644287586,-11.970544094219804,-17.9947610758245,19.693083129823208,10.622829161584377,-2.919690264388919,12.806339720264077,4.891974674537778,0.07416464388370514,-3.5521250031888485,15.404507387429476,2.841462269425392,6.8727766908705235,-11.368164345622063,-7.430021446198225,-13.656066320836544,16.156748635694385,-8.383497912436724,13.103689281269908,6.010639052838087,-7.631115596741438,1.867380030453205,0.3353717364370823,9.068961376324296,-3.7378583755344152,16.261377669870853,-18.463087799027562,-6.751019926741719,-2.3966852575540543,6.043099677190185,-9.693228779360652,-3.9552079420536757,0.5782019905745983,6.321144998073578,-14.269775217399001,11.27222316339612,14.353909939527512,12.740518944337964,16.34217477403581,-10.524911135435104,-2.5601206440478563,15.999492695555091,-2.75180296972394,1.3672042172402143,-12.573355101048946,3.95070843398571,-16.01059994660318,2.385868364945054,-1.9898068066686392,-3.541490649804473,4.4292303547263145,10.423862971365452,-3.038498079404235,-2.0755500812083483,-6.467855982482433,17.143118800595403,16.794654866680503,15.041299294680357,12.50725083053112,-15.047454917803407,4.133709324523807,9.186163917183876,18.687761584296823,9.286442883312702,11.830815561115742,-14.124984852969646,16.05847098864615,-4.139833934605122,-0.4333796724677086,-5.328219486400485,0.16607307828962803,-2.305856551975012,-10.237693022936583,5.281595829874277,-10.766334952786565,-11.06093768030405,-2.412252575159073,-5.135013712570071,3.9480989053845406,-18.548554368317127,-11.597800431773067,-8.028847659006715,5.231512086465955,-4.692625664174557,11.971219647675753,-12.843430740758777,-2.6989136543124914,3.6323431227356195,16.053998367860913,-8.412237549200654,8.88633955270052,19.089499469846487,-12.15371006168425,2.1240364108234644,14.468382699415088,15.792243666946888,-18.855544673278928,-18.231119960546494,-18.41979150660336,15.491754207760096,-6.431828290224075,0.06537161767482758,-2.850608928129077,-9.853566577658057,-19.66240587644279,-9.586900770664215,9.491181056946516,-8.66076773032546,-12.19566110521555,-15.166692798957229,11.267042011022568,-6.273026382550597,18.655583346262574,13.869342682883143,-0.6459829583764076,-1.534845419228077,11.282633785158396,13.042648816481233,16.19829543866217,-2.538289688527584,-15.043115029111505,-1.9907086435705423,-19.554106816649437,6.678243838250637,-15.404843129217625,-13.123086653649807,1.6154812648892403,-7.365845330059528,1.0236205998808146,-12.9736253246665,18.6697964835912,-11.481171827763319,12.562566231936216,-7.523554107174277,6.614251211285591,16.88940128311515,-19.18682756833732,6.020707972347736,12.966219540685415,9.458764651790261,6.521145552396774,6.375639075413346,-19.99991330318153,-12.645443752408028,1.94433624856174,4.209374189376831,-3.1101078540086746,11.296256063506007,12.430685944855213,-10.210902215912938,-19.879758693277836,19.861670369282365,8.312952490523458,-16.803980208933353,-15.237452564761043,-3.7609403766691685,-3.1751584634184837},
    });
  }

  void Start() {
    SetOrientation();
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

  void SetOrientation() {
    Quaternion rotation;

    switch (orientation) {
      case Orientations.Left:
        rotation = Quaternion.Euler(0, 0, 185);
        break;
      case Orientations.Right:
        rotation = Quaternion.Euler(0, 0, 175);
        break;
      case Orientations.Random:
        rotation = Quaternion.Euler(0, 0, Random.value > 0.5 ? 185 : 175);
        break;
      default:
        rotation = Quaternion.Euler(0, 0, 180);
        break;
    }

    cart.transform.localRotation = rotation;
  }

  void OnSpawned() {
    SetOrientation();
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

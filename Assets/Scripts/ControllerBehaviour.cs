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
      new List<double>(){0.443986221216619,0.19896818464621902,-82.26224679965526,-11.375673664733768,0.6609883708879352,0.21186699578538537,-67.3255562549457,3.7246490735560656},
      new List<double>(){6.283921897411346,-16.2404228374362,-1.6271031089127064,-12.727378606796265,-7.428777748718858,8.270693933591247,6.032925909385085,3.6808137223124504,19.571955166757107,-2.7711760252714157,5.469321291893721,15.272554494440556,-3.6811079178005457,15.215693535283208,-3.4570342395454645,-11.66617157869041,-12.952243154868484,5.48513769172132,-17.589312652125955,-9.878367567434907,10.110465195029974,-19.64655589312315,-0.07394718937575817,2.467857161536813,16.949122520163655,-7.352936957031488,-0.48878544941544533,-9.99012976884842,13.887386303395033,5.955695938318968,-11.178459767252207,-19.117405703291297,-10.36856452934444,13.86227011680603,17.425618823617697,-13.718893667683005,-5.666551161557436,6.5724424459040165,19.00608171708882,19.24635368399322,16.977369859814644,-9.638965371996164,-8.293889258056879,5.395697634667158,-4.621768118813634,-14.24020390957594,-16.43683890812099,1.3989385310560465,3.5643030144274235,8.065681736916304,-18.29427434131503,-10.820324644446373,-10.247324472293258,-15.539616849273443,-2.644835691899061,-15.65963483415544,13.49557999521494,4.177863271906972,-8.689833879470825,10.981142790988088,15.950680011883378,-11.212322507053614,-0.8957247249782085,-10.0865348149091,11.607471331954002,18.49190249107778,-10.13684630393982,11.31782939657569,11.97793454863131,-5.696610687300563,-12.757355803623796,4.941945532336831,16.335774390026927,-15.382613660767674,2.229122770950198,11.876041861250997,-15.516532259061933,-16.104027340188622,10.556523818522692,11.427076235413551,14.126425217837095,12.732010940089822,-19.86249804496765,18.541752817109227,4.477717820554972,-11.182353030890226,-17.599649904295802,-4.125482216477394,6.699802950024605,19.250157456845045,0.4635635670274496,-3.9636453799903393,7.2690225020051,-14.998612962663174,14.909170139580965,-5.395729867741466,6.466196216642857,-1.5324240643531084,2.311909571290016,1.3861592300236225,14.579674899578094,16.577563052996993,3.874790072441101,9.151165122166276,-17.102320520207286,11.201208783313632,-0.17831316217780113,-19.116156501695514,15.00467386096716,13.317171605303884,-15.087677566334605,-11.73556693829596,-7.631785348057747,-14.23373093828559,-14.59382874891162,3.32265286706388,-12.963671144098043,-2.2419302631169558,-13.991319788619876,14.63168335147202,0.29077972285449505,-1.5375274419784546,-19.786796662956476,-2.470893766731024,1.7480497527867556,-13.051943676546216,-19.859487554058433,-12.882939502596855,-16.515641417354345,3.046346791088581,-14.63451093994081,-19.107084721326828,7.115150289610028,10.234914775937796,-19.70550874248147,4.154537282884121,-12.912361584603786,14.83783551491797,10.29040526598692,-2.63310338370502,3.448622105643153,19.032938526943326,-10.894519863650203,10.652826456353068,15.046975463628769,-8.82896956987679,-8.861731821671128,-17.778420178219676,8.70584829710424,-4.7965066228061914,-3.062632130458951,-1.284372266381979,7.754271719604731,12.347341049462557,-19.153297003358603,18.166585294529796,-3.2380229141563177,-6.932934727519751,-1.7318955343216658,-8.439999436959624,10.891320900991559,16.53566907159984,4.44555732421577,-2.848005509003997,-4.020802089944482,9.120653877034783,-16.951861828565598,5.972572918981314,6.89884333871305,-11.890438990667462,5.648329881951213,11.698346743360162,4.669601824134588,8.937157150357962,-11.567520136013627,-13.87043853290379,-19.683628547936678,-1.060627093538642,16.254245499148965,8.447922179475427,18.145782509818673,-14.742515906691551,11.029682476073503,-13.50039859302342,-14.69260579906404,-3.2518091797828674,18.5401130374521,-13.196535278111696,-18.427376467734575,-8.092714929953218,-7.732950774952769,15.391742028295994,7.463746769353747,0.5570131912827492,14.339317977428436,-17.917544785887003,-7.698599733412266,-2.782137868925929,-2.887623840942979,-10.977994799613953,-3.4142358601093292,12.890347326174378,-16.13808646798134,-6.088857296854258,19.206713316962123,5.037843007594347,-12.585149705410004,-6.932051395997405,6.814986299723387,6.376278921961784,-8.764712251722813,-0.21955749951303005,-11.930754473432899,-12.431539809331298,-1.118912510573864,-12.407479155808687,-3.1391251645982265,-8.678603926673532,9.693805174902081,14.373202230781317,9.195121796801686,-11.162392450496554,-8.237135829403996,2.1954583562910557,0.4558551125228405,6.2924583069980145,-18.492703288793564,3.9749669190496206,-11.542505156248808,2.016728762537241,11.718710903078318,11.704111713916063,-2.1085209120064974,-4.616621490567923,4.043714599683881,12.609981903806329,-13.264712765812874,-18.058750545606017,8.272181395441294,-3.471776619553566,-1.2726411875337362,19.54622084274888,7.423461517319083,13.254191419109702,-5.955301634967327,18.745834659785032,-15.404801303520799,-9.758142912760377,7.851277031004429,-16.69599236920476,-1.2938134744763374,-3.3413136284798384,-17.207905147224665,-1.7429379187524319,-12.05819733440876,16.176699930801988,-11.043344615027308,8.535255305469036,5.4071778897196054,11.976794907823205,10.349494572728872,-1.594808492809534,-14.20677151530981,-2.180194230750203,-7.88948580622673,-9.269088851287961,-16.536997677758336,13.700683014467359,-8.674222081899643,-16.387122133746743,-2.5066346675157547,-4.17522206902504,-2.025767946615815,16.755085652694106,4.754264121875167,0.6077900435775518,-15.908207884058356,-16.002687457948923,-18.162471959367394,-8.493547094985843,-3.997033406049013,2.5117928255349398,18.868226800113916,17.54675241187215,-4.038432398810983,11.371565582230687,-4.861617041751742,4.314701650291681},
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

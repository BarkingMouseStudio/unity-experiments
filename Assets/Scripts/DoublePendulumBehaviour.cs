using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DoublePendulumBehaviour : MonoBehaviour {

  const float TWO_PI = 2.0f * Mathf.PI;
  const float maxDistance = 12.0f;

  protected internal bool isComplete = false;

  Rigidbody2D upper;
  Rigidbody2D lower;
  Transform wheel;

  Neural.Network network;

  ulong[] inNeuronIds = new ulong[]{0, 1, 2, 3, 4, 5};
  ulong outNeuronId = 6;
  ulong neuronCount;

  double[] inputs;
  double[] outputs;

  float duration = 5.0f;
  float now = 0.0f;

  float[] fitnessHistory = new float[100];
  int fitnessIndex = 0;
  int fitnessCount = 0;

  void Start() {
    upper = transform.Find("DoublePendulum/Upper").GetComponent<Rigidbody2D>();
    lower = transform.Find("DoublePendulum/Lower").GetComponent<Rigidbody2D>();
    wheel = transform.Find("DoublePendulum/Wheel");

    SetGenotype(new List<List<double>>(){
      new List<double>(){-0.5385752688162029,-0.9255957440473139,-44.19480683282018,6.2535954266786575,-0.7033140370622277,-0.5770359211601317,-57.52903721295297,-0.9288753848522902,0.43941365787759423,-0.698256925214082,-4.373402474448085,-17.294252393767238,-0.6450817119330168,0.3411713968962431,-60.59825918637216,12.814581776037812,0.44216878712177277,-0.7366140903905034,-59.46672565769404,13.447362845763564,0.12321440689265728,0.13246938399970531,-9.637254918925464,-7.566007636487484,0.14826317923143506,-0.6105808513239026,-11.725192097947001,-6.643243590369821,-0.09416186343878508,0.22772831795737147,-49.92104994598776,7.766789244487882,-0.8842157702893019,-0.8492906247265637,-15.168529679067433,-0.6752384547144175,-0.4582015499472618,-0.00789797818288207,-44.74742063321173,-5.517514767125249,0.39408281026408076,0.9914421499706805,-11.424846365116537,-17.30797686614096,-0.6737327626906335,-0.6843529255129397,-56.73197731375694,-5.74247183278203,-0.6994412890635431,-0.4807081618346274,-36.874011415056884,4.359477683901787,0.07198500819504261,0.8788539757952094,-36.77029078826308,18.708208026364446,-0.22103716665878892,0.627336404286325,-5.649316986091435,-16.560928877443075,0.47689821338281035,-0.12481874926015735,-97.92044907808304,18.957414804026484,-0.06595144420862198,-0.7064349269494414,-76.928795943968,-19.155415194109082,-0.2021382600069046,-0.01954039977863431,-64.36554789543152,-0.2649354748427868,0.2854766845703125,-0.6276519354432821,-16.978184692561626,7.581885755062103,0.05340124620124698,-0.2655360265634954,-99.79897213634104,-14.384626420214772,-0.9188301474787295,0.14246641797944903,-41.47835043258965,-3.41466311365366,0.11453544069081545,0.6889300644397736,-99.46856256574392,-17.718174941837788,-0.5098010879009962,0.2963727037422359,-83.08381715323776,-15.732864663004875},
      new List<double>(){7.730015651322901,3.818238670937717,2.7302639838308096,-3.164940709248185,1.2070935824885964,5.96092252060771,-0.490156183950603,-7.524066520854831,-3.513714144937694,8.134261243976653,5.4993196576833725,-2.7390370005741715,-0.7121254922822118,3.4042331855744123,1.4618676342070103,-4.6959857596084476,-5.179611528292298,2.1754597639665008,-7.891964404843748,1.7573540564626455,-5.633244090713561,-9.640511027537286,6.367279156111181,-6.7992431204766035,-4.439303143881261,-9.30153681896627,6.348322848789394,-6.87021893914789,7.389279003255069,4.929600055329502,-3.652837867848575,3.1561221787706017,-4.668783349916339,-8.40126620605588,2.4672056920826435,-6.5254090912640095,-1.1207828484475613,-6.431038100272417,-1.7778645968064666,3.257493181154132,-8.573598237708211,9.786450094543397,-4.865685543045402,3.590783542022109,-8.286153189837933,0.1314229005947709,-8.848130567930639,4.509033034555614,6.040384201332927,-6.896976516582072,2.150791995227337,-7.03567108605057,2.3748209746554494,8.84808249771595,9.12580634932965,-0.5336895631626248,-4.0775845013558865,6.629859427921474,4.4621166586875916,4.125510584563017,-8.726750053465366,5.060583045706153,-2.2316971933469176,7.270108601078391,-2.5443999469280243,-8.002863484434783,-8.002542280592024,8.599427076987922,-6.048243008553982,2.6822379929944873,4.63582688011229,2.0989973517134786,-0.6679600570350885,7.765580401755869,-3.1215607281774282,4.228555262088776,-4.59803806617856,4.2474322859197855,2.6081088231876493,0.7866918575018644,-2.8727707313373685,0.14160974882543087,1.348760207183659,-4.719876232556999,-1.570470379665494,8.768559987656772,6.506765130907297,-2.063959646038711,-9.446970401331782,-8.786419155076146,6.891898293979466,-5.262256050482392,8.604455031454563,9.515022770501673,8.012359971180558,-6.123797809705138,7.916254177689552,-4.28650189191103,-2.5359570933505893,4.916329933330417,-6.208636648952961,2.3691791156306863,-9.90437010768801,-6.68970497790724,4.13208591286093,-6.365142627619207,-9.735054839402437,0.2429922390729189,-2.2033374197781086,-9.884018655866385,-8.282288857735693,-6.948266034014523,-2.873623678460717},
    });
  }

  public float Fitness {
    get {
      float fitness;
      if (fitnessCount > fitnessHistory.Length) {
        fitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
      } else {
        fitness = 100.0f; // Worst case, didn't live long enough
      }
      return fitness * fitness; // Square it!
    }
  }

  public void SetGenotype(List<List<double>> genotype) {
    network = new Neural.Network(20);

    var chromosomeN = genotype[0];
    for (int i = 0; i < Mathf.FloorToInt(chromosomeN.Count / 4); i++) {
	    network.AddNeuron(
        chromosomeN[(i * 4) + 0],
        chromosomeN[(i * 4) + 1],
        chromosomeN[(i * 4) + 2],
        chromosomeN[(i * 4) + 3]
      );
    }

    var chromosomeS = genotype[1];
    var synapseConfigs = chromosomeS.GetEnumerator();

    int hiddenNeuronCount = 16;

    // Connect each input neuron to each hidden neuron.
    for (var i = 0; i < inNeuronIds.Length; i++) {
      for (var j = 0; j < hiddenNeuronCount; j++) {
        if (synapseConfigs.MoveNext()) {
          network.AddSynapse(inNeuronIds[i], outNeuronId + (ulong)j + 1, synapseConfigs.Current, -100.0f, 100.0f);
        } else {
          UnityEngine.Debug.LogWarning("Ran out of synapses for hidden neurons.");
        }
      }
    }

    // Connect each hidden neuron to the outputs neuron.
    for (var j = 0; j < hiddenNeuronCount; j++) {
      if (synapseConfigs.MoveNext()) {
        network.AddSynapse(outNeuronId + (ulong)j + 1, outNeuronId, synapseConfigs.Current, -10.0f, 10.0f);
      } else {
        UnityEngine.Debug.LogWarning("Ran out of synapses for the outputs neuron.");
      }
    }

    // Connect the outputs neuron to the last input neuron (recurrent).
    if (synapseConfigs.MoveNext()) {
      network.AddSynapse(outNeuronId, inNeuronIds[inNeuronIds.Length - 1], synapseConfigs.Current, -10.0f, 10.0f);
    } else {
      UnityEngine.Debug.LogWarning("Ran out of synapses for the recurrent input neuron.");
    }

    if (synapseConfigs.MoveNext()) {
      UnityEngine.Debug.LogWarning("Unused synapse configs.");
    }

    neuronCount = network.NeuronCount;
  }

  void OnDespawned() {
    isComplete = false;
  }

  void Complete() {
    now = 0.0f;
    isComplete = true;
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (duration - now <= 0) {
      Complete();
      return;
    }

    var upperAngle = Quaternion.Angle(Quaternion.identity, upper.transform.rotation) * Mathf.Deg2Rad;
    var lowerAngle = Quaternion.Angle(Quaternion.identity, lower.transform.rotation) * Mathf.Deg2Rad;
    var upperAngularVelocity = upper.angularVelocity * Mathf.Deg2Rad;
    var lowerAngularVelocity = lower.angularVelocity * Mathf.Deg2Rad;
    var relativeWheelDistance = Vector2.Distance(Vector2.zero, wheel.localPosition) / maxDistance;

    // Send inputs
    inputs = new double[neuronCount];
    inputs[0] = upperAngle;
    inputs[1] = lowerAngle;
    inputs[2] = upperAngularVelocity;
    inputs[3] = lowerAngularVelocity;
    inputs[4] = relativeWheelDistance;
    // UnityEngine.Debug.LogFormat("{0}", string.Join(",", inputs.Select(i => i.ToString()).ToArray()));

    // Receive outputs
    var ticks = Time.fixedDeltaTime * 1000.0f;
    outputs = new double[neuronCount];
    network.Tick((ulong)ticks, inputs, ref outputs);
    // UnityEngine.Debug.LogFormat("{0}", string.Join(",", outputs.Select(o => o.ToString()).ToArray()));

    // Read out neuron V for torque
    var V = outputs[outNeuronId]; // Default spike voltage
    var T = (float)(V / 10.0);
    if (T.IsFinite()) {
      lower.AddTorque(T);
    }

    // Add noise to upper pendulum
    upper.AddTorque(UnityEngine.Random.Range(-1, 1));

    // Record history
    float currentFitness = Mathf.Abs(lowerAngle) * 1 + Mathf.Abs(upperAngle) * 1 +
                Mathf.Abs(lowerAngularVelocity) * 1 + Mathf.Abs(upperAngularVelocity) * 1 +
                Mathf.Abs(T) * 1 + Mathf.Abs(relativeWheelDistance) * 1;
    fitnessHistory[fitnessIndex] = currentFitness;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;

    fitnessCount++;
    now += Time.fixedDeltaTime;
	}
}

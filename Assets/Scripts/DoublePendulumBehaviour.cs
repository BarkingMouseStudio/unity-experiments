using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DoublePendulumBehaviour : MonoBehaviour {

  const float TWO_PI = 2.0f * Mathf.PI;
  const float maxDistance = 12.0f;

  public float speed, Va, Vb, currentFitness;

  protected internal bool isComplete = false;

  Transform parent;
  Rigidbody2D upper;
  Rigidbody2D lower;
  Rigidbody2D wheel;
  WheelJoint2D wheelJoint;

  Neural.Network network;

  ulong[] inNeuronIds = new ulong[]{
    0, 1, 2, 3, 4,
    5, 6, 7, 8, 9,
    10, 11
  };
  ulong[] outNeuronIds = new ulong[]{12, 13};
  ulong neuronCount;

  double[] inputs;
  double[] outputs;

  float duration = 8.0f;
  float now = 0.0f;

  float[] fitnessHistory;
  int fitnessIndex;
  int fitnessCount;

  void Awake() {
    parent = transform.Find("DoublePendulum");
    upper = parent.Find("Upper").GetComponent<Rigidbody2D>();
    lower = parent.Find("Lower").GetComponent<Rigidbody2D>();
    wheel = parent.Find("Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();

    SetGenotype(new List<List<double>>(){
      new List<double>(){0.030240713618695736,0.2858109828084707,-17.099039908498526,-3.98639352992177,-0.09252404049038887,-0.5645759226754308,-89.35704107861966,3.129984624683857,-0.32323266938328743,-0.2820330043323338,-29.5691262697801,10.90599580667913,0.44192608119919896,-0.2727102395147085,-14.774306351318955,-9.999111574143171,0.5329056000337005,-0.21635572193190455,-14.741435530595481,-4.319693185389042,-0.5550133753567934,0.27981191873550415,-18.088895338587463,8.86582643724978,0.2896900540217757,-0.405563450884074,-88.92051875591278,4.914799332618713,0.24111286411061883,-0.23580606142058969,-79.88836073782295,-19.803263507783413,0.6747261811979115,-0.9641528790816665,-42.73162190802395,1.7203876469284296,-0.22941533755511045,-0.09263844788074493,-90.17830961383879,5.472796633839607,-0.07592560909688473,0.9276403109543025,-24.135076510719955,13.321006624028087,-0.9157845829613507,0.01457053842023015,-48.9282809663564,-4.9688019417226315,0.47485026391223073,0.6502056680619717,-95.22150612901896,-10.898202257230878,0.6692204065620899,0.783392854500562,-85.90032735373825,3.2410515192896128},
      new List<double>(){10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0,10.0},
    });

    OnSpawned();
  }

  static float GetAngle(float angle) {
  	while (angle > 180.0f) {
      angle -= 360.0f;
    }
  	while (angle < -180.0f) {
      angle += 360.0f;
    }
  	return angle;
  }

  public float Fitness {
    get {
      float fitness;
      if (fitnessCount > fitnessHistory.Length) {
        fitness = fitnessHistory.Aggregate(0.0f,
          (total, next) => total + next,
          (total) => total / fitnessHistory.Length);
        var worstFitness = 1080.0f;
        var maxFitnessCount = 500.0f;
        return 0.1f * (fitnessCount / maxFitnessCount) + 0.9f * (fitness / worstFitness); // Normalize
      } else {
        return 1.0f; // Worst case, didn't live long enough
      }
    }
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
        // Debug.LogFormat("Hidden out connections: {0} => {1}", inNeuronIds[i], outNeuronIds[j]);
        if (synapseConfigs.MoveNext()) {
          network.AddSynapse(inNeuronIds[i], outNeuronIds[j], synapseConfigs.Current, -10.0f, 10.0f);
        } else {
          UnityEngine.Debug.LogWarning("Ran out of synapses for the outputs neuron.");
        }
      }
    }

    // Connect the output neurons to the last input neuron (recurrent).
    for (int i = 0; i < outNeuronIds.Length; i++) {
      // Debug.LogFormat("Back connections: {0} => {1}", outNeuronIds[i], inNeuronIds[inNeuronIds.Length - i - 1]);
      if (synapseConfigs.MoveNext()) {
        network.AddSynapse(outNeuronIds[i], inNeuronIds[inNeuronIds.Length - i - 1], synapseConfigs.Current, -10.0f, 10.0f);
      } else {
        UnityEngine.Debug.LogWarning("Ran out of synapses for the recurrent input neuron.");
      }
    }

    while (synapseConfigs.MoveNext()) {
      UnityEngine.Debug.LogWarning("Unused synapse configs.");
    }

    neuronCount = network.NeuronCount;
    // Debug.LogFormat("Neurons: {0}, Synapses: {1}",
    //   network.NeuronCount, network.SynapseCount);
  }

  void OnSpawned() {
    // Choose random angle
    parent.localRotation = Quaternion.Euler(0, 0, Random.value > 0.5 ? 185 : 175);

    // Clear fitness history
    fitnessHistory = new float[300];
    fitnessIndex = 0;
    fitnessCount = 0;
  }

  void OnDespawned() {
    isComplete = false;
  }

  void Complete() {
    // Reset motor speed
    SetMotorSpeed(0);

    now = 0.0f;
    isComplete = true;
  }

  void SetMotorSpeed(float speed) {
    var motor = wheelJoint.motor;
  	motor.motorSpeed = speed;
    motor.maxMotorTorque = 3000;
  	wheelJoint.motor = motor;
  }

	void FixedUpdate() {
    if (isComplete) {
      return;
    }

    if (duration - now <= 0) {
      Complete();
      return;
    }

    if (wheel.transform.position.y < -2.0) {
      Complete();
      return;
    }

    // Send inputs
    var upperAngle = GetAngle(upper.rotation);
    var lowerAngle = GetAngle(lower.rotation);
    var upperAngularVelocity = GetAngle(upper.angularVelocity);
    var lowerAngularVelocity = GetAngle(lower.angularVelocity);
    var wheelPosition = wheel.transform.localPosition.x;
    // Debug.LogFormat("{0} {1} {2} {3} {4}", upperAngle, lowerAngle, upperAngularVelocity, lowerAngularVelocity, wheelPosition);

    inputs = new double[neuronCount];
    if (upperAngle >= 0.0f) {
      inputs[0] = +20.0f * (upperAngle / 180.0f);
    } else {
      inputs[1] = -20.0f * (upperAngle / 180.0f);
    }
    if (lowerAngle >= 0.0f) {
      inputs[2] = +20.0f * (lowerAngle / 180.0f);
    } else {
      inputs[3] = -20.0f * (lowerAngle / 180.0f);
    }
    if (upperAngularVelocity >= 0.0f) {
      inputs[4] = +20.0f * (upperAngularVelocity / 180.0f);
    } else {
      inputs[5] = -20.0f * (upperAngularVelocity / 180.0f);
    }
    if (lowerAngularVelocity >= 0.0f) {
      inputs[6] = +20.0f * (lowerAngularVelocity / 180.0f);
    } else {
      inputs[7] = -20.0f * (lowerAngularVelocity / 180.0f);
    }
    if (wheelPosition >= 0.0f) {
      inputs[8] = +20.0f * (wheelPosition / 6.0f);
    } else {
      inputs[9] = -20.0f * (wheelPosition / 6.0f);
    }
    // UnityEngine.Debug.Log(string.Join(", ", inputs.Select(i => i.ToString()).ToArray()));

    // Receive outputs
    var ticks = Time.fixedDeltaTime * 1000.0f;
    outputs = new double[neuronCount];
    network.Tick((ulong)ticks, inputs, ref outputs);
    // UnityEngine.Debug.Log(string.Join(",", outputs.Select(o => o.ToString()).ToArray()));

    // Read out neuron V for speed
    var V_acc = (float)outputs[outNeuronIds[0]];
    var V_dec = (float)outputs[outNeuronIds[1]];

    // Update motor speed
    var speed = 0.0f;
    speed += (V_acc / 30.0f) / 2.0f;
    speed -= (V_dec / 30.0f) / 2.0f;
    speed *= 100.0f;
    SetMotorSpeed(speed);

    // Record history
    currentFitness = Mathf.Abs(lowerAngle) * 1.0f + Mathf.Abs(upperAngle) * 1.0f +
                Mathf.Abs(lowerAngularVelocity) * 1.0f + Mathf.Abs(upperAngularVelocity) * 1.0f +
                Mathf.Abs(speed) * 0.18f + Mathf.Abs(wheelPosition) * 30.0f;
    fitnessHistory[fitnessIndex] = currentFitness;
    fitnessIndex++;
    fitnessIndex %= fitnessHistory.Length;

    fitnessCount++;
    now += Time.fixedDeltaTime;
	}
}

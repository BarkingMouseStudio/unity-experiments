using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DoublePendulumBehaviour : MonoBehaviour {

  const float TWO_PI = 2.0f * Mathf.PI;
  const float maxDistance = 12.0f;

  static string[] names = new string[]{"Ben", "Jim", "Bob", "Sue", "Amy", "Ann", "Sam", "Dan", "George", "Ed", "Joe"};

  protected internal bool isComplete = false;
  public Text nameField;

  Transform parent;
  Rigidbody2D upper;
  Rigidbody2D lower;
  Rigidbody2D wheel;
  WheelJoint2D wheelJoint;

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
    -1000.0, -100.0, -10.0, -1.0,
    1.0, 10.0, 100.0, 1000.0,
  };

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

  float duration = 7.0f;
  float now = 0.0f;

  float[] fitnessHistory;
  int fitnessIndex;
  int fitnessCount;
  int fitnessLength = 350;

  void Awake() {
    parent = transform.Find("DoublePendulum");
    upper = parent.Find("Upper").GetComponent<Rigidbody2D>();
    lower = parent.Find("Lower").GetComponent<Rigidbody2D>();
    wheel = parent.Find("Wheel").GetComponent<Rigidbody2D>();
    wheelJoint = wheel.transform.GetComponentInChildren<WheelJoint2D>();

    SetGenotype(new List<List<double>>(){
      new List<double>(){-0.4058302557095885,0.2521210815757513,-5.332865077070892,7.407665168866515,0.14045477379113436,0.6768293231725693,-58.56127846054733,-9.874404184520245,0.04888633964583278,0.9351401054300368,-63.32889280747622,18.58414063230157,-0.5381791852414608,-0.13699913024902344,-35.47081716824323,-7.911439090967178,-0.4273441433906555,0.8700608261860907,-15.214023552834988,-15.677474085241556,0.7338666860014207,-0.18538392800837755,-11.31864816416055,11.928714960813522,-0.8387203668244183,0.055621285922825336,-46.045068837702274,-16.809250134974718,0.12544145341962576,-0.023462950717657804,-75.86477911099792,-8.279131073504686,-0.3565202667377889,-0.8347263182513416,-7.913040649145842,-18.29885588027537,-0.3223045477643609,0.14865243341773748,-25.453503034077585,12.825178913772106,-0.2907294984906912,-0.25207078736275434,-48.614497133530676,11.053375629708171,0.7288209716789424,0.17539994325488806,-85.8292557997629,-7.11353512480855,0.6950296037830412,0.04847937868908048,-49.142358684912324,-6.192994313314557,-0.16766278725117445,0.4759996719658375,-56.83437210973352,14.149533528834581},
      new List<double>(){-7.403752664104104,-6.964480448514223,17.153176655992866,-4.896272625774145,10.20842527039349,17.36826341599226,16.884225457906723,14.588113892823458,17.04016056843102,17.816350664943457,5.057464921846986,-16.078465450555086,4.717035843059421,-8.877367274835706,-12.825829265639186,-5.0760292913764715,2.8809707332402468,6.104256762191653,-12.926153726875782,-1.135990647599101,-4.828482121229172,14.239508677273989,2.6491028908640146,13.36763747036457,-12.837479868903756,-10.779782868921757},
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
        return 0.1f * (fitnessCount / fitnessLength) + 0.9f * (fitness / worstFitness); // Normalize
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
        // Debug.LogFormat("In => out connections: {0} => {1}", inNeuronIds[i], outNeuronIds[j]);
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
    // Debug.LogFormat("Neurons: {0}, Synapses: {1}",
    //   network.NeuronCount, network.SynapseCount);
  }

  void OnSpawned() {
    // Choose random angle
    parent.localRotation = Quaternion.Euler(0, 0, Random.value > 0.5 ? 185 : 175);

    // Clear fitness history
    fitnessHistory = new float[fitnessLength];
    fitnessIndex = 0;
    fitnessCount = 0;

    // Pick a random name
    nameField.text = names[Random.Range(0, names.Length)];
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
    var data = new float[]{
      GetAngle(upper.rotation),
      GetAngle(lower.rotation),
      GetAngle(upper.angularVelocity),
      GetAngle(lower.angularVelocity)
    };
    // UnityEngine.Debug.Log(string.Join(", ", data.Select(d => d.ToString()).ToArray()));

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
    // UnityEngine.Debug.Log(string.Join(", ", inputs.Select(i => i.ToString()).ToArray()));

    // Receive outputs
    var ticks = Time.fixedDeltaTime * 1000.0f;
    outputs = new double[neuronCount];
    network.Tick(ticks, inputs, ref outputs);
    // UnityEngine.Debug.Log(string.Join(",", outputs.Select(o => o.ToString()).ToArray()));

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

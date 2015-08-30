using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

// XXX: UNKNOWNS: sigma, weight random and min/max, noise, population sizes
public class ActuatorBehaviour : MonoBehaviour {

	public Transform shoulderJoint;
	public Transform elbowJoint;
	public Transform endEffector;
	public Transform target;

	public bool saveWeights = false;

	public bool isTraining = false;
	public float noiseRate = 3.0f;

	[Header("Static")]
	public float sigma = 3.0f;
	public float F_max = 15.0f;
	public float w_min = -15.0f;
	public float w_max = 15.0f;

	private ActuatorPorts network;
	private float inputRate = 0.0f;
	private float outputRate = 0.0f;

	void Awake() {
		network = new ActuatorPorts(w_min, w_max, sigma, F_max);
	}

	void Start() {
    var logPath = string.Format("logs_{0}", DateTime.Now.Ticks);
    Debug.LogFormat("Logging to {0}", logPath);

    if (!Directory.Exists(logPath)) {
      Directory.CreateDirectory(logPath);
    }

    weightsLog = File.CreateText(Path.Combine(logPath, "weights.csv"));
    outputLog = File.CreateText(Path.Combine(logPath, "output.csv"));
    inputLog = File.CreateText(Path.Combine(logPath, "input.csv"));
	}

	// void Start() {
	// 	var population = new double[200];
  //   var estimator = new CenterOfMassEstimator(6.0f, 30.0f, population.Length);
	// 	estimator.Set(population, 0, 0.2f);
	// 	estimator.Set(population, 0, 0.4f);
	// 	Debug.Log(population.Stringify());
	// }

	float shoulderMotorCommand = 0.0f;
	float elbowMotorCommand = 0.0f;
	float targetAngleNorm = 0.0f;

	// int nextHomePosition = 0;
	int interval = 0;
	int intervalCount = 0;
	float intervalTime = 0.0f;
	float trainingDuration = 0.02f;
	float restingDuration = 0.05f;

	// float[][] homePositions = new float[][]{
	// 	new float[]{0, 0},
	// 	new float[]{45, 0},
	// 	new float[]{-45, 0},
	// };

  StreamWriter weightsLog;
  StreamWriter inputLog;
  StreamWriter outputLog;

	void LogInput() {
		inputLog.WriteLine(network.input.Stringify());
		inputLog.Flush();

		inputRate = 0f;
		for (var i = 0; i < network.input.Length; i++) {
			if (network.input[i] > 0f) {
				inputRate += (float)network.input[i];
			}
		}
	}

	void LogOutput() {
		outputLog.WriteLine(network.output.Stringify());
		outputLog.Flush();

		outputRate = 0f;
		for (var i = 0; i < network.output.Length; i++) {
			if (network.output[i] > 0f) {
				outputRate += (float)(network.output[i] / 30.0);
			}
		}

		if (saveWeights) {
			var weights = network.DumpWeights();
			weightsLog.WriteLine(weights.Stringify());
			weightsLog.Flush();
			saveWeights = false;
		}
	}

	float GetTargetAngleNorm(Vector2 targetDirection) {
		var targetAngleRad = Mathf.Atan2(targetDirection.y, targetDirection.x); // [-PI / 2, PI / 2]
		var targetAngleDeg = targetAngleRad * Mathf.Rad2Deg; // [-180, 180]
		var targetAngleBin = NumberHelper.Bin(targetAngleDeg, 45.0f) / 8f; // [-0.5, 0.5]
		var targetAngleNorm = NumberHelper.Normalize(targetAngleBin, -0.5f, 0.5f); // [0, 1]
		return targetAngleNorm;
	}

	// void SetHomePosition(int i) {
	// 	var homePosition = homePositions[i];
	// 	shoulderJoint.localRotation = Quaternion.Euler(0, 0, homePosition[0]);
	// 	elbowJoint.localRotation = Quaternion.Euler(0, 0, homePosition[1]);
	// }

	void SetMotorCommand() {
		// Set end effector position before applying movements
		var previousEndEffectorPosition = endEffector.position;

		// Generate random motor commands (ERG)
		var shoulderMotorAngle = UnityEngine.Random.Range(-5.0f, 5.0f);
		var elbowMotorAngle = UnityEngine.Random.Range(-5.0f, 5.0f);

		// Move joints randomly
		shoulderJoint.Rotate(0, 0, shoulderMotorAngle);
		elbowJoint.Rotate(0, 0, elbowMotorAngle);

		shoulderMotorCommand = NumberHelper.Normalize(shoulderMotorAngle, -5.0f, 5.0f);
		elbowMotorCommand = NumberHelper.Normalize(elbowMotorAngle, -5.0f, 5.0f);

		// Compute difference of end effectors position from movements
		var currentEndEffectorPosition = endEffector.position;
		var targetDirection = (currentEndEffectorPosition - previousEndEffectorPosition).normalized;
		Debug.DrawRay(previousEndEffectorPosition, targetDirection, Color.red);

  	network.targetDirection.Set(GetTargetAngleNorm(targetDirection));
		network.shoulderMotorCommand.Set(shoulderMotorCommand);
		network.elbowMotorCommand.Set(elbowMotorCommand);
	}

	void Train() {
		if (interval == 0) { // Training interval
			if (intervalTime > trainingDuration) {
				intervalTime = 0.0f;
				interval = (interval + 1) % 2;
			} else {
			  network.targetDirection.Set(targetAngleNorm);
				network.shoulderMotorCommand.Set(shoulderMotorCommand);
				network.elbowMotorCommand.Set(elbowMotorCommand);
				intervalTime += Time.deltaTime;
			}
		} else if (interval == 1) { // Resting interval
			if (intervalTime > restingDuration) {
				intervalTime = 0.0f;
				interval = (interval + 1) % 2;

				SetMotorCommand();
				intervalCount++;

				// if (intervalCount % 4 == 1) { // Every fourth interval, change home position
				// 	Debug.Log("Setting new home position");
				// 	SetHomePosition(nextHomePosition);
				// 	nextHomePosition = (nextHomePosition + 1) % homePositions.Length;
				// }
			} else {
				intervalTime += Time.deltaTime;
			}
		}

		LogInput();

    network.Tick();
	}

	void Perform() {
		// Compute desired direction of end effector
		var currentEndEffectorPosition = endEffector.position;

		var targetPosition = target.position;
		var targetDirection = (targetPosition - currentEndEffectorPosition).normalized;
		Debug.DrawRay(currentEndEffectorPosition, targetDirection, Color.red);

	  network.targetDirection.Set(GetTargetAngleNorm(targetDirection));

		LogInput();

    network.Tick();

		if (network.shoulderMotorCommand.TryGet(out shoulderMotorCommand)) {
			shoulderJoint.Rotate(0, 0,
				NumberHelper.Scale(shoulderMotorCommand, -5.0f, 5.0f));
		}

		if (network.elbowMotorCommand.TryGet(out elbowMotorCommand)) {
			elbowJoint.Rotate(0, 0,
				NumberHelper.Scale(elbowMotorCommand, -5.0f, 5.0f));
		}
	}

	void FixedUpdate() {
    network.Clear();

		if (noiseRate != 0f) network.Noise(noiseRate);

		var shoulderAngle = shoulderJoint.rotation.eulerAngles.z;
		var elbowAngle = elbowJoint.rotation.eulerAngles.z;

		var shoulderBin = NumberHelper.Bin(shoulderAngle, 5.0f);
		var elbowBin = NumberHelper.Bin(elbowAngle, 5.0f);

		var shoulderNorm = shoulderBin / 72f;
		var elbowNorm = elbowBin / 72f;

	  network.shoulderProprioception.Set(shoulderNorm);
	  network.elbowProprioception.Set(elbowNorm);

		if (isTraining) {
			Train();
		} else {
			Perform();
		}

		LogOutput();
	}

  void OnApplicationQuit() {
    weightsLog.Close();
    outputLog.Close();
    inputLog.Close();
  }
}

using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

public class ActuatorBehaviour : MonoBehaviour {

	public Transform shoulderJoint;
	public Transform elbowJoint;
	public Transform endEffector;
	public Transform target;

	public bool isTraining = true;
	public bool saveWeights = false;
	public bool saveRates = false;
	public bool saveInputs = false;
	public bool saveOutputs = false;

	public float saveWeightsInterval = 10.0f;
	float nextSaveWeights = 0.0f;

	ActuatorPorts network;

  #pragma warning disable 0414
	double totalRate, averageRate;
  #pragma warning restore 0414

	float shoulderProprioception = 0.0f;
	float elbowProprioception = 0.0f;
	float targetDirection = 0.0f;
	float shoulderMotorCommand = 0.0f;
	float elbowMotorCommand = 0.0f;

	float[] homePositions = new float[]{0, -45, 45};

  StreamWriter weightsLog;
  StreamWriter inputLog;
  StreamWriter outputLog;
  StreamWriter rateLog;

	void Awake() {
		network = new ActuatorPorts();
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
    rateLog = File.CreateText(Path.Combine(logPath, "rate.csv"));
	}

	void LogInput() {
		Debug.DrawRay(endEffector.position, Quaternion.Euler(0, 0, targetDirection) * Vector2.right, Color.red);

		if (saveInputs) {
			inputLog.WriteLine(network.input.Stringify());
			inputLog.Flush();
		}
	}

	void LogOutput() {
		totalRate = network.totalRate;
		averageRate = network.averageRate;

		if (saveOutputs) {
			outputLog.WriteLine(network.output.Stringify());
			outputLog.Flush();
		}

		if (saveRates) {
			rateLog.WriteLine(network.rate.Stringify());
			rateLog.Flush();
		}

		if (saveWeights) {
			if (Time.time >= nextSaveWeights) {
				Debug.Log("Saving weights");
				var weights = network.DumpWeights();
				weightsLog.WriteLine(weights.Stringify());
				weightsLog.Flush();
				nextSaveWeights += saveWeightsInterval;
			}
		}
	}

	static float GetTargetDirection(Vector2 targetDirection) {
		return Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
	}

	void SetHomePosition() {
		var shoulderHome = homePositions[UnityEngine.Random.Range(0, homePositions.Length)];
		var elbowHome = homePositions[UnityEngine.Random.Range(0, homePositions.Length)];
		shoulderJoint.localRotation = Quaternion.Euler(0, 0, shoulderHome);
		elbowJoint.localRotation = Quaternion.Euler(0, 0, elbowHome);
	}

	void Train() {
		// Set end effector position before applying movements
		var previousEndEffectorPosition = endEffector.position;

		// Generate random motor commands (ERG)
		shoulderMotorCommand = UnityEngine.Random.Range(-5.0f, 5.0f);
		elbowMotorCommand = UnityEngine.Random.Range(-5.0f, 5.0f);

		// Move joints randomly
		shoulderJoint.Rotate(0, 0, shoulderMotorCommand);
		elbowJoint.Rotate(0, 0, elbowMotorCommand);

		shoulderProprioception = AngleHelper.GetAngle(shoulderJoint.localRotation.eulerAngles.z);
		elbowProprioception = AngleHelper.GetAngle(elbowJoint.localRotation.eulerAngles.z);
		targetDirection = GetTargetDirection((endEffector.position - previousEndEffectorPosition).normalized);

		// Reset home position
		SetHomePosition();

	  network.shoulderProprioception.Set(shoulderProprioception);
	  network.elbowProprioception.Set(elbowProprioception);
	  network.targetDirection.Set(targetDirection);
		network.shoulderMotorCommand.Set(shoulderMotorCommand);
		network.elbowMotorCommand.Set(elbowMotorCommand);

		LogInput();
    network.Tick();

	  network.shoulderProprioception.TryGet(out shoulderProprioception);
	  network.elbowProprioception.TryGet(out elbowProprioception);
	  network.targetDirection.TryGet(out targetDirection);
		network.shoulderMotorCommand.TryGet(out shoulderMotorCommand);
		network.elbowMotorCommand.TryGet(out elbowMotorCommand);

		LogOutput();
	}

	void Perform() {
		shoulderProprioception = AngleHelper.GetAngle(shoulderJoint.localRotation.eulerAngles.z);
		elbowProprioception = AngleHelper.GetAngle(elbowJoint.localRotation.eulerAngles.z);
		targetDirection = GetTargetDirection((target.position - endEffector.position).normalized);

	  network.shoulderProprioception.Set(shoulderProprioception);
	  network.elbowProprioception.Set(elbowProprioception);
	  network.targetDirection.Set(targetDirection);

		LogInput();

    network.Tick();

	  network.shoulderProprioception.TryGet(out shoulderProprioception);
	  network.elbowProprioception.TryGet(out elbowProprioception);
	  network.targetDirection.TryGet(out targetDirection);

		if (network.shoulderMotorCommand.TryGet(out shoulderMotorCommand)) {
			shoulderJoint.Rotate(0, 0, shoulderMotorCommand);
		}

		if (network.elbowMotorCommand.TryGet(out elbowMotorCommand)) {
			elbowJoint.Rotate(0, 0, elbowMotorCommand);
		}

		LogOutput();
	}

	void FixedUpdate() {
    network.Clear();

		if (isTraining) {
			network.IsTraining = true;
			network.Noise(3f, 30f);
			Train();
		} else {
			network.IsTraining = false;
			Perform();
		}
	}

  void OnApplicationQuit() {
    weightsLog.Close();
    outputLog.Close();
    inputLog.Close();
    rateLog.Close();
  }
}

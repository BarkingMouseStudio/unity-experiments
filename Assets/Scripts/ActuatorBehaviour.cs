using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Collections;
using System.Linq;
using System.IO;

public class ActuatorBehaviour : MonoBehaviour {

	enum IntervalMode {
		Training,
		Resting,
	}

	public Transform shoulderJoint;
	public Transform elbowJoint;
	public Transform endEffector;
	public Transform target;

	public bool isTraining = true;
	public bool saveWeights = false;
	public bool saveRates = false;
	public bool saveInputs = false;
	public bool saveOutputs = false;

	ActuatorPorts network;

  #pragma warning disable 0414
	double totalRate, averageRate;
  #pragma warning restore 0414

	float shoulderMotorCommand = 0.0f;
	float elbowMotorCommand = 0.0f;
	float targetAngleNorm = 0.0f;

	IntervalMode intervalMode = IntervalMode.Resting;
	int intervalCount = 0;
	float intervalTime = 0.0f;
	float trainingDuration = 0.02f;
	float restingDuration = 0.05f;

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
			var weights = network.DumpWeights();
			weightsLog.WriteLine(weights.Stringify());
			weightsLog.Flush();
			saveWeights = false;
		}
	}

	float GetTargetAngleNorm(Vector2 targetDirection) {
		var targetAngleRad = Mathf.Atan2(targetDirection.y, targetDirection.x); // [-PI / 2, PI / 2]
		var targetAngleDeg = targetAngleRad * Mathf.Rad2Deg; // [-180, 180]
		var targetAngleBin = NumberHelper.Bin(targetAngleDeg, 5.0f) / 72f; // [-0.5, 0.5]
		return NumberHelper.Normalize(targetAngleBin, -0.5f, 0.5f); // [0, 1]
	}

	void Train() {
		if (intervalMode == IntervalMode.Training) { // Training interval
			Debug.LogFormat("Training... {0}s", intervalTime);

			if (intervalTime > trainingDuration) {
				intervalTime = 0.0f;
				intervalMode = IntervalMode.Resting;
			} else {
			  network.targetDirection.Set(targetAngleNorm);
				network.shoulderMotorCommand.Set(shoulderMotorCommand);
				network.elbowMotorCommand.Set(elbowMotorCommand);
				intervalTime += Time.deltaTime;
			}
		} else if (intervalMode == IntervalMode.Resting) { // Resting interval
			Debug.LogFormat("Resting... {0}s", intervalTime);

			if (intervalTime > restingDuration) {
				intervalTime = 0.0f;
				intervalMode = IntervalMode.Training;

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

				targetAngleNorm = GetTargetAngleNorm(targetDirection);

				intervalCount++;
			} else {
				intervalTime += Time.deltaTime;
			}
		}

		LogInput();

    network.Tick();

		LogOutput();
	}

	void Perform() {
		var currentEndEffectorPosition = endEffector.position;
		var targetPosition = target.position;
		var targetDirection = (targetPosition - currentEndEffectorPosition).normalized;
		Debug.DrawRay(currentEndEffectorPosition, targetDirection, Color.red);

		targetAngleNorm = GetTargetAngleNorm(targetDirection);
	  network.targetDirection.Set(targetAngleNorm);

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

		LogOutput();
	}

	void FixedUpdate() {
    network.Clear();
		network.Noise(3f, 120f);

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
	}

  void OnApplicationQuit() {
    weightsLog.Close();
    outputLog.Close();
    inputLog.Close();
    rateLog.Close();
  }
}

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

	IntervalMode intervalMode = IntervalMode.Resting;
	int intervalCount = 0;
	float intervalTime = 0.0f;
	float trainingDuration = 0.02f;
	float restingDuration = 0.05f;

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

	float target_shoulderProprioception = 0.0f;
	float target_elbowProprioception = 0.0f;
	float target_targetDirection = 0.0f;
	float target_shoulderMotorCommand = 0.0f;
	float target_elbowMotorCommand = 0.0f;

  #pragma warning restore 0414
	float actual_shoulderProprioception = 0.0f;
	float actual_elbowProprioception = 0.0f;
	float actual_targetDirection = 0.0f;
  #pragma warning disable 0414

	float actual_shoulderMotorCommand = 0.0f;
	float actual_elbowMotorCommand = 0.0f;

	float[] homePositions = new float[]{0, -45, 45, -90, 90};

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
		Debug.DrawRay(endEffector.position, Quaternion.Euler(0, 0, target_targetDirection) * Vector2.right, Color.red);

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
		if (intervalMode == IntervalMode.Training) { // Training interval
			if (intervalTime < trainingDuration) {
			  network.shoulderProprioception.Set(target_shoulderProprioception);
			  network.elbowProprioception.Set(target_elbowProprioception);
			  network.targetDirection.Set(target_targetDirection);
				network.shoulderMotorCommand.Set(target_shoulderMotorCommand);
				network.elbowMotorCommand.Set(target_elbowMotorCommand);

				intervalTime += Time.deltaTime;
			} else {
				intervalTime = 0.0f;
				intervalMode = IntervalMode.Resting;
			}
		}

		if (intervalMode == IntervalMode.Resting) { // Resting interval
			if (intervalTime > restingDuration) {
				intervalTime = 0.0f;
				intervalMode = IntervalMode.Training;

				// Every fourth intervalMode, change home position
				if (intervalCount % 4 == 0) {
					SetHomePosition();
				}

				intervalCount++;

				// Set end effector position before applying movements
				var previousEndEffectorPosition = endEffector.position;

				// Generate random motor commands (ERG)
				target_shoulderMotorCommand = UnityEngine.Random.Range(-5.0f, 5.0f);
				target_elbowMotorCommand = UnityEngine.Random.Range(-5.0f, 5.0f);

				// Move joints randomly
				shoulderJoint.Rotate(0, 0, target_shoulderMotorCommand);
				elbowJoint.Rotate(0, 0, target_elbowMotorCommand);

				target_shoulderProprioception = AngleHelper.GetAngle(shoulderJoint.localRotation.eulerAngles.z);
				target_elbowProprioception = AngleHelper.GetAngle(elbowJoint.localRotation.eulerAngles.z);
				target_targetDirection = GetTargetDirection((endEffector.position - previousEndEffectorPosition).normalized);
			} else {
				intervalTime += Time.deltaTime;
			}
		}

		LogInput();
    network.Tick();

		if (intervalMode == IntervalMode.Training) { // Training interval
		  network.shoulderProprioception.TryGet(out actual_shoulderProprioception);
		  network.elbowProprioception.TryGet(out actual_elbowProprioception);
		  network.targetDirection.TryGet(out actual_targetDirection);
			network.shoulderMotorCommand.TryGet(out actual_shoulderMotorCommand);
			network.elbowMotorCommand.TryGet(out actual_elbowMotorCommand);
		}

		LogOutput();
	}

	void Perform() {
		target_shoulderProprioception = AngleHelper.GetAngle(shoulderJoint.localRotation.eulerAngles.z);
		target_elbowProprioception = AngleHelper.GetAngle(elbowJoint.localRotation.eulerAngles.z);
		target_targetDirection = GetTargetDirection((target.position - endEffector.position).normalized);

	  network.shoulderProprioception.Set(target_shoulderProprioception);
	  network.elbowProprioception.Set(target_elbowProprioception);
	  network.targetDirection.Set(target_targetDirection);

		LogInput();

    network.Tick();

	  network.shoulderProprioception.TryGet(out actual_shoulderProprioception);
	  network.elbowProprioception.TryGet(out actual_elbowProprioception);
	  network.targetDirection.TryGet(out actual_targetDirection);

		if (network.shoulderMotorCommand.TryGet(out actual_shoulderMotorCommand)) {
			shoulderJoint.Rotate(0, 0, actual_shoulderMotorCommand);
		}

		if (network.elbowMotorCommand.TryGet(out actual_elbowMotorCommand)) {
			elbowJoint.Rotate(0, 0, actual_elbowMotorCommand);
		}

		LogOutput();
	}

	void FixedUpdate() {
    network.Clear();

		if (isTraining) {
			network.ToggleTraining(true);
			network.Noise(5f, 120f);
			Train();
		} else {
			network.ToggleTraining(false);
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

using UnityEngine;
using System.Collections;

public class ErrorBehavior : MonoBehaviour {

	public Transform[] targets;
	public Transform[] actual;

	private float error = 0.0f;
	public float Error {
		get { return error; }
	}

	void Update() {
		error = 0.0f;

		var pDiff = 0.0f;
		var aDiff = 0.0f;
		for (var i = 0; i < targets.Length; i++) {
			pDiff += Vector3.Distance(targets[i].position, actual[i].position);
			aDiff += Quaternion.Angle(targets[i].rotation, actual[i].rotation);
		}

		error += pDiff;
		error += aDiff;
	}
}

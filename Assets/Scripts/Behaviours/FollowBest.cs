using UnityEngine;
using System.Collections;

public class FollowBest : MonoBehaviour {

	public EvolutionBehaviour evolution;
	public float speed = 1.0f;
	public float changeDuration = 3.0f;

	EvaluationBehaviour currentBest;
	float lastBestTime;

	void OnEnable() {
		evolution.BestEvaluation += OnBestEvaluation;
	}

	void OnDisable() {
		evolution.BestEvaluation -= OnBestEvaluation;
	}

	void OnBestEvaluation(EvaluationBehaviour nextBest) {
		// Only change look-at every 3 seconds
		if (lastBestTime == 0.0f || Time.time - lastBestTime > changeDuration) {
			lastBestTime = Time.time;
			currentBest = nextBest;
		}
	}

	void Update() {
		Vector3 targetPosition = currentBest != null ? currentBest.wheel.transform.position : Vector3.zero;

		Vector3 nextPosition = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * speed);
		nextPosition.z = -10.0f;

		transform.position = nextPosition;
	}
}

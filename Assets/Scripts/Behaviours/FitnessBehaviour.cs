using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FitnessBehaviour : MonoBehaviour {

  public Text textField;

  EvaluationBehaviour evaluation;

	void Awake() {
    textField.text = (0.0f).ToString();
    evaluation = GetComponent<EvaluationBehaviour>();
	}

	void Update() {
    textField.text = evaluation.CurrentTrial.Fitness.ToString();
	}
}

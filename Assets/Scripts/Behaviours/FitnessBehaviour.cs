using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FitnessBehaviour : MonoBehaviour {

  Text textField;
  EvaluationBehaviour evaluation;

	void Awake() {
    textField = transform.Find("Canvas/Text").GetComponent<Text>();
    textField.text = (0.0f).ToString();

    evaluation = GetComponent<EvaluationBehaviour>();
	}

	void Update() {
    if (evaluation.Phenotype != null) {
      textField.text = evaluation.Phenotype.CurrentTrial.Fitness.ToString();
    }
	}
}

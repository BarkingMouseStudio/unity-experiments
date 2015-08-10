using UnityEngine;
using System.Collections;

public class SpeedBehaviour : MonoBehaviour {

	public SpriteRenderer tile;
	ControllerBehaviour controller;

	void Awake() {
		controller = GetComponent<ControllerBehaviour>();
	}

	static Color GetColor(double rate) {
		if (rate >= 0.0f) {
    	return Color.Lerp(Color.white, Color.red, (float)rate);
		} else {
    	return Color.Lerp(Color.white, Color.blue, Mathf.Abs((float)rate));
		}
	}

	void Update() {
		tile.color = GetColor(controller.Speed / 800.0f);
	}
}

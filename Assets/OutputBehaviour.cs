using UnityEngine;
using System.Collections;

public class OutputBehaviour : MonoBehaviour {

	public SpriteRenderer[] tiles;
	ControllerBehaviour controller;

	void Awake() {
		controller = GetComponent<ControllerBehaviour>();
	}

	void SetColor(SpriteRenderer tile, double rate) {
		if (rate >= 0.0f) {
    	tile.color = Color.Lerp(Color.white, Color.red, (float)rate);
		} else {
    	tile.color = Color.Lerp(Color.white, Color.blue, Mathf.Abs((float)rate));
		}
	}

	void Update() {
		if (controller.Network != null) {
    	SetColor(tiles[0], controller.Network.Speed.Get() / 250.0f);
		}
	}
}

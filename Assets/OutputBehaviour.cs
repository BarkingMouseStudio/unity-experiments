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
	    SetColor(tiles[0], controller.Network.SlowForward.Rate);
	    SetColor(tiles[1], controller.Network.MediumForward.Rate);
	    SetColor(tiles[2], controller.Network.FastForward.Rate);

	    SetColor(tiles[3], controller.Network.FastBackward.Rate);
	    SetColor(tiles[4], controller.Network.MediumBackward.Rate);
	    SetColor(tiles[5], controller.Network.SlowBackward.Rate);
		}

    SetColor(tiles[6], controller.Speed / 250.0f);
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Handles pool manager reset during spawn/despawn
public class ResetBehaviour : MonoBehaviour {

  public class State {
    public Transform transform;
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public List<State> states;
  }

  Rigidbody2D[] rigidbodies;
  List<State> states;

  void Start() {
    rigidbodies = GetComponentsInChildren<Rigidbody2D>();
    states = new List<State>(1);

    SaveState(transform, states);
  }

	void OnDespawned() {
    RestoreState(states);

    foreach (var rb in rigidbodies) {
      rb.velocity = Vector2.zero;
      rb.angularVelocity = 0;
    }
	}

  void SaveState(Transform transform, List<State> states) {
    var state = new State{
      transform = transform,
      position = transform.localPosition,
      rotation = transform.localRotation,
      scale = transform.localScale,
      states = new List<State>(transform.childCount),
    };
    states.Add(state);

    foreach (Transform child in transform) {
      SaveState(child, state.states);
    }
  }

  void RestoreState(List<State> states) {
    foreach (var state in states) {
      state.transform.localPosition = state.position;
      state.transform.localRotation = state.rotation;
      state.transform.localScale = state.scale;

      RestoreState(state.states);
    }
  }
}

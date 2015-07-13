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
    public Rigidbody2D rigidbody;
    public bool isKinematic;
  }

  List<State> states = new List<State>(1);

  void Start() {
    SaveState(transform, states);
  }

	void OnDespawned() {
    RestoreState(states);
	}

  void SaveState(Transform transform, List<State> states) {
    Rigidbody2D rigidbody = transform.GetComponent<Rigidbody2D>();
    bool isKinematic = false;
    if (rigidbody != null) {
      isKinematic = rigidbody.isKinematic;
    }
    var state = new State{
      transform = transform,
      position = transform.localPosition,
      rotation = transform.localRotation,
      scale = transform.localScale,
      states = new List<State>(transform.childCount),
      rigidbody = rigidbody,
      isKinematic = isKinematic,
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

      if (state.rigidbody != null) {
        state.rigidbody.velocity = Vector2.zero;
        state.rigidbody.angularVelocity = 0;
        state.rigidbody.isKinematic = state.isKinematic;
      }

      RestoreState(state.states);
    }
  }
}

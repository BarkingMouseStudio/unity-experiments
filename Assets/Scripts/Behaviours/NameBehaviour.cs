using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Picks a random name.
public class NameBehaviour : MonoBehaviour {

  static readonly string[] names = new string[]{"Ben", "Jim", "Bob", "Sue", "Amy", "Ann", "Sam", "Dan", "George", "Ed", "Joe"};

  public Text nameField;

	void Awake() {
    nameField.text = names[Random.Range(0, names.Length)];
	}

	void OnSpawned() {
    nameField.text = names[Random.Range(0, names.Length)];
	}
}

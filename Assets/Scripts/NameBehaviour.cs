using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Pick a random name
public class NameBehaviour : MonoBehaviour {

  static readonly string[] names = new string[]{"Ben", "Jim", "Bob", "Sue", "Amy", "Ann", "Sam", "Dan", "George", "Ed", "Joe"};

  Text nameField;

	void Awake() {
    nameField = transform.Find("Canvas/Text").GetComponent<Text>();
    nameField.text = names[Random.Range(0, names.Length)];
	}

	void OnSpawned() {
    nameField.text = names[Random.Range(0, names.Length)];
	}
}

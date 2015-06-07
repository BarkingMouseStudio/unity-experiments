﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NameBehaviour : MonoBehaviour {

  static readonly string[] names = new string[]{"Ben", "Jim", "Bob", "Sue", "Amy", "Ann", "Sam", "Dan", "George", "Ed", "Joe"};

  Text nameField;

	void Awake() {
    nameField = transform.Find("Canvas/Text");

    // Pick a random name
    nameField.text = names[Random.Range(0, names.Length)];
	}

	void OnSpawned() {
    // Pick a random name
    nameField.text = names[Random.Range(0, names.Length)];
	}
}

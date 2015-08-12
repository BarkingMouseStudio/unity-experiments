using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum NeuronType {
  UpperNeuron = 0,
  LowerNeuron = 1,
  PositionNeuron = 2,
  SpeedNeuron = 3,
  HiddenNeuron = 4,
}

public static class NeuronTypeExtensions {

  public static bool IsInput(this NeuronType type) {
    switch (type) {
      case NeuronType.UpperNeuron:
      case NeuronType.LowerNeuron:
      case NeuronType.PositionNeuron:
        return true;
      default:
        return false;
    }
  }

  public static bool IsOutput(this NeuronType type) {
    return type == NeuronType.PositionNeuron;
  }

  public static bool IsHidden(this NeuronType type) {
    return type == NeuronType.HiddenNeuron;
  }
}

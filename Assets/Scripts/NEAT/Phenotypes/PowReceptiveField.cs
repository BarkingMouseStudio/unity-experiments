using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct PowReceptiveField : IReceptiveField {
  float mean;
  float sigma;

  public PowReceptiveField(float mean, float sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public float Normalize(float val) {
    return Mathf.Clamp01(
      1.0f - Mathf.Pow(
        Mathf.Abs(val - mean) / sigma
      , 2.0f)
    );
  }
}

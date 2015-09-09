using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct SignReceptiveField : IReceptiveField {
  float mean;
  float sigma;

  public SignReceptiveField(float mean, float sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public float Normalize(float val) {
    var x = 1.0f - Mathf.Abs(val - mean) / sigma;
    if (float.IsNaN(x)) {
      return 0.0f;
    }
    return Mathf.Max(0.0f, Math.Sign(x));
  }
}

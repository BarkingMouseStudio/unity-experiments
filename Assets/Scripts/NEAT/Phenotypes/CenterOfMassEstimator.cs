using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class CenterOfMassEstimator {

  private float sigma2_2; // 2 x standard deviation, squared of the gaussian curve
  private float F_max; // maximum firing rate
  private int n;

  public CenterOfMassEstimator(float sigma, float F_max, int n) {
    this.sigma2_2 = 2.0f * Mathf.Pow(sigma, 2.0f);
    this.F_max = F_max;
    this.n = n;
  }

  // f : [0, 1] -> [1, n]
  private int f(float v) {
    return Mathf.RoundToInt(v * n);
  }

  // f_inv : [1, n] -> [0, 1]
  private float f_inv(int x) {
    return (float)x / (float)n;
  }

  // expresses the firing rate of neuron x when the normalized value v_0 is encoded
  public void Set(double[] p, int offset, float v) {
    for (var i = 0; i < n; i++) {
      p[offset + i] += F_max * Mathf.Exp(
        -1.0f * (Mathf.Pow(i - f(v), 2.0f) / sigma2_2
      ));
    }
  }

  public bool TryGet(double[] p, int offset, out float v) {
    var num = 0.0f;
    for (var i = 0; i < n; i++) {
      num += f_inv(i) * (float)p[offset + i];
    }

    var den = 0.0f;
    for (var i = 0; i < n; i++) {
      den += (float)p[offset + i];
    }

    if (den == 0.0f) {
      v = 0.0f;
      return false;
    }

    v = num / den;
    return true;
  }
}

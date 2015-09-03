using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class CenterOfMassEstimator {

  private float sigma2_2; // 2 x standard deviation, squared of the gaussian curve
  private float F_max; // maximum firing rate
  private float v; // minimum input voltage required to induce a spike

  public CenterOfMassEstimator(float sigma, float F_max, float v) {
    this.sigma2_2 = 2.0f * Mathf.Pow(sigma, 2.0f);
    this.F_max = F_max;
    this.v = v;
  }

  // f : [0, 1] -> [1, n]
  private static int f(float v, int n) {
    return Mathf.RoundToInt(v * n);
  }

  // f_inv : [1, n] -> [0, 1]
  private static float f_inv(int x, int n) {
    return (float)x / (float)n;
  }

  // expresses the firing rate of neuron x when the normalized value v_0 is encoded
  public void Set(double[] p, int sliceOffset, int sliceSize, int totalCount, float theta) {
    float rate;
    for (var i = 0; i < sliceSize; i++) {
      rate = F_max * Mathf.Exp(
        -1.0f * (Mathf.Pow(i - f(theta, sliceSize), 2.0f) / sigma2_2
      ));

      for (var t = 0; t < 20; t++) { // 20ms
        p[t * totalCount + (sliceOffset + i)] += RandomHelper.PoissonInput(rate, v);
      }
    }
  }

  public bool TryGet(double[] p, int sliceOffset, int sliceSize, out float theta) {
    var num = 0.0f;
    for (var i = 0; i < sliceSize; i++) {
      num += f_inv(i, sliceSize) * (float)p[sliceOffset + i];
    }

    var den = 0.0f;
    for (var i = 0; i < sliceSize; i++) {
      den += (float)p[sliceOffset + i];
    }

    if (den == 0.0f) {
      theta = 0.0f;
      return false;
    }

    theta = num / den;
    return true;
  }
}

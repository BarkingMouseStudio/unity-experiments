using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

public class CenterOfMassEstimator {

  private int N;

  private float sigma2_2; // 2 x standard deviation, squared of the gaussian curve
  private float F_max; // maximum firing rate
  private float v; // minimum input voltage required to induce a spike

  private float width; // width of the gaussian curve in the value units
  private float scale; // scaling factor applied to values

  private int binSize;

  public CenterOfMassEstimator(float sigma, float F_max, float v, int N) {
    this.sigma2_2 = 2.0f * Mathf.Pow(sigma, 2.0f);
    this.F_max = F_max;
    this.v = v;
    this.N = N;

    this.binSize = 3; // Number of neurons in the bin

    this.width = ((sigma * 3.0f) * binSize) / N;
    this.scale = 1.0f - (width * 2.0f);
  }

  // f : [0, 1] -> [1, N]
  private int f(float v) {
    v *= scale;
    v += width;
    return Mathf.RoundToInt(v * N);
  }

  // f_inv : [1, N] -> [0, 1]
  private float f_inv(int x) {
    var v = (float)x / (float)N;
    v -= width;
    v /= scale;
    return v;
  }

  private int Bin(int x) {
    return Mathf.RoundToInt(x / binSize);
  }

  // expresses the firing rate of neuron x when the normalized value v_0 is encoded
  public void Set(float[] p, int sliceOffset, int sliceSize, int totalCount, float theta) {
    float rate;
    for (var i = 0; i < sliceSize; i++) {
      rate = F_max * Mathf.Exp(
        -1.0f * (Mathf.Pow(Bin(i - f(theta)), 2.0f) / sigma2_2
      ));

      for (var t = 0; t < 20; t++) { // 20ms
        // NOTE: We use "add" here so that noise isn't cancelled out.
        p[t * totalCount + (sliceOffset + i)] += RandomHelper.PoissonInput(rate, v);
      }
    }
  }

  public bool TryGet(float[] p, int sliceOffset, int sliceSize, out float theta) {
    var num = 0.0f;
    for (var i = 0; i < sliceSize; i++) {
      num += f_inv(i) * (float)p[sliceOffset + i];
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

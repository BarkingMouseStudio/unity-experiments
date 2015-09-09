using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Population represens as single var
public class PopulationPort {

  private float[] input;
  private float[] rate;
  private int offset;
  private int size;
  private int total;

  private float min;
  private float max;

  private CenterOfMassEstimator estimator;

  public PopulationPort(float[] input, float[] rate, int offset, int size, int total, float sigma, float F_max, float v, float min, float max) {
    this.input = input;
    this.rate = rate;
    this.offset = offset;
    this.size = size;
    this.total = total;

    this.min = min;
    this.max = max;

    this.estimator = new CenterOfMassEstimator(sigma, F_max, v);
  }

  public void Set(float theta) {
    var thetaNorm = NumberHelper.Normalize(theta, min, max);
    Assert.IsTrue(thetaNorm >= 0.0f && thetaNorm <= 1.0f, thetaNorm.ToString());
    estimator.Set(input, offset, size, total, thetaNorm);
  }

  public bool TryGet(out float theta) {
    float thetaNorm;
    if (!estimator.TryGet(rate, offset, size, out thetaNorm)) {
      theta = 0.0f;
      return false;
    }

    Assert.IsTrue(thetaNorm >= 0.0f && thetaNorm <= 1.0f, thetaNorm.ToString());
    theta = NumberHelper.Scale(thetaNorm, min, max);
    return true;
  }
}

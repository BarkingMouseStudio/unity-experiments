using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Population represens as single var
public class PopulationPort {

  private double[] input;
  private double[] rate;
  private int offset;
  private int size;
  private int total;

  private CenterOfMassEstimator estimator;

  public PopulationPort(double[] input, double[] rate, int offset, int size, int total, float sigma, float F_max, float v) {
    this.input = input;
    this.rate = rate;
    this.offset = offset;
    this.size = size;
    this.total = total;
    this.estimator = new CenterOfMassEstimator(sigma, F_max, v);
  }

  public void Set(float theta) {
    Assert.IsTrue(theta >= 0.0f && theta <= 1.0f, theta.ToString());
    estimator.Set(input, offset, size, total, theta);
  }

  public bool TryGet(out float theta) {
    var success = estimator.TryGet(rate, offset, size, out theta);
    Assert.IsTrue(theta >= 0.0f && theta <= 1.0f, theta.ToString());
    return success;
  }
}

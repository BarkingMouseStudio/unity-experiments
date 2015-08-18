using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct SignReceptiveField : IReceptiveField {
  double mean;
  double sigma;

  public SignReceptiveField(double mean, double sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public double Normalize(double val) {
    var x = 1.0 - Math.Abs(val - mean) / sigma;
    if (double.IsNaN(x)) {
      return 0.0f;
    }
    return Math.Max(0.0, Math.Sign(x));
  }
}

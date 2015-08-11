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
    return Math.Max(0.0, Math.Sign(1.0 - Math.Abs(val - mean) / sigma));
  }
}

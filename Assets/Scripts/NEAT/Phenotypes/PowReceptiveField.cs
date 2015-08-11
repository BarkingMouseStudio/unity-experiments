using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct PowReceptiveField : IReceptiveField {
  double mean;
  double sigma;

  public PowReceptiveField(double mean, double sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public double Normalize(double val) {
    return NumberHelper.Clamp01(
      1.0 - Math.Pow(
        Math.Abs(val - mean) / sigma
      , 2.0)
    );
  }
}

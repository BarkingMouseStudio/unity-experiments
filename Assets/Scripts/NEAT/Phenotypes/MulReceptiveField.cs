using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct MulReceptiveField : IReceptiveField {
  double mean;
  double sigma;

  public MulReceptiveField(double mean, double sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public double Normalize(double val) {
    return (val * mean) * sigma;
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public struct MulReceptiveField : IReceptiveField {
  float mean;
  float sigma;

  public MulReceptiveField(float mean, float sigma) {
    this.mean = mean;
    this.sigma = sigma;
  }

  public float Normalize(float val) {
    return (val * mean) * sigma;
  }
}

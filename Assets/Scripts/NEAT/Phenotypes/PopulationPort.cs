using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;

// Population represens as single var
public class PopulationPort {

  private ArraySegment<double> population;
  private CenterOfMassEstimator estimator;

  public PopulationPort(ArraySegment<double> population, float sigma, float F_max) {
    this.population = population;
    this.estimator = new CenterOfMassEstimator(sigma, F_max, population.Count);
  }

  public void Set(float v) {
    Assert.IsTrue(v >= 0.0f && v <= 1.0f, v.ToString());
    estimator.Set(population.Array, population.Offset, v);
  }

  public bool TryGet(out float v) {
    var success = estimator.TryGet(population.Array, population.Offset, out v);
    Assert.IsTrue(v >= 0.0f && v <= 1.0f, v.ToString());
    return success;
  }
}

using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class EvaluatorTests {

    [Test]
    public void TestEvaluatorDuration() {
      var evaluatorLong = new Evaluator();
      var evaluatorShort = new Evaluator();

      for (int i = 0; i < 1000; i++) {
        evaluatorLong.Update(90.0f, 30.0f, 3.0f, 1.0f);
      }

      for (int i = 0; i < 100; i++) {
        evaluatorShort.Update(90.0f, 30.0f, 3.0f, 1.0f);
      }

      Assert.That(evaluatorLong.Fitness < evaluatorShort.Fitness);
    }

    [Test]
    public void TestEvaluatorInitial() {
      var evaluator = new Evaluator();
      Assert.AreEqual(1.0f, evaluator.NormalizedFitnessDuration);
      Assert.AreEqual(1.0f, evaluator.NormalizedFitnessHistory);
      Assert.AreEqual(1.0f, evaluator.Fitness);
    }

    [Test]
    public void TestEvaluatorBest() {
      var evaluator = new Evaluator();

      for (int i = 0; i < 1000; i++) {
        evaluator.Update(0.0f, 0.0f, 0.0f, 1.0f);
      }

      Assert.AreEqual(0.0f, evaluator.NormalizedFitnessDuration);
      Assert.AreEqual(0.0f, evaluator.NormalizedFitnessHistory);
      Assert.AreEqual(0.0f, evaluator.Fitness);
    }

    [Test]
    public void TestEvaluatorWorst() {
      var evaluator = new Evaluator();

      for (int i = 0; i < 100; i++) {
        evaluator.Update(180.0f, 180.0f, 6.0f, 7.0f);
      }

      Assert.AreEqual(0.9f, evaluator.NormalizedFitnessDuration);
      Assert.AreEqual(1.0f, evaluator.NormalizedFitnessHistory);
      Assert.AreEqual(0.99f, evaluator.Fitness, 0.01f);
    }
  }
}

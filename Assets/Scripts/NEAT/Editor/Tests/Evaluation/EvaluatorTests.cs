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
    public void TestEvaluatorBest() {
      var evaluator = new Evaluator();
      Assert.AreEqual(1.0f, evaluator.Fitness);

      for (int i = 0; i < 100; i++) {
        evaluator.Update(0.0f, 0.0f, 0.0f, 0.0f);
      }

      Assert.AreEqual(0.48f, evaluator.Fitness, 0.1f);
    }

    [Test]
    public void TestEvaluatorWorst() {
      var evaluator = new Evaluator();
      Assert.AreEqual(1.0f, evaluator.Fitness);

      for (int i = 0; i < 100; i++) {
        evaluator.Update(180.0f, 180.0f, 6.0f, 6.0f);
      }

      Assert.AreEqual(0.94f, evaluator.Fitness, 0.1f);
    }
  }
}

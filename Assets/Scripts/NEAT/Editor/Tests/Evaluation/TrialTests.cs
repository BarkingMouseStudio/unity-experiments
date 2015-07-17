using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class TrialTests {

    [Test]
    public void TestTrialDuration() {
      var trialLong = new Trial(Orientations.Upright, 0.0f);
      var trialShort = new Trial(Orientations.Upright, 0.0f);

      for (int i = 0; i < 1000; i++) {
        trialLong.Update(90.0f, 30.0f, 3.0f, 1.0f);
      }

      for (int i = 0; i < 100; i++) {
        trialShort.Update(90.0f, 30.0f, 3.0f, 1.0f);
      }

      Assert.That(trialLong.Fitness < trialShort.Fitness);
    }

    [Test]
    public void TestTrialInitial() {
      var trial = new Trial(Orientations.Upright, 0.0f);
      Assert.AreEqual(1.0f, trial.NormalizedFitnessDuration);
      Assert.AreEqual(1.0f, trial.NormalizedFitnessHistory);
      Assert.AreEqual(1.0f, trial.Fitness);
    }

    [Test]
    public void TestTrialBest() {
      var trial = new Trial(Orientations.Upright, 0.0f);

      for (int i = 0; i < 1000; i++) {
        trial.Update(0.0f, 0.0f, 0.0f, 1.0f);
      }

      Assert.AreEqual(0.0f, trial.NormalizedFitnessDuration);
      Assert.AreEqual(0.0f, trial.NormalizedFitnessHistory);
      Assert.AreEqual(0.0f, trial.Fitness);
    }

    [Test]
    public void TestTrialWorst() {
      var trial = new Trial(Orientations.Upright, 0.0f);

      for (int i = 0; i < 100; i++) {
        trial.Update(180.0f, 180.0f, 6.0f, 7.0f);
      }

      Assert.AreEqual(0.9f, trial.NormalizedFitnessDuration);
      Assert.AreEqual(1.0f, trial.NormalizedFitnessHistory);
      Assert.AreEqual(0.99f, trial.Fitness, 0.01f);
    }
  }
}

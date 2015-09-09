using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class ReceptiveFieldTests {

    [Test]
    public void TestSignReceptiveField() {
      var rf = new SignReceptiveField(60.0f, 30.0f);
      Assert.AreEqual(1.0f, rf.Normalize(60.0f));
      Assert.AreEqual(1.0f, rf.Normalize(75.0f));
      Assert.AreEqual(1.0f, rf.Normalize(45.0f));
      Assert.AreEqual(0.0f, rf.Normalize(90.0f));
      Assert.AreEqual(0.0f, rf.Normalize(30.0f));
    }

    [Test]
    public void TestPowReceptiveField() {
      var rf = new PowReceptiveField(60.0f, 30.0f);
      Assert.AreEqual(1.0f, rf.Normalize(60.0f));
      Assert.AreEqual(0.75f, rf.Normalize(75.0f));
      Assert.AreEqual(0.75f, rf.Normalize(45.0f));
      Assert.AreEqual(0.0f, rf.Normalize(90.0f));
      Assert.AreEqual(0.0f, rf.Normalize(30.0f));
    }
  }
}

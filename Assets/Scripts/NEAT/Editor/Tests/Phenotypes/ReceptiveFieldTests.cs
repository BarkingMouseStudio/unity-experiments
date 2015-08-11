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
      var rf = new SignReceptiveField(60.0, 30.0);
      Assert.AreEqual(1.0, rf.Normalize(60.0));
      Assert.AreEqual(1.0, rf.Normalize(75.0));
      Assert.AreEqual(1.0, rf.Normalize(45.0));
      Assert.AreEqual(0.0, rf.Normalize(90.0));
      Assert.AreEqual(0.0, rf.Normalize(30.0));
    }

    [Test]
    public void TestPowReceptiveField() {
      var rf = new PowReceptiveField(60.0, 30.0);
      Assert.AreEqual(1.0, rf.Normalize(60.0));
      Assert.AreEqual(0.75, rf.Normalize(75.0));
      Assert.AreEqual(0.75, rf.Normalize(45.0));
      Assert.AreEqual(0.0, rf.Normalize(90.0));
      Assert.AreEqual(0.0, rf.Normalize(30.0));
    }
  }
}

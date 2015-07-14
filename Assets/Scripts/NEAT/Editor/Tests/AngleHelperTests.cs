using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class AngleHelperTests {

    [Test]
    public void TestGetAngle() {
      Assert.AreEqual(0.0f, AngleHelper.GetAngle(-360.0f));
      Assert.AreEqual(0.0f, AngleHelper.GetAngle(0.0f));
      Assert.AreEqual(0.0f, AngleHelper.GetAngle(360.0f));

      Assert.AreEqual(90.0f, AngleHelper.GetAngle(-270.0f));
      Assert.AreEqual(90.0f, AngleHelper.GetAngle(90.0f));

      Assert.AreEqual(-90.0f, AngleHelper.GetAngle(-90.0f));
      Assert.AreEqual(-90.0f, AngleHelper.GetAngle(270.0f));

      Assert.AreEqual(180.0f, AngleHelper.GetAngle(180.0f));
      Assert.AreEqual(-180.0f, AngleHelper.GetAngle(-180.0f));
    }
  }
}

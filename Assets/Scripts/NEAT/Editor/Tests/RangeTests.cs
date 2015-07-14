using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class RangeTests {

    [Test]
    public void TestRange_From() {
      Assert.AreEqual(new Range[]{
        Range.Of(-180.0, -90.0),
        Range.Of(-90.0, -45.0),
        Range.Of(-45.0, -15.0),
        Range.Of(-15.0, -5.0),
        Range.Of(-5.0, -1.0),
        Range.Of(-1.0, 0.0),
        Range.Of(0.0, 1.0),
        Range.Of(1.0, 5.0),
        Range.Of(5.0, 15.0),
        Range.Of(15.0, 45.0),
        Range.Of(45.0, 90.0),
        Range.Of(90.0, 180.0),
      }, Range.From(new double[]{
        -180.0, -90.0, -45.0, -15.0, -5.0, -1.0,
        0.0,
        1.0, 5.0, 15.0, 45.0, 90.0, 180.0,
      }));
    }

    [Test]
    public void TestRange_Contains() {
      Assert.IsTrue(Range.Of(-180.0, -90.0).Contains(-180.0));
      Assert.IsTrue(Range.Of(-90.0, 0.0).Contains(-45.0));
      Assert.IsTrue(Range.Of(0.0, 90.0).Contains(45.0));
      Assert.IsTrue(Range.Of(90.0, 180.0).Contains(90.0));

      Assert.IsFalse(Range.Of(-180.0, -90.0).Contains(-90.0));
      Assert.IsFalse(Range.Of(-90.0, 0.0).Contains(45.0));
      Assert.IsFalse(Range.Of(0.0, 90.0).Contains(-45.0));
      Assert.IsFalse(Range.Of(90.0, 180.0).Contains(0.0));
    }
  }
}

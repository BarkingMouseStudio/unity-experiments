using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class SlicerTests {

    [Test]
    public void TestSlicer() {
      var slicer = new Slicer<int>(new []{0, 1, 2, 3, 4, 5, 6, 7, 8, 9});
      var a = slicer.NextSlice(5);
      var b = slicer.NextSlice(5);
      Assert.IsTrue(a.SequenceEqual(new []{0, 1, 2, 3, 4}));
      Assert.IsTrue(b.SequenceEqual(new []{5, 6, 7, 8, 9}));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException))]
    public void TestSlicer_Exception() {
      var slicer = new Slicer<int>(new []{0, 1, 2, 3, 4});
      slicer.NextSlice(5);
      slicer.NextSlice(5);
    }
  }
}

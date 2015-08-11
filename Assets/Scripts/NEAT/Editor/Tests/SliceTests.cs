using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class SliceTests {

    [Test]
    public void TestSlice() {
      var slice = new Slice<int>(new []{0, 1, 2, 3, 4, 5, 6, 7, 8, 9}, 3, 5);
      Assert.AreEqual(5, slice.Count);
      Assert.AreEqual(6, slice[3]);
      Assert.IsTrue(slice.SequenceEqual(new []{3, 4, 5, 6, 7}));
      Assert.IsTrue(slice.ToArray().SequenceEqual(new []{3, 4, 5, 6, 7}));
    }
  }
}

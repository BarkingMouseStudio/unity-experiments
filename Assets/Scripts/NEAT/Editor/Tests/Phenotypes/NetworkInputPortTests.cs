using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class NetworkInputPortTests {

    [Test]
    public void TestNetworkInputPort_Sign() {
      var rfs = EnumerableHelper.Range(0.0, 5.0)
        .Select(m => (IReceptiveField)new SignReceptiveField(m, 1.0)).ToArray();

      var arr = new []{0.0, 0.0, 0.0, 0.0, 0.0};
      Assert.AreEqual(arr.Length, rfs.Length);

      var slicer = new Slicer<double>(arr);
      var port = new NetworkInputPort(slicer, rfs);

      port.Set(3.5);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 0.0, 0.0, 30.0, 30.0}));

      port.Set(3.0);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 0.0, 0.0, 30.0, 0.0}));
    }

    [Test]
    public void TestNetworkInputPort_Pow() {
      var rfs = EnumerableHelper.Range(0.0, 5.0)
        .Select(m => (IReceptiveField)new PowReceptiveField(m, 2.0)).ToArray();

      var arr = new []{0.0, 0.0, 0.0, 0.0, 0.0};
      Assert.AreEqual(arr.Length, rfs.Length);

      var slicer = new Slicer<double>(arr);
      var port = new NetworkInputPort(slicer, rfs);

      port.Set(2.5);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 13.125, 28.125, 28.125, 13.125}));

      port.Set(2.0);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 22.5, 30.0, 22.5, 0.0}));
    }
  }
}

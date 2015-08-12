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
      var rfs = new Dictionary<int, IReceptiveField>();
      rfs[0] = new SignReceptiveField(0.0, 1.0f);
      rfs[1] = new SignReceptiveField(1.0, 1.0f);
      rfs[2] = new SignReceptiveField(2.0, 1.0f);
      rfs[3] = new SignReceptiveField(3.0, 1.0f);
      rfs[4] = new SignReceptiveField(4.0, 1.0f);

      var arr = new []{0.0, 0.0, 0.0, 0.0, 0.0};
      Assert.AreEqual(arr.Length, rfs.Count);

      var port = new NetworkInputPort(arr, rfs);

      port.Set(3.5);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 0.0, 0.0, 30.0, 30.0}));

      port.Set(3.0);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 0.0, 0.0, 30.0, 0.0}));
    }

    [Test]
    public void TestNetworkInputPort_Pow() {
      var rfs = new Dictionary<int, IReceptiveField>();
      rfs[0] = new PowReceptiveField(0.0, 2.0f);
      rfs[1] = new PowReceptiveField(1.0, 2.0f);
      rfs[2] = new PowReceptiveField(2.0, 2.0f);
      rfs[3] = new PowReceptiveField(3.0, 2.0f);
      rfs[4] = new PowReceptiveField(4.0, 2.0f);

      var arr = new []{0.0, 0.0, 0.0, 0.0, 0.0};
      Assert.AreEqual(arr.Length, rfs.Count);

      var port = new NetworkInputPort(arr, rfs);

      port.Set(2.5);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 13.125, 28.125, 28.125, 13.125}));

      port.Set(2.0);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0, 22.5, 30.0, 22.5, 0.0}));
    }
  }
}

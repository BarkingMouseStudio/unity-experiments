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
      rfs[0] = new SignReceptiveField(0.0f, 1.0f);
      rfs[1] = new SignReceptiveField(1.0f, 1.0f);
      rfs[2] = new SignReceptiveField(2.0f, 1.0f);
      rfs[3] = new SignReceptiveField(3.0f, 1.0f);
      rfs[4] = new SignReceptiveField(4.0f, 1.0f);

      var arr = new []{0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
      Assert.AreEqual(arr.Length, rfs.Count);

      var port = new NetworkInputPort(arr, rfs);

      port.Set(3.5f);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0f, 0.0f, 0.0f, 30.0f, 30.0f}));

      port.Set(3.0f);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0f, 0.0f, 0.0f, 30.0f, 0.0f}));
    }

    [Test]
    public void TestNetworkInputPort_Pow() {
      var rfs = new Dictionary<int, IReceptiveField>();
      rfs[0] = new PowReceptiveField(0.0f, 2.0f);
      rfs[1] = new PowReceptiveField(1.0f, 2.0f);
      rfs[2] = new PowReceptiveField(2.0f, 2.0f);
      rfs[3] = new PowReceptiveField(3.0f, 2.0f);
      rfs[4] = new PowReceptiveField(4.0f, 2.0f);

      var arr = new []{0.0f, 0.0f, 0.0f, 0.0f, 0.0f};
      Assert.AreEqual(arr.Length, rfs.Count);

      var port = new NetworkInputPort(arr, rfs);

      port.Set(2.5f);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0f, 13.125f, 28.125f, 28.125f, 13.125f}));

      port.Set(2.0f);
      Assert.IsTrue(arr.SequenceEqual(new []{0.0f, 22.5f, 30.0f, 22.5f, 0.0f}));
    }
  }
}

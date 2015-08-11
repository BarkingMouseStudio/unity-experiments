using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class NetworkSumOutputPortTests {

    [Test]
    public void TestNetworkSumOutputPort() {
      var multipliers = EnumerableHelper.Range(0.0, 10.0).ToArray();
      // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
      // 0, 1, 0, 1, 0, 1, 0, 1, 0, 1

      var arr = Enumerable.Range(0, 10)
        .Select(i => (double)(i % 2) * 30.0)
        .ToArray();
      Assert.AreEqual(arr.Length, multipliers.Length);

      var slicer = new Slicer<double>(arr);
      var port = new NetworkSumOutputPort(slicer, multipliers);
      var sum = port.Get();

      Assert.AreEqual(25.0, sum);
    }
  }
}

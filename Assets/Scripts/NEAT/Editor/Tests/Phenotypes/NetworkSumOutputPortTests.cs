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
      var multipliers = new Dictionary<int, IReceptiveField>();
      multipliers[0] = new MulReceptiveField(0.0, 1.0);
      multipliers[1] = new MulReceptiveField(1.0, 1.0);
      multipliers[2] = new MulReceptiveField(2.0, 1.0);
      multipliers[3] = new MulReceptiveField(3.0, 1.0);
      multipliers[4] = new MulReceptiveField(4.0, 1.0);
      multipliers[5] = new MulReceptiveField(5.0, 1.0);
      multipliers[6] = new MulReceptiveField(6.0, 1.0);
      multipliers[7] = new MulReceptiveField(7.0, 1.0);
      multipliers[8] = new MulReceptiveField(8.0, 1.0);
      multipliers[9] = new MulReceptiveField(9.0, 1.0);
      // 0, 1, 2, 3, 4, 5, 6, 7, 8, 9
      // 0, 1, 0, 1, 0, 1, 0, 1, 0, 1

      var arr = Enumerable.Range(0, 10)
        .Select(i => (double)(i % 2) * 30.0)
        .ToArray();
      Assert.AreEqual(arr.Length, multipliers.Count);

      var port = new NetworkSumOutputPort(arr, multipliers);
      var sum = port.Get();

      Assert.AreEqual(25.0, sum);
    }
  }
}

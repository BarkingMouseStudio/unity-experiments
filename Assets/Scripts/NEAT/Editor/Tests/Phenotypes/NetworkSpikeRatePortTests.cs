using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  [TestFixture]
  public class NetworkSpikeRatePortTests {

    [Test]
    public void TestNetworkSpikeRatePort_Zero() {
      var arr = new []{0.0};
      var port = new NetworkSpikeRatePort(arr);
      port.Tick();
      Assert.AreEqual(0.0, port.Rate);
      port.Tick();
      Assert.AreEqual(0.0, port.Rate);
      port.Tick();
      Assert.AreEqual(0.0, port.Rate);
    }

    [Test]
    public void TestNetworkSpikeRatePort_One() {
      var arr = new []{30.0};
      var port = new NetworkSpikeRatePort(arr);
      port.Tick();
      Assert.AreEqual(0.01, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.02, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.03, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.04, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.05, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.05, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.05, port.Rate, 0.001);
    }

    [Test]
    public void TestNetworkSpikeRatePort_Full() {
      var arr = new []{600.0};
      var port = new NetworkSpikeRatePort(arr);
      port.Tick();
      Assert.AreEqual(0.2, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.4, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.6, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(0.8, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(1.0, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(1.0, port.Rate, 0.001);
      port.Tick();
      Assert.AreEqual(1.0, port.Rate, 0.001);
    }
  }
}

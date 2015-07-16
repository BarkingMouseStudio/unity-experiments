using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using NUnit.Framework;

namespace NEAT {

  [TestFixture]
  public class DistanceMetricTests {

    Genotype genotypeA, genotypeB, genotypeC;

    [SetUp]
    public void Init() {
      var neuronGenesA = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i))
        .ToGeneList();
      var synapseGenesA = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(i, 0, 1, true, 0.5f))
        .ToGeneList();

      genotypeA = new Genotype(neuronGenesA, synapseGenesA);
      genotypeB = new Genotype(genotypeA);

      var neuronGenesC = Enumerable.Range(0, 3)
        .Select(i => new NeuronGene(i))
        .ToGeneList();
      var synapseGenesC = Enumerable.Range(0, 3)
        .Select(i => new SynapseGene(i + 1, -1, 1, true, 0.0f))
        .ToGeneList();

      genotypeC = new Genotype(neuronGenesC, synapseGenesC);
    }

    [Test]
    public void TestMeasureDistanceThreshold() {
      var distanceMetric = new DistanceMetric();
      Assert.That(distanceMetric.MeasureDistance(genotypeA, genotypeB, 0.0f));
      Assert.That(!distanceMetric.MeasureDistance(genotypeA, genotypeC, 0.0f));
    }

    [Test]
    public void TestMeasureDistanceDisjoint() {
      var distanceMetric = new DistanceMetric(1.0f, 0.0f, 0.0f);

      var distanceAB = distanceMetric.MeasureDistance(genotypeA, genotypeB);
      Assert.AreEqual(0.0f, distanceAB);

      var distanceAC = distanceMetric.MeasureDistance(genotypeA, genotypeC);
      Assert.AreEqual(1f / 3f, distanceAC);
    }

    [Test]
    public void TestMeasureDistanceExcess() {
      var distanceMetric = new DistanceMetric(0.0f, 1.0f, 0.0f);

      var distanceAB = distanceMetric.MeasureDistance(genotypeA, genotypeB);
      Assert.AreEqual(0.0f, distanceAB);

      var distanceAC = distanceMetric.MeasureDistance(genotypeA, genotypeC);
      Assert.AreEqual(1f / 3f, distanceAC);
    }

    [Test]
    public void TestMeasureDistanceMutation() {
      var distanceMetric = new DistanceMetric(0.0f, 0.0f, 1.0f);

      var distanceAB = distanceMetric.MeasureDistance(genotypeA, genotypeB);
      Assert.AreEqual(0.0f, distanceAB);

      var distanceAC = distanceMetric.MeasureDistance(genotypeA, genotypeC);
      Assert.AreEqual(1f / 3f, distanceAC);
    }
  }
}

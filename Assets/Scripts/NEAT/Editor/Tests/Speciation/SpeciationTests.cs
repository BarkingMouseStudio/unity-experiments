using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Speciation")]
  public class SpeciationTests {

    Phenotype[] phenotypes;

    [SetUp]
    public void Init() {
      var innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetInitialNeuronInnovationId(i);
        return new NeuronGene(inId);
      }).ToArray();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(-1, 1);
        return new SynapseGene(inId, -1, 1, true);
      }).ToArray();

      var protoGenotype = new Genotype(neuronGenes, synapseGenes);

      phenotypes = Enumerable.Range(0, 100).Select(_ => {
        var gt = Genotype.FromPrototype(protoGenotype);
        var pt = new Phenotype(gt, Random.value * 100.0f, 10.0f, 180.0f);
        return pt;
      }).ToArray();
    }

    [Test]
    public void TestSpeciation() {
      var distanceMetric = new NEAT.DistanceMetric(0.0f, 0.0f, 35.0f);
      var speciation = new NEAT.Speciation(10, 50.0f, 0.2f, distanceMetric);
      var species = speciation.Speciate(new NEAT.Specie[0], phenotypes);

      Assert.AreEqual(15, species.Length, 5);
      Assert.AreEqual(60.0f, speciation.DistanceThreshold);
    }
  }
}

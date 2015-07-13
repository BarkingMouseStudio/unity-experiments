using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Selection")]
  public class OffspringSelectorTests {

    Specie[] species;

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

      var phenotypes = Enumerable.Range(0, 100).Select(_ => {
        var gt = Genotype.FromPrototype(protoGenotype);
        var pt = new Phenotype(gt, Random.value * 100.0f, 10.0f, 180.0f);
        return pt;
      }).ToArray();

      var distanceMetric = new NEAT.DistanceMetric(0.0f, 0.0f, 35.0f);
      var speciation = new NEAT.Speciation(10, 50.0f, 0.2f, distanceMetric);
      species = speciation.Speciate(new NEAT.Specie[0], phenotypes);
    }

    [Test]
    public void TestOffspringSelector() {
      var crossover = new NEAT.MultipointCrossover();
      var offspringSelector = new OffspringSelector(crossover);

      var offspringCount = 100;
      var offspring = offspringSelector.Select(species, offspringCount);
      Assert.AreEqual(offspringCount, offspring.Length);
    }
  }
}

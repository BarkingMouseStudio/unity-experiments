using UnityEngine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace NEAT {

  [TestFixture]
  [Category("Speciation")]
  public class SpecieTests {

    Genotype protoGenotype;

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

      protoGenotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestSpecie() {
      var specie = new Specie(13, protoGenotype);

      for (int i = 0; i < 100; i++) {
        var gt = Genotype.FromPrototype(protoGenotype);
        var pt = new Phenotype(gt, Random.value * 100.0f, 10.0f, 180.0f);
        specie.Add(pt);
      }

      foreach (var pt in specie) {
        pt.AdjustedFitness = pt.Fitness / specie.Count;
      }

      Assert.AreEqual(0.5f, specie.MeanFitness, 0.1f);
      Assert.AreEqual(100, specie.Count);
    }
  }
}

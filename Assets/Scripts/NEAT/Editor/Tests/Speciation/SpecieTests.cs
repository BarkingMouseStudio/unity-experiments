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
        return new NeuronGene(inId, NeuronType.HiddenNeuron);
      }).ToGeneList();

      var synapseGenes = Enumerable.Range(0, 3).Select(i => {
        var inId = innovations.GetSynapseInnovationId(i, i + 1);
        return new SynapseGene(inId, i, i + 1, true);
      }).ToGeneList();

      protoGenotype = new Genotype(neuronGenes, synapseGenes);
    }

    [Test]
    public void TestSpecie() {
      var specie = new Specie(13, protoGenotype, 0, 0, 0.0f);

      for (int i = 0; i < 100; i++) {
        var gt = Genotype.FromPrototype(protoGenotype);
        var pt = new Phenotype(gt);
        specie.Add(pt);
      }

      foreach (var pt in specie) {
        pt.AdjustedFitness = 1.0f / (float)specie.Count;
      }

      Assert.AreEqual(0.0f, specie.MeanFitness, 0.001f);
      Assert.AreEqual(0.01f, specie.MeanAdjustedFitness, 0.001f);
      Assert.AreEqual(100, specie.Count);
    }
  }
}

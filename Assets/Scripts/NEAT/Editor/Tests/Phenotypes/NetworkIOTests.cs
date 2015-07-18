using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using System.Linq;

namespace NEAT {

  // NOTE: These aren't very good tests since they only test whether
  // exceptions are thrown.

  [TestFixture]
  public class NetworkIOTests {

    InnovationCollection innovations;
    Genotype protoGenotype;

    [SetUp]
    public void Init() {
      innovations = new InnovationCollection();

      var neuronGenes = Enumerable.Range(0, NetworkIO.InitialNeuronCount)
        .Select(i => {
          var innovationId = innovations.GetInitialNeuronInnovationId(i);
          return new NeuronGene(innovationId);
        })
        .ToGeneList();

      protoGenotype = new Genotype(neuronGenes, new GeneList<SynapseGene>(0));
    }

    [Test]
    public void TestNetworkIO_FromGenotype() {
      var genotype = Genotype.FromPrototype(protoGenotype);

      var mutators = new IMutator[]{
        new AddNeuronMutator(innovations),
        new AddSynapseMutator(innovations),
      };

      // Ensure that the network remains valid after a number of
      // structural mutations.
      var generationCount = 100;
      for (int i = 0; i < generationCount; i++) {
        var results = new MutationResults();
        foreach (var mutator in mutators) {
          mutator.Mutate(genotype, results);
        }
        NetworkIO.FromGenotype(genotype);
      }
    }

    [Test]
    [Ignore]
    public void TestNetworkIO_Send() {
      var genotype = Genotype.FromPrototype(protoGenotype);
      var network = NetworkIO.FromGenotype(genotype);
      network.Send(90.0f, 30.0f, 90.0f, 90.0f, 3.0f, 1.0f);
    }

    [Test]
    public void TestNetworkIO_PopulateWorldData() {
      var worldData = new float[NetworkIO.inNeuronCount];
      NetworkIO.PopulateWorldData(worldData, 1.0f, 2.0f, 1.0f, 2.0f, 3.0f, 4.0f);
      // Debug.LogFormat("World Data: {0}", worldData.Stringify());
      Assert.AreEqual("1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,1,1,1,1,1,2,2,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3,4,4,4,4,4,4",
        worldData.Stringify());
    }

    [Test]
    public void TestNetworkIO_MapInput() {
      var worldData = new float[NetworkIO.inNeuronCount];
      var input = new double[NetworkIO.InitialNeuronCount];
      NetworkIO.PopulateWorldData(worldData, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f);

      NetworkIO.MapInput(input, worldData);
      // Debug.LogFormat("Input: {0}", input.Stringify());
      Assert.AreEqual("0,0,0,0,0,0,0,30,0,0,0,0,0,0,0,0,0,0,0,30,0,0,0,0,0,0,0,0,0,0,0,30,0,0,0,0,0,0,0,0,0,0,0,30,0,0,0,0,0,0,0,0,30,0,0,0,0,0,30,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0",
        input.Stringify());
    }

    [Test]
    public void TestNetworkIO_MapOutput() {
      var output = new double[NetworkIO.InitialNeuronCount];
      output.Fill(30.0f);

      var speed = NetworkIO.MapOutput(output);
      // Debug.LogFormat("Output: {0}", output.Stringify());
      Assert.AreEqual(0.0f, speed); // Speeds cancel eachother out
      Assert.AreEqual("30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30",
        output.Stringify());
    }
  }
}

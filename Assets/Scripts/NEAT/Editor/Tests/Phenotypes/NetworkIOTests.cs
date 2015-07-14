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
        .Select(i => new NeuronGene(innovations.GetInitialNeuronInnovationId(i)))
        .ToArray();

      protoGenotype = new Genotype(neuronGenes, new SynapseGene[0]);
    }

    [Test]
    public void TestNetworkIO_FromGenotype() {
      var genotype = Genotype.FromPrototype(protoGenotype);

      var mutators = new IMutator[]{
        new AddNeuronMutator(1.0f, innovations),
        new AddSynapseMutator(1.0f, innovations),
      };

      // Ensure that the network remains valid after a number of
      // structural mutations.
      var generationCount = 100;
      for (int i = 0; i < generationCount; i++) {
        var results = new MutationResults();
        foreach (var mutator in mutators) {
          mutator.MutateGenotype(genotype, results);
        }
        NetworkIO.FromGenotype(genotype);
      }
    }

    [Test]
    public void TestNetworkIO_Send() {
      var genotype = Genotype.FromPrototype(protoGenotype);
      var network = NetworkIO.FromGenotype(genotype);
      network.Send(30.0f, 3.0f);
    }
  }
}

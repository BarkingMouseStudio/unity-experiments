using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using System.IO;

// Responsible for spawning evaluation prefabs, waiting for their completion and
// passing the phenotypes back to the EA.
public class EvolutionBehaviour : MonoBehaviour {

  public Transform prefab;
  private readonly int batchSize = 100;

  static NEAT.Genotype[][] CreateBatches(NEAT.Genotype[] genotypes, int batchSize) {
    return genotypes.Select((gt, i) => {
      return new {
        Batch = Mathf.FloorToInt(i / batchSize),
        Genotype = gt,
      };
    }).GroupBy((gt) => {
      return gt.Batch;
    }).Select((grp) => {
      return grp.Select(g => g.Genotype).ToArray();
    }).ToArray();
  }

  IEnumerator EvaluateBatch(int batchIndex, NEAT.Genotype[] batch, SortedList<float, NEAT.Phenotype> phenotypes) {
    IList<EvaluationBehaviour> evaluations = new List<EvaluationBehaviour>();

    var layout = new TransformLayout(28.0f, 18.0f, batchSize, Mathf.FloorToInt(Mathf.Sqrt(batchSize)));

    foreach (var genotype in batch) {
      var t = PoolManager.Pools["Evaluations"].Spawn(prefab, layout.NextPosition(), Quaternion.identity, transform);

      var controllerBehaviour = t.GetComponent<ControllerBehaviour>();
      controllerBehaviour.Network = NetworkIO.FromGenotype(genotype);

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluationBehaviour.Genotype = genotype;

      evaluations.Add(evaluationBehaviour);
    }

    // Wait for evaluations to complete
    while (evaluations.Any(ev => !ev.IsComplete)) {
      yield return new WaitForFixedUpdate();
    }

    var batchPhenotypes = evaluations.Select(ev =>
      new NEAT.Phenotype(ev.Genotype, ev.Fitness, ev.Now, ev.Angle));

    var batchPhenotypePairs = batchPhenotypes.Select(pt =>
      new KeyValuePair<float, NEAT.Phenotype>(pt.Fitness, pt));

    // Accumulate fitnesses into array
    phenotypes.AddRange(batchPhenotypePairs);

    // Cleanup
    List<Transform> children = new List<Transform>(transform.childCount);
    foreach (Transform child in transform) {
      if (child.gameObject.activeInHierarchy) {
        children.Add(child);
      }
    }
    foreach (Transform child in children) {
      PoolManager.Pools["Evaluations"].Despawn(child, null);
    }
  }

  IEnumerator EvaluateBatches(NEAT.Genotype[][] batches, SortedList<float, NEAT.Phenotype> phenotypes) {
    int batchIndex = 0;
    foreach (var batch in batches) {
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, phenotypes));
      batchIndex++;
    }
  }

  IEnumerator EvaluatePopulation(NEAT.Genotype[] genotypes, SortedList<float, NEAT.Phenotype> phenotypes) {
    // Create batches from the population
    var batches = CreateBatches(genotypes, batchSize);
    yield return StartCoroutine(EvaluateBatches(batches, phenotypes));
  }

  StreamWriter elitesLog;
  StreamWriter eliteFitnessLog;
  StreamWriter speciesLog;

  void OnApplicationQuit() {
    elitesLog.Close();
    eliteFitnessLog.Close();
    speciesLog.Close();
  }

  IEnumerator Start() {
    elitesLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/.logs/elites.csv"));
    eliteFitnessLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/.logs/elite_fitness.csv"));
    speciesLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/.logs/species.csv"));

    var populationSize = 500;
    var innovations = new NEAT.InnovationCollection();

    var distanceMetric = new NEAT.DistanceMetric(3.0f, 3.0f, 2.0f);
    var mutations = new NEAT.MutationCollection(
      new NEAT.AddNeuronMutator(0.2f, innovations),
      new NEAT.AddSynapseMutator(0.2f, innovations),
      new NEAT.PerturbNeuronMutator(0.25f, 0.5f),
      new NEAT.PerturbSynapseMutator(0.25f, 0.5f),
      new NEAT.ReplaceNeuronMutator(0.15f),
      new NEAT.ReplaceSynapseMutator(0.15f),
      new NEAT.ToggleSynapseMutator(0.1f)
    );
    var crossover = new NEAT.MultipointCrossover();
    var speciation = new NEAT.Speciation(10, 50.0f, 0.2f, distanceMetric);
    var eliteSelector = new NEAT.EliteSelector();
    var offspringSelector = new NEAT.OffspringSelector(crossover);

    var neuronGenes = Enumerable.Range(0, NetworkIO.InitialNeuronCount)
      .Select(i => NEAT.NeuronGene.Random(innovations.GetInitialNeuronInnovationId(i)))
      .ToArray();
    var synapseGenes = new NEAT.SynapseGene[0];
    var protoGenotype = new NEAT.Genotype(neuronGenes, synapseGenes);

    var genotypes = new NEAT.GenotypeStream(protoGenotype, innovations)
      .Take(populationSize).ToArray();

    var species = new NEAT.Specie[0];
    var generation = 0;

    while (true) {
      var phenotypes = new SortedList<float, NEAT.Phenotype>(genotypes.Length);
      yield return StartCoroutine(EvaluatePopulation(genotypes, phenotypes));

      species = speciation.Speciate(species, phenotypes.Values.ToArray());

      var elites = eliteSelector.Select(species, species.Length);
      Assert.AreEqual(elites.Length, species.Length,
        "Elite must be selected for each species");

      var offspringCount = populationSize - elites.Length;
      var offspring = offspringSelector.Select(species, offspringCount);
      Assert.AreEqual(offspring.Length, offspringCount,
        "Correct number of offspring must be produced");

      var mutationResults = mutations.Mutate(offspring);

      var best = phenotypes.Values.First();
      Debug.LogFormat("[{0}] Generation completed. Best Duration: {1}s Best Fitness: {2} ({3}, {4})",
        generation, best.Duration, best.Fitness,
        best.Genotype.NeuronCount,
        best.Genotype.SynapseCount);

      Debug.LogFormat("\tMutations: Added Neurons: {0}, Added Synapses: {1}, Perturbed Neurons: {2}, Perturbed Synapses: {3}, Replaced Neurons: {4}, Replaced Synapses: {5}",
        mutationResults.addedNeurons / populationSize,
        mutationResults.addedSynapses / populationSize,
        mutationResults.perturbedNeurons / populationSize,
        mutationResults.perturbedSynapses / populationSize,
        mutationResults.replacedNeurons / populationSize,
        mutationResults.replacedSynapses / populationSize);

      foreach (var sp in species) {
        speciesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}", generation, sp.SpeciesId, sp.Count, sp.MeanFitness));

        var spBest = sp.First();
        eliteFitnessLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}",
          generation, sp.SpeciesId, spBest.Fitness, spBest.AdjustedFitness,
          spBest.Duration));
        elitesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
          generation, sp.SpeciesId, spBest.Fitness, spBest.AdjustedFitness,
          spBest.Duration, JSON.Serialize(spBest.Genotype.ToJSON())));
      }

      genotypes = elites.Concat(offspring).ToArray();
      Assert.AreEqual(genotypes.Length, populationSize,
        "Population size must remain constant");

      generation++;
    }
  }
}

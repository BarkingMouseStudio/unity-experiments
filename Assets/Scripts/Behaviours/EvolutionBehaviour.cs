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

  IEnumerator EvaluateBatch(int batchIndex, NEAT.Genotype[] batch, List<NEAT.Phenotype> phenotypes) {
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

    // Accumulate fitnesses into array
    phenotypes.AddRange(batchPhenotypes);

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

  IEnumerator EvaluateBatches(NEAT.Genotype[][] batches, List<NEAT.Phenotype> phenotypes) {
    int batchIndex = 0;
    foreach (var batch in batches) {
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, phenotypes));
      batchIndex++;
    }
  }

  IEnumerator EvaluatePopulation(NEAT.Genotype[] genotypes, List<NEAT.Phenotype> phenotypes) {
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

    var mutations = new NEAT.MutationCollection(
      new NEAT.AddNeuronMutator(0.35f, innovations),
      new NEAT.AddSynapseMutator(0.35f, innovations),
      new NEAT.PerturbNeuronMutator(0.25f, 0.5f),
      new NEAT.PerturbSynapseMutator(0.25f, 0.5f),
      new NEAT.ReplaceNeuronMutator(0.15f),
      new NEAT.ReplaceSynapseMutator(0.15f),
      new NEAT.ToggleSynapseMutator(0.15f)
    );

    var eliteSelector = new NEAT.EliteSelector();

    var crossover = new NEAT.MultipointCrossover();
    var offspringSelector = new NEAT.OffspringSelector(crossover);

    var distanceMetric = new NEAT.DistanceMetric(3.0f, 3.0f, 2.0f);
    var speciation = new NEAT.Speciation(10, 6.0f, 0.1f, distanceMetric);

    var neuronGenes = Enumerable.Range(0, NetworkIO.InitialNeuronCount)
      .Select(i => NEAT.NeuronGene.Random(innovations.GetInitialNeuronInnovationId(i)))
      .ToArray();
    var synapseGenes = new NEAT.SynapseGene[0];
    var protoGenotype = new NEAT.Genotype(neuronGenes, synapseGenes);

    var genotypes = new NEAT.GenotypeStream(protoGenotype)
      .Take(populationSize).ToArray();

    var species = new NEAT.Specie[0];
    var generation = 0;

    while (true) {
      var phenotypes = new List<NEAT.Phenotype>(genotypes.Length);
      yield return StartCoroutine(EvaluatePopulation(genotypes, phenotypes));

      var longest = phenotypes.OrderByDescending(pt => pt.Duration).First();
      Debug.LogFormat("[{0}] Longest Fitness: {1}, Longest Duration: {2}s ({3}, {4})",
        generation, longest.Fitness, longest.Duration,
        longest.Genotype.NeuronCount,
        longest.Genotype.SynapseCount);

      var best = phenotypes.OrderBy(pt => pt.Fitness).First();
      Debug.LogFormat("[{0}] Best Fitness: {1}, Best Duration: {2}s ({3}, {4})",
        generation, best.Fitness, best.Duration,
        best.Genotype.NeuronCount,
        best.Genotype.SynapseCount);

      species = speciation.Speciate(species, phenotypes.ToArray());

      var adjusted = phenotypes.OrderBy(pt => pt.AdjustedFitness).First();
      Debug.LogFormat("[{0}] Best Adjusted Fitness: {1}, Best Adjusted Duration: {2}s ({3}, {4})",
        generation, adjusted.Fitness, adjusted.Duration,
        adjusted.Genotype.NeuronCount,
        adjusted.Genotype.SynapseCount);

      Debug.LogFormat("[{0}] Species Count: {1} (Threshold: {2})",
        generation, species.Length, speciation.DistanceThreshold);

      foreach (var sp in species) {
        // Debug.LogFormat("[{0}] Species Id: {1}, Species Size: {2}, Mean Fitness: {3}",
        //   generation, sp.SpeciesId, sp.Count, sp.MeanFitness);

        speciesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}",
          generation, sp.SpeciesId, sp.Count, sp.MeanFitness));

        var spBest = sp.First();
        eliteFitnessLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}",
          generation, sp.SpeciesId, spBest.Fitness, spBest.AdjustedFitness,
          spBest.Duration));
        elitesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}",
          generation, sp.SpeciesId, spBest.Fitness, spBest.AdjustedFitness,
          spBest.Duration, JSON.Serialize(spBest.Genotype.ToJSON())));
      }

      var elites = eliteSelector.Select(species, species.Length);
      Assert.AreEqual(elites.Length, species.Length,
        "Must select elite from each species");

      var offspringCount = populationSize - elites.Length;
      var offspring = offspringSelector.Select(species, offspringCount);
      Assert.AreEqual(offspring.Length, offspringCount,
        "Must produce the correct number of offspring");

      var mutationResults = mutations.Mutate(offspring);
      Debug.LogFormat("[{0}] Mutations: Added Neurons: {1}, Added Synapses: {2}, Perturbed Neurons: {3}, Perturbed Synapses: {4}, Replaced Neurons: {5}, Replaced Synapses: {6}",
        generation,
        mutationResults.addedNeurons,
        mutationResults.addedSynapses,
        mutationResults.perturbedNeurons,
        mutationResults.perturbedSynapses,
        mutationResults.replacedNeurons,
        mutationResults.replacedSynapses);

      genotypes = elites.Concat(offspring).ToArray();
      Assert.AreEqual(genotypes.Length, populationSize,
        "Population size must remain constant");

      generation++;
    }
  }
}

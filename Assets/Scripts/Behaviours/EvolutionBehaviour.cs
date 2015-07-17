using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using PathologicalGames;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NEAT;

// Responsible for spawning evaluation prefabs, waiting for their completion and
// passing the phenotypes back to the EA.
public class EvolutionBehaviour : MonoBehaviour {

  public Transform prefab;
  private readonly int batchSize = 100;

  IEnumerator EvaluateBatch(int batchIndex, Phenotype[] batch, Orientations orientation) {
    IList<EvaluationBehaviour> evaluations = new List<EvaluationBehaviour>();

    var layout = new TransformLayout(28.0f, 18.0f, batchSize, Mathf.FloorToInt(Mathf.Sqrt(batchSize)));

    foreach (var phenotype in batch) {
      var t = PoolManager.Pools["Evaluations"].Spawn(prefab, layout.NextPosition(), Quaternion.identity, transform);

      var controllerBehaviour = t.GetComponent<ControllerBehaviour>();
      controllerBehaviour.Network = NetworkIO.FromGenotype(phenotype.Genotype);

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluationBehaviour.Phenotype = phenotype;
      evaluationBehaviour.BeginTrial(orientation, Time.time);

      evaluations.Add(evaluationBehaviour);
    }

    // Wait for evaluations to complete
    while (evaluations.Any(ev => !ev.IsComplete)) {
      yield return new WaitForFixedUpdate();
    }

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

  IEnumerator EvaluateBatches(Phenotype[][] batches) {
    int batchIndex = 0;
    foreach (var batch in batches) {
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, Orientations.HardLeft));
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, Orientations.HardRight));
      batchIndex++;
    }
  }

  IEnumerator EvaluatePopulation(List<Phenotype> phenotypes) {
    yield return StartCoroutine(EvaluateBatches(phenotypes.Batch(batchSize)));
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

    var populationSize = 200;
    var innovations = new InnovationCollection();

    var mutations = new MutationCollection();
    mutations.Add(0.001f, new AddNeuronMutator(innovations)); // 0.1%
    mutations.Add(0.01f, new AddSynapseMutator(innovations)); // 1%
    mutations.Add(0.18f, new PerturbNeuronMutator(0.15f, 0.5f)); // 98% vvv
    mutations.Add(0.18f, new PerturbSynapseMutator(0.15f, 0.5f));
    mutations.Add(0.18f, new ToggleSynapseMutator(0.15f));
    mutations.Add(0.18f, new ReplaceNeuronMutator(0.15f));
    mutations.Add(0.18f, new ReplaceSynapseMutator(0.15f));
    // TODO: Modes
    // TODO: mutations.Add(0.10f, new PruneSynapseMutator(0.15f)); // 0.1%
    // TODO: Pruning mutator: deletes disabled synapses, removes orphaned neurons
    mutations.Add(0.089f, new NoopMutator());

    var eliteSelector = new EliteSelector();

    var crossover = new MultipointCrossover();
    var offspringSelector = new OffspringSelector(crossover);

    var distanceMetric = new DistanceMetric(3.0f, 3.0f, 2.0f);
    var speciation = new Speciation(10, 6.0f, 0.1f, distanceMetric);

    var neuronGenes = Enumerable.Range(0, NetworkIO.InitialNeuronCount)
      .Select(i => NeuronGene.Random(innovations.GetInitialNeuronInnovationId(i)))
      .ToGeneList();
    var synapseGenes = new GeneList<SynapseGene>();
    var protoGenotype = new Genotype(neuronGenes, synapseGenes);

    var genotypes = new GenotypeStream(protoGenotype)
      .Take(populationSize).ToArray();

    var species = new Specie[0];
    var generation = 0;

    while (true) {
      var phenotypes = genotypes.Select(gt => new Phenotype(gt)).ToList();
      yield return StartCoroutine(EvaluatePopulation(phenotypes));

      var longest = phenotypes.OrderByDescending(pt => pt.AverageDuration).First();
      Debug.LogFormat("[{0}] Longest Fitness: {1}, Longest Duration: {2}s ({3}, {4})",
        generation, longest.Fitness, longest.AverageDuration,
        longest.Genotype.NeuronCount,
        longest.Genotype.SynapseCount);

      var best = phenotypes.OrderBy(pt => pt.Fitness).First();
      Debug.LogFormat("[{0}] Best Fitness: {1}, Best Duration: {2}s ({3}, {4})",
        generation, best.Fitness, best.AverageDuration,
        best.Genotype.NeuronCount,
        best.Genotype.SynapseCount);

      eliteFitnessLog.WriteLine(string.Format("{0}, {1}, {2}",
        generation, best.Fitness, best.AverageDuration));
      elitesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}",
        generation, best.Fitness, best.AverageDuration,
        JSON.Serialize(best.Genotype.ToJSON())));

      species = speciation.Speciate(species, phenotypes.ToArray());

      var adjusted = phenotypes.OrderBy(pt => pt.AdjustedFitness).First();
      Debug.LogFormat("[{0}] Best Adjusted Fitness: {1}, Best Adjusted Duration: {2}s ({3}, {4})",
        generation, adjusted.Fitness, adjusted.AverageDuration,
        adjusted.Genotype.NeuronCount,
        adjusted.Genotype.SynapseCount);

      Debug.LogFormat("[{0}] Species Count: {1} (Threshold: {2})",
        generation, species.Length, speciation.DistanceThreshold);

      foreach (var sp in species) {
        // Debug.LogFormat("[{0}] Species Id: {1}, Species Size: {2}, Mean Fitness: {3}",
        //   generation, sp.SpeciesId, sp.Count, sp.MeanFitness);

        speciesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}",
          generation, sp.SpeciesId, sp.Count, sp.MeanFitness));
      }

      var elites = eliteSelector.Select(species, species.Length);
      Assert.AreEqual(elites.Length, species.Length,
        "Must select elite from each species");

      var offspringCount = populationSize - elites.Length;
      var offspring = offspringSelector.Select(species, offspringCount);
      Assert.AreEqual(offspring.Length, offspringCount,
        "Must produce the correct number of offspring");

      var mutationResults = mutations.Mutate(offspring);
      Debug.LogFormat("[{0}] Mutations: Added Neurons: {1}, Added Synapses: {2}, Perturbed Neurons: {3}, Perturbed Synapses: {4}, Toggled Synapses: {7}, Replaced Neurons: {5}, Replaced Synapses: {6}",
        generation,
        mutationResults.addedNeurons,
        mutationResults.addedSynapses,
        mutationResults.perturbedNeurons,
        mutationResults.perturbedSynapses,
        mutationResults.replacedNeurons,
        mutationResults.replacedSynapses,
        mutationResults.toggledSynapses);

      genotypes = elites.Concat(offspring).ToArray();
      Assert.AreEqual(genotypes.Length, populationSize,
        "Population size must remain constant");

      generation++;
    }
  }
}

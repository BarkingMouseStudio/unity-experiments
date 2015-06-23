using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;

// Responsible for spawning evaluation prefabs, waiting for their completion and
// passing the results back to the EA.
public class EvolutionBehaviour : MonoBehaviour {

  public Transform prefab;

  int subpopulationSize = 100;
  int sampleSize = 50;
  int batchSize = 50;

  IEvolutionaryAlgorithm algorithm;

  CommonGenotype[][] CreateBatches(CommonGenotype[] genotypes, int batchSize) {
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

  IEnumerator EvaluateBatch(int batchIndex, CommonGenotype[] batch, List<float> fitness) {
    IList<EvaluationBehaviour> evaluations = new List<EvaluationBehaviour>();

    float spacing = 28.0f;
    int count = batchSize;
    int stride = batchSize;

    float width = (count - 1) * spacing;
    Vector3 offset = new Vector3(-width / 2, 0, 0);

    int i = 0;
    foreach (var genotype in batch) {
      float x = i % stride;

      Vector3 position = offset + new Vector3(x * spacing, 0, 0);
      Quaternion rotation = Quaternion.identity;

      var t = PoolManager.Pools["Evaluations"].Spawn(prefab, position, rotation, transform);
      var controllerBehaviour = t.GetComponent<ControllerBehaviour>();

      controllerBehaviour.networkIO = new NetworkIO(genotype.GetPhenotype());

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluations.Add(evaluationBehaviour);

      i++;
    }

    // Wait for evaluations to complete
    while (!evaluations.All(ev => ev.IsComplete)) {
      yield return new WaitForFixedUpdate();
    }

    // Accumulate fitnesses into array
    fitness.AddRange(evaluations.Select(ev => ev.Fitness));

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

  IEnumerator EvaluateBatches(CommonGenotype[][] batches, List<float> fitness) {
    // Setup fitness tests
    int batchIndex = 0;
    foreach (var batch in batches) {
      var batchFitness = new List<float>(batch.Length);
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, batchFitness));
      batchIndex++;

      // Debug.LogFormat("[{0}:{1}] Batch complete. ({2})", algorithm.Generation, batchIndex, batchFitness.Min(f => f));

      fitness.AddRange(batchFitness);
    }
  }

  IEnumerator EvaluatePopulation(CommonGenotype[] genotypes, List<float> fitness) {
    // Create batches from the population
    var batches = CreateBatches(genotypes, batchSize);
    yield return StartCoroutine(EvaluateBatches(batches, fitness));
    // Debug.LogFormat("[{0}] Population complete. ({1})", algorithm.Generation, fitness.Min(f => f));
  }

  IEnumerator Start() {
    // Setup the algorithm with the proto-genotype
    algorithm = new EnforcedSubpopulations(new CommonGenotype(), subpopulationSize);
    var start = DateTime.Now;

    while (true) {
      // Sample from the EA
      var population = algorithm.Sample(sampleSize);

      List<float> fitness = new List<float>(population.Length);
      yield return StartCoroutine(EvaluatePopulation(population, fitness));

      var best = fitness.Zip(population, (f, genotype) => {
        return Tuple.Of(f, genotype);
      }).OrderBy(f => f.First).First();

      // Update the EA's internals
      var results = algorithm.Update(fitness.ToArray());
      Debug.LogFormat("[{0}] Generation completed at {1}. (Best: {2}, Average Trials: {3}, Offspring: {4}, Mutations: {5})",
        algorithm.Generation, DateTime.Now - start, best.First,
        results.First, results.Second, results.Third);
      Debug.LogFormat("\t{0}", best.Second.ToString());
    }
  }
}

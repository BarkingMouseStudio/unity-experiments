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

  int populationSize = 1000;
  int subpopulationSize = 100;
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

    // Keep track of lowest fitness
    Debug.LogFormat("[{0}:{1}] Evaluations complete. ({3})", algorithm.Generation, batchIndex,
      evaluations.Min(ev => ev.Fitness));

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
      var batchFitnessA = new List<float>(batch.Length);
      var batchFitnessB = new List<float>(batch.Length);

      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, batchFitnessA));
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, batchFitnessB));
      batchIndex++;

      var batchFitness = batchFitnessA.Zip(batchFitnessB, (a, b) => {
        return (a + b + Mathf.Abs(a - b)) / 3f;
      });

      Debug.LogFormat("[{0}:{1}] Batch complete. ({1})", algorithm.Generation, batchIndex, batchFitness.Min(f => f));

      fitness.AddRange(batchFitness);
    }
  }

  IEnumerator EvaluatePopulation(CommonGenotype[] genotypes, List<float> fitness) {
    // Create batches from the population
    var batches = CreateBatches(genotypes, batchSize);
    yield return StartCoroutine(EvaluateBatches(batches, fitness));
  }

  IEnumerator Start() {
    // Setup the algorithm with the proto-genotype
    algorithm = new EnforcedSubpopulations(new CommonGenotype(), subpopulationSize);

    while (true) {
      // Sample from the EA
      var population = algorithm.Sample(populationSize);

      List<float> fitness = new List<float>(population.Length);
      yield return StartCoroutine(EvaluatePopulation(population, fitness));

      // Update the EA's internals
      algorithm.Update(fitness.ToArray());
    }
  }
}

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;
using System.IO;

// Responsible for spawning evaluation prefabs, waiting for their completion and
// passing the results back to the EA.
public class EvolutionBehaviour : MonoBehaviour {

  public Transform prefab;

  int subpopulationSize = 100;
  int sampleSize = 30;
  int batchSize = 30;

  IEvolutionaryAlgorithm algorithm;

  public struct Result {
    public float fitness;
    public float duration;
    public float orientation;
  }

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

  IEnumerator EvaluateBatch(int batchIndex, CommonGenotype[] batch, List<Result> results) {
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
    results.AddRange(evaluations.Select(ev => {
      return new Result{
        fitness = ev.Fitness,
        duration = ev.Now,
        orientation = ev.Angle,
      };
    }));

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

  IEnumerator EvaluateBatches(CommonGenotype[][] batches, List<Result> results) {
    int batchIndex = 0;
    foreach (var batch in batches) {
      var batchFitness = new List<Result>(batch.Length);
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, batchFitness));
      batchIndex++;

      results.AddRange(batchFitness);
    }
  }

  IEnumerator EvaluatePopulation(CommonGenotype[] genotypes, List<Result> results) {
    // Create batches from the population
    var batches = CreateBatches(genotypes, batchSize);
    yield return StartCoroutine(EvaluateBatches(batches, results));
  }

  StreamWriter resultsLog;

  void OnApplicationQuit() {
    resultsLog.Close();
  }

  IEnumerator Start() {
    resultsLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/Logs/results.csv"));

    // Setup the algorithm with the proto-genotype
    algorithm = new EnforcedSubpopulations(new CommonGenotype(), subpopulationSize);

    while (true) {
      // Sample from the EA
      var population = algorithm.Sample(sampleSize);

      var results = new List<Result>(population.Length);
      yield return StartCoroutine(EvaluatePopulation(population, results));

      var best = results.Zip(population, (result, genotype) => Tuple.Of(result, genotype))
        .OrderBy(result => result.First.fitness)
        .First();

      // Update the EA's internals
      var updateResults = algorithm.Update(results.Select(r => r.fitness).ToArray());
      Debug.LogFormat("[{0}] Generation completed with {1}s and a fitness of {2}. ({3})", algorithm.Generation, best.First.duration, best.First.fitness, updateResults.First);

      resultsLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", algorithm.Generation, best.First.fitness, best.First.duration, best.First.orientation, best.Second.ToString()));
    }
  }
}

using UnityEngine;
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

  int batchSize = 25;
  int populationSize = 100;

  List<List<NEAT.Genotype>> CreateBatches(List<NEAT.Genotype> genotypes, int batchSize) {
    return genotypes.Select((gt, i) => {
      return new {
        Batch = Mathf.FloorToInt(i / batchSize),
        Genotype = gt,
      };
    }).GroupBy((gt) => {
      return gt.Batch;
    }).Select((grp) => {
      return grp.Select(g => g.Genotype).ToList();
    }).ToList();
  }

  IEnumerator EvaluateBatch(int batchIndex, List<NEAT.Genotype> batch, List<NEAT.Phenotype> phenotypes) {
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
      controllerBehaviour.networkIO = new NetworkIO(Reifier.Reify(genotype));

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluationBehaviour.genotype = genotype;
  
      evaluations.Add(evaluationBehaviour);

      i++;
    }

    // Wait for evaluations to complete
    while (!evaluations.All(ev => ev.IsComplete)) {
      yield return new WaitForFixedUpdate();
    }

    var batchPhenotypes = evaluations.Select(ev => {
      return new NEAT.Phenotype(ev.genotype, ev.Fitness, ev.Now, ev.Angle);
    }).ToList();

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

  IEnumerator EvaluateBatches(List<List<NEAT.Genotype>> batches, List<NEAT.Phenotype> phenotypes) {
    int batchIndex = 0;
    foreach (var batch in batches) {
      var batchPhenotypes = new List<NEAT.Phenotype>(batch.Count);

      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, batchPhenotypes));

      phenotypes.AddRange(batchPhenotypes);
      batchIndex++;
    }
  }

  IEnumerator EvaluatePopulation(List<NEAT.Genotype> genotypes, List<NEAT.Phenotype> phenotypes) {
    // Create batches from the population
    var batches = CreateBatches(genotypes, batchSize);
    yield return StartCoroutine(EvaluateBatches(batches, phenotypes));
  }

  StreamWriter resultsLog;

  void OnApplicationQuit() {
    resultsLog.Close();
  }

  IEnumerator Start() {
    resultsLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/Logs/phenotypes.csv"));

    var neat = new NEAT.NEAT(populationSize);

    while (true) {
      var genotypes = neat.Sample();

      var phenotypes = new List<NEAT.Phenotype>(genotypes.Count);
      yield return StartCoroutine(EvaluatePopulation(genotypes, phenotypes));

      var sorted = phenotypes.OrderBy(pt => pt.fitness);
      var best = sorted.First();

      // Update the EA's internals
      neat.Update(phenotypes);

      Debug.LogFormat("[{0}] Generation completed with {1}s and a fitness of {2}.", neat.Generation, best.duration, best.fitness);

      resultsLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", neat.Generation, best.fitness, best.duration, best.orientation, best.ToString()));
    }
  }
}

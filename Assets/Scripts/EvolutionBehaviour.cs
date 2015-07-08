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

  StreamWriter elitesLog;
  StreamWriter eliteFitnessLog;
  StreamWriter speciesLog;

  void LogResult(NEAT.Result result) {
    Debug.LogFormat("[{0}] Generation completed. Best Duration: {1}s. Best Fitness: {2}.",
      result.generation, result.best.duration, result.best.fitness);

    foreach (var sp in result.spp) {
      speciesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}", result.generation, sp.speciesId, sp.Size, sp.AverageFitness));
      foreach (var pt in sp.elites) {
        eliteFitnessLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}", result.generation, sp.speciesId, pt.fitness, pt.adjustedFitness, pt.duration));
        elitesLog.WriteLine(string.Format("{0}, {1}, {2}, {3}, {4}, {5}", result.generation, sp.speciesId, pt.fitness, pt.adjustedFitness, pt.duration, pt.genotype.ToJSON()));
      }
    }
  }

  void OnApplicationQuit() {
    elitesLog.Close();
    eliteFitnessLog.Close();
    speciesLog.Close();
  }

  IEnumerator Start() {
    elitesLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/Logs/elites.csv"));
    eliteFitnessLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/Logs/elite_fitness.csv"));
    speciesLog = File.CreateText(AssetDatabase.GenerateUniqueAssetPath("Assets/Logs/species.csv"));

    var innovations = new NEAT.Innovations();
    var neuronGenes = Enumerable.Range(0, NetworkIO.inNeuronCount + NetworkIO.outNeuronCount)
      .Select(i => NEAT.NeuronGene.Random(innovations.GetInitialNeuronInnovationId(i)))
      .ToList();
    var protoGenotype = new NEAT.Genotype(neuronGenes, new List<NEAT.SynapseGene>());

    var engine = new NEAT.Builder()
      .Evaluation(EvaluatePopulation)
      .Results(LogResult)
      .Innovations(innovations)
      .ProtoGenotype(protoGenotype)
      .Population(500)
      .Elitism(0.25f)
      .Species(10, 30.0f, 0.2f)
      .Measurement(new NEAT.Measurement(3.0f, 3.0f, 2.0f))
      .Crossover(new NEAT.MultipointCrossover())
      .Mutators(new NEAT.IMutator[]{
        new NEAT.AddNeuronMutator(0.2f),
        new NEAT.AddSynapseMutator(0.2f),
        new NEAT.PerturbNeuronMutator(0.25f, 0.25f),
        new NEAT.PerturbSynapseMutator(0.25f, 0.25f, 0.2f),
        new NEAT.ReplaceNeuronMutator(0.15f),
        new NEAT.ReplaceSynapseMutator(0.15f, 0.2f),
      })
      .Build();

    yield return StartCoroutine(engine.Run(this));
  }
}

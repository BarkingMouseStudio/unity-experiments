using UnityEngine;
using UnityEngine.Assertions;
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

  public delegate void BestEvaluationEvent(EvaluationBehaviour bestEvaluation);
  public event BestEvaluationEvent BestEvaluation;

  IEnumerator EvaluateBatch(int batchIndex, Phenotype[] batch, Orientations orientation) {
    IList<EvaluationBehaviour> evaluations = new List<EvaluationBehaviour>();

    var layout = new TransformLayout(28.0f, 18.0f, batchSize, Mathf.FloorToInt(Mathf.Sqrt(batchSize)))
      .GetEnumerator();

    foreach (var phenotype in batch) {
      layout.MoveNext();

      var t = PoolManager.Pools["Evaluations"].Spawn(prefab, layout.Current, Quaternion.identity, transform);

      var controllerBehaviour = t.GetComponent<ControllerBehaviour>();
      controllerBehaviour.Network = NetworkPorts.FromGenotype(phenotype.Genotype);

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluationBehaviour.Phenotype = phenotype;
      evaluationBehaviour.BeginTrial(orientation, Time.time);

      evaluations.Add(evaluationBehaviour);
    }

    // Wait for evaluations to complete
    while (evaluations.Any(ev => !ev.IsComplete)) {
      if (BestEvaluation != null) {
        var ordered = evaluations.OrderByDescending(ev => ev.Phenotype.CurrentTrial.Fitness);
        var best = ordered.First();
        BestEvaluation(best);
      }
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
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, Orientations.Downward));
      batchIndex++;
    }
  }

  IEnumerator EvaluatePopulation(List<Phenotype> phenotypes) {
    yield return StartCoroutine(EvaluateBatches(phenotypes.Batch(batchSize)));
  }

  StreamWriter elitesLog;
  StreamWriter populationLog;
  StreamWriter generationLog;
  StreamWriter speciesLog;

  void OnApplicationQuit() {
    elitesLog.Close();
    populationLog.Close();
    generationLog.Close();
    speciesLog.Close();
  }

  IEnumerator Start() {
    var logPath = string.Format("logs_{0}", DateTime.Now.Ticks);
    Debug.LogFormat("Logging to {0}", logPath);

    if (!Directory.Exists(logPath)) {
      Directory.CreateDirectory(logPath);
    }

    elitesLog = File.CreateText(Path.Combine(logPath, "elites.csv"));
    populationLog = File.CreateText(Path.Combine(logPath, "populations.csv"));
    generationLog = File.CreateText(Path.Combine(logPath, "generations.csv"));
    speciesLog = File.CreateText(Path.Combine(logPath, "species.csv"));

    var populationSize = 100;
    var innovations = new InnovationCollection();

    var mutations = new MutationCollection();
    mutations.Add(0.005f, new AddNeuronMutator(innovations)); // 0.1%
    mutations.Add(0.01f, new AddSynapseMutator(innovations)); // 1%
    mutations.Add(0.01f, new ToggleSynapseMutator(0.125f));
    mutations.Add(0.20f, new PerturbNeuronMutator(0.5f, 0.25f)); // 98% vvv
    mutations.Add(0.20f, new PerturbSynapseMutator(0.5f, 0.25f));
    mutations.Add(0.20f, new ReplaceNeuronMutator(0.5f));
    mutations.Add(0.20f, new ReplaceSynapseMutator(0.5f));

    var eliteSelector = new EliteSelector();

    var crossover = new MultipointCrossover();
    var offspringSelector = new OffspringSelector(crossover);

    var distanceMetric = new DistanceMetric(2.0f, 2.0f, 1.0f);
    var speciation = new Speciation(10, 6.0f, 0.3f, distanceMetric);

    var neuronGenes = new []{
      new NeuronGene(innovations.GetInitialNeuronInnovationId(0), NeuronType.UpperNeuron),
      new NeuronGene(innovations.GetInitialNeuronInnovationId(1), NeuronType.LowerNeuron),
      new NeuronGene(innovations.GetInitialNeuronInnovationId(2), NeuronType.PositionNeuron),
      new NeuronGene(innovations.GetInitialNeuronInnovationId(3), NeuronType.SpeedNeuron),
    }.ToGeneList();
    var synapseGenes = new GeneList<SynapseGene>();
    var protoGenotype = new Genotype(neuronGenes, synapseGenes);

    var genotypes = new GenotypeStream(protoGenotype)
      .Take(populationSize).ToArray();

    var species = new Specie[0];
    var generation = 0;

    var elitePhenotypes = new List<Phenotype>();
    var offspringPhenotypes = genotypes.Select(gt => new Phenotype(gt)).ToList();

    while (true) {
      yield return StartCoroutine(EvaluatePopulation(offspringPhenotypes));

      var phenotypes = new List<Phenotype>(elitePhenotypes.Count + offspringPhenotypes.Count);
      phenotypes.AddRange(elitePhenotypes);
      phenotypes.AddRange(offspringPhenotypes);

      Assert.AreEqual(phenotypes.Count, populationSize,
        "Population size must remain constant");

      var longest = phenotypes.OrderByDescending(pt => pt.BestDuration).First();
      Debug.LogFormat("[{0}] Fitness: {1}, Duration: {2}s ({3}, {4}) (Longest)",
        generation, longest.MeanFitness, longest.MeanDuration,
        longest.Genotype.NeuronCount,
        longest.Genotype.SynapseCount);

      var best = phenotypes.OrderByDescending(pt => pt.Fitness).First();
      Debug.LogFormat("[{0}] Fitness: {1}, Duration: {2}s ({3}, {4}) (Best)",
        generation, best.MeanFitness, best.MeanDuration,
        best.Genotype.NeuronCount,
        best.Genotype.SynapseCount);

      elitesLog.WriteLine(string.Join(",", new string[]{
        generation.ToString(),
        best.Fitness.ToString(),
        best.BestDuration.ToString(),
        JSON.Serialize(best.Genotype.ToJSON()),
      }));

      var meanComplexity = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + (float)pt.Genotype.Complexity,
        (sum) => sum / (float)phenotypes.Count);

      var meanNeuronCount = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + (float)pt.Genotype.NeuronCount,
        (sum) => sum / (float)phenotypes.Count);

      var meanSynapseCount = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + (float)pt.Genotype.SynapseCount,
        (sum) => sum / (float)phenotypes.Count);

      var meanFitness = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + (float)pt.Fitness,
        (sum) => sum / (float)phenotypes.Count);

      var stdevFitness = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + Mathf.Pow(pt.Fitness - meanFitness, 2.0f),
        (sum) => Mathf.Sqrt(sum / (float)phenotypes.Count));

      var stdevComplexity = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + Mathf.Pow((float)pt.Genotype.Complexity - meanComplexity, 2.0f),
        (sum) => Mathf.Sqrt(sum / (float)phenotypes.Count));

      var stdevNeuronCount = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + Mathf.Pow((float)pt.Genotype.NeuronCount - meanNeuronCount, 2.0f),
        (sum) => Mathf.Sqrt(sum / (float)phenotypes.Count));

      var stdevSynapseCount = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + Mathf.Pow((float)pt.Genotype.SynapseCount - meanSynapseCount, 2.0f),
        (sum) => Mathf.Sqrt(sum / (float)phenotypes.Count));

      species = speciation.Speciate(species, phenotypes.ToArray());

      Debug.LogFormat("[{0}] Species Count: {1} (Threshold: {2})",
        generation, species.Length, speciation.DistanceThreshold);

      var meanAdjustedFitness = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + (float)pt.AdjustedFitness,
        (sum) => sum / (float)phenotypes.Count);

      // standard deviation:
      // take the square root of the average of the squared deviations of the values from their average value
      var stdevAdjustedFitness = phenotypes.Aggregate(0.0f,
        (sum, pt) => sum + Mathf.Pow(pt.AdjustedFitness - meanAdjustedFitness, 2.0f),
        (sum) => Mathf.Sqrt(sum / (float)phenotypes.Count));

      generationLog.WriteLine(new []{
        generation,
        best.Fitness,
        best.MeanDuration,
        meanAdjustedFitness, stdevAdjustedFitness,
        meanFitness, stdevFitness,
        meanComplexity, stdevComplexity,
        meanNeuronCount, stdevNeuronCount,
        meanSynapseCount, stdevSynapseCount
      }.Stringify());

      foreach (var sp in species) {
        speciesLog.WriteLine(new []{
          generation,
          sp.SpeciesId, sp.Count,
          sp.BestFitness,
          sp.MeanFitness,
          sp.MeanAdjustedFitness,
          sp.MeanComplexity,
        }.Stringify());

        foreach (var pt in sp) {
          populationLog.WriteLine(new []{
            generation,
            sp.SpeciesId,
            pt.Fitness,
            pt.AdjustedFitness,
            pt.BestDuration,
            pt.Genotype.Complexity,
            pt.Genotype.NeuronCount,
            pt.Genotype.SynapseCount,
          }.Stringify());
        }
      }

      var eliteCount = species.Length;
      elitePhenotypes = eliteSelector.Select(species, eliteCount);

      var offspringCount = populationSize - elitePhenotypes.Count;
      var offspringGenotypes = offspringSelector.Select(species, offspringCount);

      var mutationResults = mutations.Mutate(offspringGenotypes);
      Debug.LogFormat("[{0}] Mutations: Added Neurons: {1}, Added Synapses: {2}, Perturbed Neurons: {3}, Perturbed Synapses: {4}, Replaced Neurons: {5}, Replaced Synapses: {6}, Toggled Synapses: {7}, Pruned Synapses: {8}, Orphaned Neurons: {9}",
        generation,
        mutationResults.addedNeurons,
        mutationResults.addedSynapses,
        mutationResults.perturbedNeurons,
        mutationResults.perturbedSynapses,
        mutationResults.replacedNeurons,
        mutationResults.replacedSynapses,
        mutationResults.toggledSynapses,
        mutationResults.prunedSynapses,
        mutationResults.orphanedNeurons);

      offspringPhenotypes = offspringGenotypes.Select(gt => new Phenotype(gt)).ToList();

      generation++;

      // Flush these so we can preview results while it runs
      generationLog.Flush();
      populationLog.Flush();
      speciesLog.Flush();
    }
  }
}

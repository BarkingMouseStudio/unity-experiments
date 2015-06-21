using UnityEngine;

public class EvolutionBehaviour : MonoBehaviour {

  public Transform prefab;

  int subpopulationSize = 100;
  int populationSize = 1000;
  int batchSize = 50;

  public float[][] NewGenotype() {
    var inNeurons = (12 * 2) + (6 * 2);
    var outNeurons = 8;
    var totalNeurons = inNeurons + outNeurons;
    var totalSynapses = inNeurons * outNeurons;

    var genotype = new List<float[]>(totalNeurons + totalSynapses);

    // subpopulation(s) for input neurons (a, b, c, d)
    // subpopulation(s) for output neurons (a, b, c, d)
    foreach (var _ in Enumerable.Range(0, totalNeurons)) {
      genotype.Add(
        new float[]{0.0f, 1.0f, 2.0f, 3.0f}
      );
    }

    // subpopulation(s) for synapses (w)
    foreach (var _ in Enumerable.Range(0, totalSynapses)) {
      genotype.Add(
        new float[]{0.0f}
      );
    }

    return genotype.ToArray();
  }

  Tuple<int, float[]>[][][] CreateBatches(Tuple<int, float[]>[][] genotypes, int batchSize) {
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

  IEnumerator EvaluateBatch(int batchIndex, Tuple<int, float[]>[][] batch, ControllerBehaviour.Orientations orientation, Tuple<int, float>[][] fitness) {
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
      controllerBehaviour.orientation = orientation;
      controllerBehaviour.UpdateOrientation();
      controllerBehaviour.SetGenotype(genotype);

      var evaluationBehaviour = t.GetComponent<EvaluationBehaviour>();
      evaluations.Add(evaluationBehaviour);

      i++;
    }

    // Wait for evaluations to complete
    while (!evaluations.All(ev => ev.isComplete)) {
      yield return new WaitForFixedUpdate();
    }

    // Accumulate fitnesses into array
    fitness.AddRange(evaluations.Select(ev => ev.Fitness));

    // Keep track of lowest fitness
    Debug.LogFormat("[{0}:{1}:{2}] Evaluations complete. ({3})", generation, batchIndex,
      orientation,
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

  IEnumerator EvaluateBatches(Tuple<int, float[]>[][][] batches, List<Tuple<int, float>[]> fitness) {
    var fitnessA = new List<float>(genotypes.Count);
    var fitnessB = new List<float>(genotypes.Count);

    // Setup fitness tests
    int batchIndex = 0;
    foreach (var batch in batches) {
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, ControllerBehaviour.Orientations.HardLeft, fitnessA));
      yield return StartCoroutine(EvaluateBatch(batchIndex, batch, ControllerBehaviour.Orientations.HardRight, fitnessB));
      batchIndex++;

      fitness = fitnessA.Skip(Math.Max(0, fitnessA.Count - batch.Count))
        .Zip(fitnessB.Skip(Math.Max(0, fitnessB.Count - batch.Count)),
          (a, b) => (a + b + Mathf.Abs(a - b)) / 3f)
        .ToList();

      Debug.LogFormat("[{0}:{1}] Batch complete. ({1})", subpopulations.generation, batchIndex, fitness.Min(f => f));
    }

    var zipped = fitnessA.Zip(fitnessB, (a, b) => {
      return (a + b + Mathf.Abs(a - b)) / 3f;
    });

    fitness.AddRange(zipped);
  }

  IEnumerator Start() {
    var genotype = NewGenotype();
    var subpopulations = new Subpopulations(genotype, subpopulationSize);

    var averageTrials = 0.0f;
    while (true) {
      var population = subpopulations.Sample(populationSize);
      var batches = CreateBatches(population, batchSize);

      List<Tuple<int, float>[]> fitness;
      yield return StartCoroutine(EvaluateBatches(batches, fitness));
      averageTrials = subpopulations.Evaluate(fitness);

      if (averageTrials > 10.0f) {
        var results = subpopulations.Recombine();
        Debug.LogFormat("[{0}] Generation complete. (Best: {1}, Offspring: {2}, Mutations: {3})", subpopulations.generation, fitness.Min(f => f), results.First, results.Second);
      } else {
        Debug.LogFormat("[{0}] Generation complete. (Best: {1})", subpopulations.generation, fitness.Min(f => f));
      }
    }
  }
}

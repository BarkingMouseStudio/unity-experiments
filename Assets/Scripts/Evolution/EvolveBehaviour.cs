using UnityEngine;
using NetMQ;
using NetMQ.zmq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PathologicalGames;

public class EvolveBehaviour : MonoBehaviour {

  public Transform prefab;

  readonly string address = "tcp://127.0.0.1:5556";

  NetMQContext context;
  NetMQSocket socket;

  int generation = 0;
  int batchSize = 50;

  IEnumerator EvaluateBatch(int batchIndex, List<List<List<double>>> batch, ControllerBehaviour.Orientations orientation, List<float> fitness) {
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
      // controllerBehaviour.SetGenotype(genotype);

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
    children = null;
  }

  IEnumerator Start() {
    context = NetMQContext.Create();

    socket = context.CreateResponseSocket();
    socket.Bind(address);

    while (true) {
      // Wait for next message
      IDictionary<string, object> message = new Dictionary<string, object>();
      yield return StartCoroutine(socket.WaitForResponse((response) => {
        message = (IDictionary<string, object>)JSON.Deserialize(response);
        Debug.LogFormat("[{0}] Received genotypes for evaluation.", generation);
      }));

      // Cast untyped message to appropriate types
      var id = message["id"];
      var genotypes = ((IList<object>)message["genotypes"])
        .Cast<IList<object>>()
        .Select(gt => {
          return gt.Cast<IList<object>>()
            .Select((ch) => ch.Cast<double>().ToList())
            .ToList();
        }).ToList();

      // Create batches
      var batches = genotypes.Select((gt, i) => {
        return new {
          Batch = Mathf.FloorToInt(i / batchSize),
          Genotype = gt,
        };
      }).GroupBy((gt) => {
        return gt.Batch;
      }).Select((grp) => {
        return grp.Select(g => g.Genotype).ToList();
      }).ToList();

      var fitnessA = new List<float>(genotypes.Count);
      var fitnessB = new List<float>(genotypes.Count);

      List<float> fitness;

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

        Debug.LogFormat("[{0}:{1}] Batch complete. ({1})", generation, batchIndex, fitness.Min(f => f));
      }

      fitness = fitnessA.Zip(fitnessB, (a, b) =>
        (a + b + Mathf.Abs(a - b)) / 3f).ToList();

      Debug.LogFormat("[{0}] Generation complete. ({1})", generation, fitness.Min(f => f));

      // Send back results
      socket.Send(JSON.Serialize(new Dictionary<string, object> {
        { "id", id },
        { "fitness", fitness }
      }));

      generation++;
    }
  }

  void OnApplicationQuit() {
    if (socket != null) {
      socket.Unbind(address);
    }
  }
}

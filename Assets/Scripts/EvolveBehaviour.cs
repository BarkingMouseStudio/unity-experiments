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
  protected internal float bestFitness = 0.0f;

  readonly string address = "tcp://127.0.0.1:5556";

  NetMQContext context;
  NetMQSocket socket;

  int generation = 0;

  IEnumerator Start() {
    context = NetMQContext.Create();

    socket = context.CreateResponseSocket();
    socket.Bind(address);

    while (true) {
      // Wait for next message
      Debug.LogFormat("[{0}] Waiting for fitness evaluation to begin...", generation);
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
      var batchSize = 100;
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

      List<float> fitness = new List<float>(genotypes.Count);

      float spacing = 24.0f;
      int count = batchSize;
      int stride = batchSize;

      float width = (count - 1) * spacing;
      Vector3 offset = new Vector3(-width / 2, 0, 0);

      // Setup fitness tests
      int batchIndex = 0;
      foreach (var batch in batches) {
        IList<DoublePendulumBehaviour> evaluations = new List<DoublePendulumBehaviour>();

        int i = 0;
        foreach (var genotype in batch) {
          float x = i % stride;

          Vector3 position = offset + new Vector3(x * spacing, 0, 0);
          Quaternion rotation = Quaternion.identity;

          var t = PoolManager.Pools["DoublePendulum"].Spawn(prefab, position, rotation, transform);

          var evaluationBehaviour = t.GetComponent<DoublePendulumBehaviour>();
          evaluationBehaviour.SetGenotype(genotype);
          evaluations.Add(evaluationBehaviour);

          i++;
        }

        // Wait for evaluations to complete
        Debug.LogFormat("[{0}:{1}] Waiting for evaluations to complete...", generation, batchIndex);
        while (!evaluations.All(ev => ev.isComplete)) {
          yield return new WaitForFixedUpdate();
        }
        Debug.LogFormat("[{0}:{1}] Evaluations complete.", generation, batchIndex);

        // Accumulate fitnesses into array
        fitness.AddRange(evaluations.Select(ev => ev.Fitness));

        // Keep track of lowest fitness
        bestFitness = Mathf.Min(bestFitness, evaluations.Min(ev => ev.Fitness));

        // Cleanup
        List<Transform> children = new List<Transform>(transform.childCount);
        foreach (Transform child in transform) {
          if (child.gameObject.activeInHierarchy) {
            children.Add(child);
          }
        }
        foreach (Transform child in children) {
          PoolManager.Pools["DoublePendulum"].Despawn(child, null);
        }
        children = null;

        batchIndex++;
      }

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

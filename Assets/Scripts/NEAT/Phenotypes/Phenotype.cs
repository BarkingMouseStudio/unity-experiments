using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class Phenotype {

    private readonly Genotype genotype;
    private readonly float fitness;
    private readonly float duration;
    private readonly float orientation;

    public Genotype Genotype {
      get {
        return genotype;
      }
    }

    public float Fitness {
      get {
        return fitness;
      }
    }

    public float Duration {
      get {
        return duration;
      }
    }

    public float Orientation {
      get {
        return orientation;
      }
    }

    public float AdjustedFitness { get; set; }

    public Phenotype(Genotype genotype, float fitness, float duration, float orientation) {
      this.genotype = genotype;
      this.fitness = fitness;
      this.duration = duration;
      this.orientation = orientation;
    }
  }
}

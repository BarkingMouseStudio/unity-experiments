using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public class Phenotype {

    public Genotype genotype;
    public float fitness;
    public float duration;
    public float orientation;

    public float adjustedFitness;

    public Phenotype(Genotype genotype, float fitness, float duration, float orientation) {
      this.genotype = genotype;
      this.fitness = fitness;
      this.duration = duration;
      this.orientation = orientation;
    }
  }
}

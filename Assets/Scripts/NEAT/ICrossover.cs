using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NEAT {

  public interface ICrossover {

    Genotype Crossover(Phenotype a, Phenotype b);
  }
}
